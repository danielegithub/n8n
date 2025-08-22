using Microsoft.AspNetCore.Http.HttpResults;

public static class DocumentApi
{
    public static IEndpointRouteBuilder MapDocumentApi(this IEndpointRouteBuilder app)
    {
        // POST endpoint per upload PDF
        app.MapPost("/InsertDocument", async (Document document, ApplicationDbContext dbContext) =>
        {
            if (document == null)
            {
                return Results.BadRequest("Nessun file caricato");
            }
            try
            {
                await dbContext.Documents.AddAsync(document);
                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Errore durante la lettura dei PDF o invio al webhook: {ex.Message}");
            }
        })
        .WithName("InsertDocument")
        .WithOpenApi().WithTags("Documents")
        .DisableAntiforgery(); // Necessario per file upload

        app.MapPost("/InsertMoreDocuments", async (List<Document> documents, ApplicationDbContext dbContext) =>
                {
                    if (documents == null)
                    {
                        return Results.BadRequest("Nessun file caricato");
                    }
                    if (documents.Count == 0)
                    {
                        return Results.BadRequest("Nessun file caricato");
                    }
                    try
                    {
                        await dbContext.Documents.AddRangeAsync(documents);
                        await dbContext.SaveChangesAsync();

                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem($"Errore durante la lettura dei PDF o invio al webhook: {ex.Message}");
                    }
                })
                .WithName("InsertMoreDocuments")
                .WithTags("Documents")
                .WithOpenApi()
                .DisableAntiforgery(); // Necessario per file upload

        app.MapGet("/GetDocument", async (ApplicationDbContext dbContext) =>
        {
            var documents = await dbContext.Documents.Where(item => item.IsChunked).ToListAsync();
            return Results.Ok(documents);
        }).WithTags("Documents");
        return app;
    }

}
