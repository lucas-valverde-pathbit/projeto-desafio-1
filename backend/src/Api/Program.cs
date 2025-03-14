using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Repositories; 
using Domain.Services;
using Infrastructure.Repositories;
using System.Text.Json;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

  
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IUserRepository, UserRepository>(); 
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>(); 
            // builder.Services.AddScoped<IProductRepository, ProductRepository>(); 
            // builder.Services.AddScoped<IOrderRepository, OrderRepository>(); 

            // Serviços de aplicação
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Configuração de autenticação JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Configuração de HttpClient
            builder.Services.AddHttpClient();

            // Configuração de CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:8080")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });

            // Adicionando o controlador e outros serviços
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve; // Adicionando suporte a ciclos
                });

            // Configuração do Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Aplicar migrações automaticamente durante a inicialização
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Obter o contexto do banco de dados
                    var context = services.GetRequiredService<AppDbContext>();
                    await context.Database.MigrateAsync(); // Aplica as migrações

                    // Opcional: Adicionar um Seeder, caso necessário
                    var seeder = new DatabaseSeeder();
                    await seeder.SeedAsync(context);
                    Console.WriteLine("Migrações aplicadas com sucesso e banco de dados seedado.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Erro ao aplicar migrações ou seed do banco de dados.");
                    throw;
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            // CORS deve vir antes de autenticação e autorização
            app.UseCors("CorsPolicy");

            // Configuração de Autenticação e Autorização
            app.UseAuthentication();
            app.UseAuthorization();

            // Habilitar arquivos estáticos
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });

            app.UseStaticFiles(); 

            // Redirecionar a rota raiz para index.html
            app.MapGet("/", () => Results.Redirect("/html/login.html")); 
            app.MapControllers();

            // Iniciar o servidor
            await app.RunAsync();
        }
    }
}
