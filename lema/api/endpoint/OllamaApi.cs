using System.Text;
using System.Text.Json;

public static class OllamaApi
{
    public static void MapOllamaApi(this IEndpointRouteBuilder app)
    {
        app.MapPost("/ask", async (QuestionRequestQwen request, IHttpClientFactory httpClientFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest("La domanda non può essere vuota");
            }

            var httpClient = httpClientFactory.CreateClient("OllamaClient");

            try
            {
                // Prepara la richiesta per Ollama
                var ollamaRequest = new OllamaRequest(
                    model: request.Model,
                    prompt: request.Question,
                    stream: false
                );

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
                        question = request.Question,
                        answer = responseText.GetString(),
                        model = request.Model,
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
        });

        /// Verifica lo stato di Ollama
        app.MapGet("/ollama/status", async (IHttpClientFactory httpClientFactory) =>
                {
                    var httpClient = httpClientFactory.CreateClient("OllamaClient");

                    try
                    {
                        var response = await httpClient.GetAsync("api/tags");
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            return Results.Ok(new { status = "connected", models = JsonSerializer.Deserialize<JsonElement>(content) });
                        }

                        return Results.Problem($"Ollama non raggiungibile: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem($"Ollama non disponibile: {ex.Message}");
                    }
                });

        app.MapGet("/ollama/models", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("OllamaClient");

    try
    {
        var response = await httpClient.GetAsync("api/tags");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Results.Ok(JsonSerializer.Deserialize<JsonElement>(content));
        }

        return Results.Problem($"Impossibile recuperare i modelli: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Errore nel recupero modelli: {ex.Message}");
    }
});
    }


}