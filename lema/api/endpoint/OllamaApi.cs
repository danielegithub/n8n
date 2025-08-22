using Microsoft.Extensions.Options;

public static class OllamaApi
{


    public static void MapOllamaApi(this IEndpointRouteBuilder app)
    {
        app.MapPost("/AskOllama", async (string message, IHttpClientFactory httpClientFactory, IOptions<MyConst> settings) =>
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Results.BadRequest("La domanda non può essere vuota");
            }

            var httpClient = httpClientFactory.CreateClient("ollama");

            try
            {
                // Prepara la richiesta per Ollama
                var ollamaRequest = new
                {
                    model = settings.Value.gemma34b,
                    prompt = message,
                    stream = false
                };


                var jsonContent = JsonSerializer.Serialize(ollamaRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Invia la richiesta a Ollama
                var response = await httpClient.PostAsync("api/generate", content);

                if (!response.IsSuccessStatusCode)
                {
                    return Results.Problem($"Errore da Ollama: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }

                // Legge la risposta da Ollama
                var responseContent = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Estrae la risposta dal JSON di Ollama
                if (ollamaResponse.TryGetProperty("response", out var responseText))
                {
                    return Results.Ok(new
                    {
                        answer = responseText.GetString(),
                        timestamp = DateTime.UtcNow
                    });
                }

                return Results.Problem("Formato risposta non riconosciuto da Ollama");
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem($"Errore di connessione a Ollama: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return Results.Problem("Timeout nella richiesta a Ollama");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Errore interno: {ex.Message}");
            }
        }).WithTags("ollama");

    }


}