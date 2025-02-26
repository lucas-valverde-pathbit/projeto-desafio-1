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

app.MapGet("/", () => Results.Redirect("/html/home.html")); 


// Mapeia Razor Pages, se necessário
app.MapRazorPages();

// Iniciar a aplicação
app.Run();
