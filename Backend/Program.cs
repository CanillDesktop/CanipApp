// Program.cs corrigido
// -------------------------------------------------------------
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Backend.Context;
using Backend.Models.Usuarios;
using Backend.Repositories;
using Backend.Repositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Shared.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Backend
{
    public class Program
    {
        private static string? _discoveryFilePath;
        private static int _assignedPort;
        private static int _currentPid;

        public static void Main(string[] args)
        {
            var logsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CanilApp", "logs");
            Directory.CreateDirectory(logsPath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(logsPath, "backend-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("🚀 Iniciando CanilApp Backend...");

                var builder = WebApplication.CreateBuilder(args);

                builder.WebHost.UseKestrel();

                if (!args.Any(a => a.StartsWith("--urls", StringComparison.OrdinalIgnoreCase)))
                {
                    builder.WebHost.UseUrls("http://127.0.0.1:0");
                    Log.Information("⚙️ Porta dinâmica configurada (http://127.0.0.1:0)");
                }

                builder.Host.UseSerilog();

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();

                // CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("LocalhostOnly", policy =>
                    {
                        policy.SetIsOriginAllowed(origin =>
                        {
                            if (string.IsNullOrEmpty(origin)) return false;

                            var uri = new Uri(origin);
                            return uri.Host == "localhost" || uri.Host == "127.0.0.1" ||
                                   uri.Host.StartsWith("192.168.") || uri.Host.StartsWith("10.") || uri.Host == "::1";
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });

                // Swagger
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CanilApp API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // DB
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var canilAppPath = Path.Combine(appDataPath, "CanilApp");
                Directory.CreateDirectory(canilAppPath);
                var dbPath = Path.Combine(canilAppPath, "canilapp.db");

                builder.Services.AddDbContext<CanilAppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

                // Auth
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "backend",
                            ValidAudience = "CanilApp",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave_simetrica_de_teste_validacao"))
                        };
                    });

                // Rate Limiter
                builder.Services.AddRateLimiter(options =>
                {
                    options.AddFixedWindowLimiter("sync-policy", opt =>
                    {
                        opt.PermitLimit = 1;
                        opt.Window = TimeSpan.FromSeconds(30);
                        opt.QueueLimit = 0;
                    });
                });

                // Services & Repositories
                builder.Services.AddScoped<IMedicamentosRepository, MedicamentosRepository>();
                builder.Services.AddScoped<IMedicamentosService, MedicamentosService>();

                builder.Services.AddScoped<IProdutosRepository, ProdutosRepository>();
                builder.Services.AddScoped<IProdutosService, ProdutosService>();

                builder.Services.AddScoped<IUsuariosRepository<UsuariosModel>, UsuariosRepository>();
                builder.Services.AddScoped<IUsuariosService, UsuariosService>();

                builder.Services.AddScoped<IInsumosRepository, IInsumosModelRepository>();
                builder.Services.AddScoped<IInsumosService, InsumosService>();

                builder.Services.AddScoped<EstoqueItemService>();
                builder.Services.AddScoped<EstoqueItemRepository>();
                builder.Services.AddScoped<RetiradaEstoqueService>();
                builder.Services.AddScoped<RetiradaEstoqueRepository>();

                builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
                builder.Services.AddAWSService<IAmazonDynamoDB>();
                builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
                builder.Services.AddScoped<ISyncService, SyncService>();

                var app = builder.Build();

                // Migrations
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<CanilAppDbContext>();
                    try
                    {
                        db.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Erro ao aplicar migrations");
                        throw;
                    }
                }

                // Pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    app.UseDeveloperExceptionPage();
                }

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

                        var response = new ErrorResponse
                        {
                            Title = "Erro interno no servidor",
                            StatusCode = 500,
                            Message = exceptionHandler?.Error.Message ?? "Erro desconhecido"
                        };

                        Log.Error(exceptionHandler?.Error, "Erro não tratado");
                        await context.Response.WriteAsJsonAsync(response);
                    });
                });

                app.UseCors("LocalhostOnly");
                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                // Endpoints
                app.MapGet("/", () => new { status = "backend rodando", version = "1.0.0", timestamp = DateTime.UtcNow });
                app.MapGet("/api/health", () => "OK");

                // Shutdown
                app.MapPost("/internal/shutdown", async (IHostApplicationLifetime lifetime) =>
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500);

                        if (_discoveryFilePath != null && File.Exists(_discoveryFilePath))
                            File.Delete(_discoveryFilePath);

                        lifetime.StopApplication();
                    });

                    return Results.Ok(new { message = "Shutdown iniciado" });
                });

                _currentPid = Process.GetCurrentProcess().Id;

                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    var httpUrl = app.Urls.First(u => u.StartsWith("http://"));
                    _assignedPort = new Uri(httpUrl).Port;

                    var canilPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CanilApp");
                    Directory.CreateDirectory(canilPath);

                    _discoveryFilePath = Path.Combine(canilPath, "backend.json");

                    var discoveryInfo = new
                    {
                        port = _assignedPort,
                        pid = _currentPid,
                        startedAt = DateTime.UtcNow.ToString("o"),
                        version = "1.0.0",
                        url = httpUrl
                    };

                    var tempFile = _discoveryFilePath + ".tmp";
                    File.WriteAllText(tempFile, JsonSerializer.Serialize(discoveryInfo, new JsonSerializerOptions { WriteIndented = true }));

                    if (File.Exists(_discoveryFilePath)) File.Delete(_discoveryFilePath);
                    File.Move(tempFile, _discoveryFilePath);

                    Console.WriteLine($"BACKEND_URL:{httpUrl}");
                });

                app.Lifetime.ApplicationStopping.Register(() =>
                {
                    if (_discoveryFilePath != null && File.Exists(_discoveryFilePath))
                        File.Delete(_discoveryFilePath);
                });

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Backend falhou ao iniciar");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}