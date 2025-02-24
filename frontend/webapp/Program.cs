var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Permitir servir arquivos estáticos
app.UseStaticFiles();

// Definir a rota da raiz para servir o arquivo login.html
app.MapGet("/", () => Results.Redirect("/html/login.html")); // Redireciona para login.html na raiz


// Mapeia Razor Pages, se necessário
app.MapRazorPages();

// Iniciar a aplicação
app.Run();
