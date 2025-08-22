using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

public static class OllamaApi
{
    public static void MapOllamaApi(this IEndpointRouteBuilder app)
    {
        app.MapPost("/AskOllamaRag", async (string question, IRequestDb requestDb, IRequestOllama requestOllama) =>
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return Results.BadRequest("La domanda non può essere vuota");
            }

            var vector = await requestOllama.GetEmbeddingWithOllamaAsync(question);
            var allDocuments = await requestDb.GetAllDocumentAsync();

            var prompt = RagProcessor.ProcessRAGQuery(allDocuments, vector, question);
            var response = await requestOllama.GetRequestWithRagAsync(question);
            try
            {
                return Results.Ok(response);
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