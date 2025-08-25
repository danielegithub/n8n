

namespace api.endpoint
{
    public static class FileApi
    {

        public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/CaricaPdf", async (IFormFileCollection files, IRequestDb requestDb, IRequestOllama requestOllama, IHttpClientFactory httpClientFactory, IOptions<MyConst> myconst) =>
            {
                if (files == null || !files.Any())
                {
                    return Results.BadRequest("Nessun file caricato");
                }

                if (files.Count > 10)
                {
                    return Results.BadRequest("Massimo 10 file consentiti");
                }

                var httpClient = httpClientFactory.CreateClient("n8n");
                var httpClientOllama = httpClientFactory.CreateClient("ollama");

                var results = new List<object>();
                foreach (var file in files)
                {
                    if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    var pdfContent = ExtractTextFromPdf(memoryStream.ToArray());
                    var fileData = new
                    {
                        title = file.FileName,
                        content = pdfContent,
                        category = "pdf-reader"
                    };
                    results.Add(fileData);
                    var chunks = IntelligentChunking.ChunkDocument(file.FileName, CleanContent(pdfContent));
                    foreach (var chunk in chunks)
                    {

                        var vector = await requestOllama.GetEmbeddingWithOllamaAsync($"{chunk.Title} {chunk.Content}");
                        var document = new api.model.Document
                        {
                            Title = chunk.Title,
                            OriginalTitle = chunk.OriginalTitle,
                            Content = chunk.Content,
                            Category = "pdf-reader",
                            Model = "nomic-embed-text",
                            ChunkIndex = chunk.ChunkIndex,
                            TotalChunks = chunk.TotalChunks,
                            IsChunked = chunk.IsChunked,
                            FileName = file.FileName,
                            MimeType = "application/pdf",
                            DocLength = chunk.Content?.Length
                        };
                        document.SetEmbeddingVector(vector);
                        await requestDb.InsertDocumentAsync(document);
                    }
                }
                return Results.Ok(new { files = results });

            })
            .WithName("UploadPdfs")
            .WithTags("File")
            .WithOpenApi()
            .DisableAntiforgery();
            return app;
        }


        private static string ExtractTextFromPdf(byte[] pdfBytes)
        {
            try
            {
                using var document = PdfDocument.Open(pdfBytes);
                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Errore nell'estrazione del testo dal PDF: {ex.Message}", ex);
            }
        }

        private static string CleanContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            return content
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}