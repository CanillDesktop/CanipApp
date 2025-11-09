using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Backend.Models.Produtos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly IDynamoDBContext _dynamoDBContext;

    public SyncController(ISyncService syncService, IDynamoDBContext dynamoDBContext)
    {
        _syncService = syncService;
        _dynamoDBContext = dynamoDBContext;
    }

    [HttpPost]
    public async Task<IActionResult> Sincronizar()
    {
        try
        {
            await _syncService.SincronizarTabelasAsync();
            return Ok(new { message = "Sincronização concluída." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erro ao sincronizar: {ex.ToString()}" });
        }
    }

    [HttpPost("limpar")]
    public async Task<IActionResult> Limpar()
    {
        try
        {
            await _syncService.LimparRegistrosExcluidosAsync();
            return Ok(new { message = "Limpeza concluída." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erro ao limpar: {ex.Message}" });
        }
    }

    [HttpGet("test-dynamo")]
    public async Task<IActionResult> TestDynamo()
    {
        try
        {
            var client = new AmazonDynamoDBClient();
            var response = await client.ListTablesAsync();
            return Ok(response.TableNames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet("test-produto-schema")]
    public async Task<IActionResult> TestProdutoSchema()
    {
        try
        {
            // Create a test product
            var testProduto = new Produtos
            {
                IdProduto = $"TEST-{Guid.NewGuid()}",
                DescricaoSimples = "Test Product",
                Quantidade = 1,
                EstoqueDisponivel = 1,
                DataAtualizacao = DateTime.UtcNow,
                Unidade = Shared.Enums.UnidadeEnum.KG,
                Categoria = Shared.Enums.CategoriaEnum.DIVERSOS
            };

            // Try to save
            await _dynamoDBContext.SaveAsync(testProduto);

            // Try to load it back
            var loaded = await _dynamoDBContext.LoadAsync<Produtos>(testProduto.IdProduto);

            if (loaded != null)
            {
                // Clean up
                await _dynamoDBContext.DeleteAsync(testProduto);
                return Ok(new
                {
                    message = "✅ Success! Schema is correct.",
                    testedId = testProduto.IdProduto,
                    loaded = loaded
                });
            }

            return StatusCode(500, "Could not load saved item");
        }
        catch (AmazonDynamoDBException ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                errorCode = ex.ErrorCode,
                requestId = ex.RequestId,
                hint = "Check if table has a sort key that's not in your model"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                stack = ex.StackTrace,
                innerException = ex.InnerException?.Message
            });
        }
    }
}