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
            builder.Services.AddScoped<Domain.Services.IOrderItemService, Infrastructure.Services.OrderItemService>();

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
            builder.Services.AddControllers();

            // Configuração do Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Migração e seed do banco de dados se solicitado
            if (args.Contains("--migrate"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<AppDbContext>();
                        await context.Database.MigrateAsync();
                        Console.WriteLine("Migrations applied successfully.");

                        // Executar o seeder
                        var seeder = new DatabaseSeeder();
                        await seeder.SeedAsync(context);
                        Console.WriteLine("Database seeded successfully.");
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                        throw;
                    }
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
            
            // Add explicit CORS headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:8080");
                context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                await next();
            });

            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Inicia o servidor
            await app.RunAsync();
        }
    }
}
