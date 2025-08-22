
public static class ConversationHistoryApi
{

    public static IEndpointRouteBuilder MapConversationHistoryApi(this IEndpointRouteBuilder app)
    {
        // POST endpoint per upload PDF
        app.MapPost("/InsertConversationHistory", async (ConversationHistory conversationHistory, ApplicationDbContext dbContext) =>
        {
            if (conversationHistory == null)
            {
                return Results.BadRequest("Nessun file caricato");
            }
            try
            {
                await dbContext.ConversationHistory.AddAsync(conversationHistory);
                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Errore durante la lettura dei PDF o invio al webhook: {ex.Message}");
            }
        })
        .WithName("InsertConversationHistory").WithTags("ConversationHistory")
        .WithOpenApi();

        app.MapGet("/GetConversationHistory", async (ApplicationDbContext dbContext) =>
        {
            var conversation = await dbContext.ConversationHistory.Where(item => item.DeletedAt != null).ToListAsync();
            return Results.Ok(conversation);
        })
        .WithName("GetConversationHistory").WithTags("ConversationHistory")
        .WithOpenApi();
        return app;
    }
}