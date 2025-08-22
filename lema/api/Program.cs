using api.endpoint;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Configurazione CORS per consentire richieste da frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<MyConst>(
    builder.Configuration.GetSection("MyConst"));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("PostgreSQL")
));

builder.Services.AddHttpClient("ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout lungo per le risposte AI
});

builder.Services.AddHttpClient("n8n", client =>
{
    client.BaseAddress = new Uri("http://localhost:5678/");
    client.Timeout = TimeSpan.FromMinutes(25); // Timeout lungo per le risposte AI
});

builder.Services.AddHttpClient("postgres", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/");
    client.Timeout = TimeSpan.FromMinutes(25); // Timeout lungo per le risposte AI
});

builder.Services.AddScoped<IRequestDb, RequestDb>();
builder.Services.AddScoped<IRequestOllama, RequestOllama>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiPostgres");
    c.RoutePrefix = string.Empty; // 👈 fa aprire Swagger direttamente su "/"
});

app.UseHttpsRedirection();

app.MapFileEndpoints();
app.MapN8nApi();
app.MapOllamaApi();

app.Run();
