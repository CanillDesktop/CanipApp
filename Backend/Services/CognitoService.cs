using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;
using Shared.Models;
using Shared.DTOs;
using Shared.Enums;

namespace Backend.Services;

public interface ICognitoService
{
    Task<LoginResponseModel> AuthenticateAsync(string username, string password);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task<UsuarioResponseDTO> RegisterUserAsync(UsuarioRequestDTO dto);
    Task<ImmutableCredentials> GetTemporaryCredentialsAsync(string idToken);
}

public class CognitoService : ICognitoService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoProvider;
    private readonly IAmazonCognitoIdentity _cognitoIdentity;
    private readonly string _userPoolId;
    private readonly string _clientId;
    private readonly string _identityPoolId;
    private readonly string _region;
    private readonly ILogger<CognitoService> _logger;

    public CognitoService(IConfiguration configuration, ILogger<CognitoService> logger)
    {
        _region = configuration["AWS:Region"] ?? throw new ArgumentNullException("AWS:Region");
        _userPoolId = configuration["AWS:UserPoolId"] ?? throw new ArgumentNullException("AWS:UserPoolId");
        _clientId = configuration["AWS:ClientId"] ?? throw new ArgumentNullException("AWS:ClientId");
        _identityPoolId = configuration["AWS:IdentityPoolId"] ?? throw new ArgumentNullException("AWS:IdentityPoolId");

        var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region);
        _cognitoProvider = new AmazonCognitoIdentityProviderClient(regionEndpoint);
        _cognitoIdentity = new AmazonCognitoIdentityClient(regionEndpoint);
        _logger = logger;
    }

    public async Task<LoginResponseModel> AuthenticateAsync(string username, string password)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = _clientId,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", username },
                    { "PASSWORD", password }
                }
            };

            var response = await _cognitoProvider.InitiateAuthAsync(authRequest);

            if (response.AuthenticationResult == null)
                throw new UnauthorizedAccessException("Credenciais inválidas");

            var userRequest = new GetUserRequest
            {
                AccessToken = response.AuthenticationResult.AccessToken
            };

            var userResponse = await _cognitoProvider.GetUserAsync(userRequest);

            var email = userResponse.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? username;
            var nome = userResponse.UserAttributes.FirstOrDefault(a => a.Name == "name")?.Value ?? "Usuário";
            var permissaoStr = userResponse.UserAttributes.FirstOrDefault(a => a.Name == "custom:permissao")?.Value ?? "LEITURA";
            var userId = userResponse.UserAttributes.FirstOrDefault(a => a.Name == "sub")?.Value ?? Guid.NewGuid().ToString();

            if (!Enum.TryParse<PermissoesEnum>(permissaoStr, out var permissao))
                permissao = PermissoesEnum.LEITURA;

            _logger.LogInformation($"✅ Usuário autenticado: {email}");

            return new LoginResponseModel
            {
                Token = new TokenResponse
                {
                    AccessToken = response.AuthenticationResult.AccessToken,
                    RefreshToken = response.AuthenticationResult.RefreshToken,
                    IdToken = response.AuthenticationResult.IdToken,
                    ExpiresIn = (int)response.AuthenticationResult.ExpiresIn
                },
                Usuario = new UsuarioResponseDTO
                {
                    Email = email,
                    Nome = nome.Split(' ')[0],
                    Sobrenome = nome.Contains(' ') ? nome.Substring(nome.IndexOf(' ') + 1) : "",
                    Permissao = permissao,
                    CognitoSub = userId
                }
            };
        }
        catch (Amazon.CognitoIdentity.Model.NotAuthorizedException ex)
        {
            _logger.LogWarning($"Autenticação falhou: {ex.Message}");
            throw new UnauthorizedAccessException("Usuário ou senha inválidos", ex);
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogWarning($"Usuário não encontrado: {ex.Message}");
            throw new UnauthorizedAccessException("Usuário não encontrado", ex);
        }
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                ClientId = _clientId,
                AuthParameters = new Dictionary<string, string>
            {
                { "REFRESH_TOKEN", refreshToken }
            }
            };

            // Essa chamada lança Amazon.CognitoIdentityProvider.Model.NotAuthorizedException se o token for ruim
            var response = await _cognitoProvider.InitiateAuthAsync(request);

            if (response.AuthenticationResult == null)
                throw new UnauthorizedAccessException("Falha ao obter resposta do Cognito.");

            _logger.LogInformation("✅ Token renovado com sucesso.");

            return new TokenResponse
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                // CORREÇÃO 1: Verifica se o Cognito mandou um refresh token NOVO. 
                // Se mandou, usa o novo. Se não (null), continua usando o atual.
                RefreshToken = response.AuthenticationResult.RefreshToken ?? refreshToken,
                IdToken = response.AuthenticationResult.IdToken,
                ExpiresIn = (int)response.AuthenticationResult.ExpiresIn
            };
        }
        // CORREÇÃO 2: Captura a exceção do namespace CORRETO (IdentityProvider, não Identity)
        catch (Amazon.CognitoIdentityProvider.Model.NotAuthorizedException ex)
        {
            _logger.LogWarning($"Refresh token rejeitado pela AWS: {ex.Message}");
            // Lança a exceção correta para o Controller retornar 401
            throw new UnauthorizedAccessException("Sessão expirada. Por favor, faça login novamente.", ex);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException ex)
        {
            _logger.LogWarning($"Usuário não existe mais: {ex.Message}");
            throw new UnauthorizedAccessException("Usuário inválido.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao renovar token.");
            throw;
        }
    }

    public async Task<UsuarioResponseDTO> RegisterUserAsync(UsuarioRequestDTO dto)
    {
        try
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = _clientId,
                Username = dto.Email,
                Password = dto.Senha,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = dto.Email },
                    new AttributeType { Name = "name", Value = $"{dto.Nome} {dto.Sobrenome}" },
                    new AttributeType { Name = "custom:permissao", Value = dto.Permissao.ToString() }
                }
            };

            var response = await _cognitoProvider.SignUpAsync(signUpRequest);

            var confirmRequest = new AdminConfirmSignUpRequest
            {
                UserPoolId = _userPoolId,
                Username = dto.Email
            };

            await _cognitoProvider.AdminConfirmSignUpAsync(confirmRequest);

            _logger.LogInformation($"✅ Usuário registrado: {dto.Email}");

            return new UsuarioResponseDTO
            {
                Email = dto.Email,
                Nome = dto.Nome,
                Sobrenome = dto.Sobrenome,
                Permissao = (PermissoesEnum)dto.Permissao,
                CognitoSub = response.UserSub
            };
        }
        catch (UsernameExistsException)
        {
            throw new InvalidOperationException("Usuário já existe");
        }
        catch (InvalidPasswordException ex)
        {
            throw new InvalidOperationException($"Senha inválida: {ex.Message}");
        }
    }

    public async Task<ImmutableCredentials> GetTemporaryCredentialsAsync(string idToken)
    {
        try
        {
            var getIdRequest = new GetIdRequest
            {
                IdentityPoolId = _identityPoolId,
                Logins = new Dictionary<string, string>
                {
                    { $"cognito-idp.{_region}.amazonaws.com/{_userPoolId}", idToken }
                }
            };

            var getIdResponse = await _cognitoIdentity.GetIdAsync(getIdRequest);

            var getCredsRequest = new GetCredentialsForIdentityRequest
            {
                IdentityId = getIdResponse.IdentityId,
                Logins = new Dictionary<string, string>
                {
                    { $"cognito-idp.{_region}.amazonaws.com/{_userPoolId}", idToken }
                }
            };

            var getCredsResponse = await _cognitoIdentity.GetCredentialsForIdentityAsync(getCredsRequest);

            var credentials = new ImmutableCredentials(
                getCredsResponse.Credentials.AccessKeyId,
                getCredsResponse.Credentials.SecretKey,
                getCredsResponse.Credentials.SessionToken
            );

            _logger.LogInformation($"✅ Credenciais temporárias obtidas");

            return credentials;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter credenciais temporárias");
            throw;
        }
    }
}