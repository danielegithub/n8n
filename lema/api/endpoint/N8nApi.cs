using Microsoft.AspNetCore.Http.HttpResults;

namespace api.endpoint
{
    public static class N8nApi
    {
        public static void MapN8nApi(this IEndpointRouteBuilder app)
        {
            app.MapPost("/AskN8n", async (string message, IHttpClientFactory httpClientFactory, IOptions<MyConst> myconst) =>
            {
                try
                {
                    var httpClient = httpClientFactory.CreateClient("n8n");

                    var jsonContent = JsonSerializer.Serialize(new { question = message });
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    // Invia la richiesta a Ollama
                    var response = await httpClient.PostAsync(myconst.Value.QuestionAI, content);

                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    return Results.Ok(jsonResponse);
                }
                catch (Exception ex)
                {
                    return Results.InternalServerError(ex.Message);
                }

            }).WithTags("n8n");
        }
    }
}