

namespace api.endpoint
{
    public static class FileApi
    {

        public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
        {
            /*
                CARICA I PDF DEMANDANDO LA COMPETENZA AD N8n
            */
            app.MapPost("/CaricaPdfByN8n", async (IFormFileCollection files, IHttpClientFactory httpClientFactory, IOptions<MyConst> myconst) =>
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

                var results = new List<object>();

                try
                {
                    foreach (var file in files)
                    {
                        // Verifica che sia un PDF
                        if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Leggi il contenuto del PDF
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        var pdfContent = ExtractTextFromPdf(memoryStream.ToArray());

                        // Dati per la risposta
                        var fileData = new
                        {
                            title = file.FileName,
                            content = pdfContent,
                            category = "pdf-reader"
                        };

                        results.Add(fileData);

                        // Dati puliti per il webhook
                        var clearFileData = new
                        {
                            title = file.FileName,
                            content = CleanContent(pdfContent),
                            category = "pdf-reader"
                        };

                        // Invia al webhook
                        var jsonContent = JsonSerializer.Serialize(clearFileData);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        await httpClient.PostAsync(myconst.Value.AddPdf, content);
                    }

                    return Results.Ok(new { files = results });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Errore durante la lettura dei PDF o invio al webhook: {ex.Message}");
                }
            })
            .WithName("UploadPdfsN8n")
            .WithTags("File")
            .WithOpenApi()
            .DisableAntiforgery(); // Necessario per file upload

            /*
                CARICA I PDF FACENDO EMBEDDING IN AUTONOMIA
            */
            app.MapPost("/CaricaPdf", async (IFormFileCollection files, IHttpClientFactory httpClientFactory, IOptions<MyConst> myconst) =>
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
                var httpClientPostgres = httpClientFactory.CreateClient("postgres");
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
                        var embeddingRequest = new
                        {
                            model = myconst.Value.nomicembedtext,
                            prompt = $"{chunk.Title} {chunk.Content}"
                        };
                        var json = JsonSerializer.Serialize(embeddingRequest);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await httpClientOllama.PostAsync("/api/embeddings", content);
                        var embeddingResponse = await response.Content.ReadAsStringAsync();

                        // 2. Estrai il vettore
                        var embeddingDoc = JsonDocument.Parse(embeddingResponse);
                        var vector = embeddingDoc.RootElement.GetProperty("embedding").EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
                        var document = new Document
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
                        var jsonContent = JsonSerializer.Serialize(new { document });
                        var documentJson = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        await httpClientPostgres.PostAsync(myconst.Value.InsertDocument, documentJson);
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