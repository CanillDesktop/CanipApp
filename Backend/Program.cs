using Backend.Context;
using Backend.Models.Usuarios;
using Backend.Repositories;
using Backend.Repositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.DTOs;
using Shared.Models;
using System.Text;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API", Version = "v1" });

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

            builder.Services.AddDbContext<CanilAppDbContext>(options =>
                options.UseSqlite("Data Source=canilapp.db"));

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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

                    var response = new ErrorResponse
                    {
                        Title = "Erro interno no servidor",
                        StatusCode = 500,
                        Message = exceptionHandlerPathFeature?.Error.Message ?? "Erro interno no servidor"
                    };

                    await context.Response.WriteAsJsonAsync(response);
                });
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
