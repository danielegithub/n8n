var builder = WebApplication.CreateBuilder(args);

// Forza l'ascolto su tutte le interfacce porta 8080
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddOpenApi();

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));


builder.Services.AddHttpClient("ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient("n8n", client =>
{
    client.BaseAddress = new Uri("http://localhost:5678/");
    client.Timeout = TimeSpan.FromMinutes(25);
});

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

app.MapDocumentApi();
app.MapConversationHistoryApi();

app.Run();
