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
using System.Text.Json; // Adicionando a diretiva de namespace para ReferenceHandler

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuração do banco de dados
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Serviços de aplicação
            builder.Services.AddScoped<Domain.Services.IUserService, Infrastructure.Services.UserService>();
            builder.Services.AddScoped<Domain.Services.IProductService, Infrastructure.Services.ProductService>();
            builder.Services.AddScoped<Domain.Services.ICustomerService, Infrastructure.Services.CustomerService>();
            builder.Services.AddScoped<Domain.Services.IOrderService, Infrastructure.Services.OrderService>();

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

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
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
            app.MapGet("/", () => Results.Redirect("/html/home.html")); 
            app.MapControllers();

            // Iniciar o servidor
            await app.RunAsync();
        }
    }
}
