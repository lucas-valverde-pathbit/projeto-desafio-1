using Microsoft.AspNetCore.Authentication.JwtBearer;
using Domain.Models; // Add this for models
using Infrastructure.Repositories; // Add this for repositories

using Domain.Models; // Add this for models
using Infrastructure.Repositories; // Add this for repositories

using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Domain.Services;
using Infrastructure.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Exibe logs no console
    .WriteTo.File("./logs/application.log", rollingInterval: RollingInterval.Day)  // Grava logs em arquivo, criando um novo arquivo a cada dia
    .CreateLogger();

// Usar o Serilog para o logging global
builder.Host.UseSerilog();

builder.Services.AddControllers();

// Registro dos serviços
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>(); // Add this line


// Registro dos repositórios
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>();
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IRepository<OrderItem>, OrderItemRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();


builder.Services.AddScoped<DatabaseSeeder>();

// Configurar o DbContext com PostgreSQL (ajuste a connection string conforme necessário)
try
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}
catch (Exception ex)
{
    Log.Error("Erro ao conectar ao banco de dados: {Message}", ex.Message);
    throw; // Re-throw the exception to stop the application startup
}


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.WebHost.UseUrls("http://*:5064"); // Configurar a aplicação para escutar na porta 5064

// Criar o aplicativo
var app = builder.Build(); // Declarar a variável app apenas uma vez


// Configuração de CORS
app.UseCors("AllowAll");

// Aplicar migrações ao banco de dados
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Aplicar migrações
}

// Popular o banco de dados com dados iniciais
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

// Swagger - Apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}

// Configuração do pipeline de requisição
app.UseRouting();

// Configuração para redirecionamento HTTPS
app.UseHttpsRedirection();

// Corrigindo o uso de rotas
app.MapControllers(); // Usando o novo método para mapear controladores

// Tratamento de erros globais
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        if (app.Environment.IsDevelopment())
        {
            await context.Response.WriteAsync("{\"message\":\"Ocorreu um erro interno no servidor.\"}");
        }
        else
        {
            await context.Response.WriteAsync("{\"message\":\"Ocorreu um erro. Tente novamente mais tarde.\"}");
        }
    });
});

app.Run();