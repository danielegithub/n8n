

namespace api.endpoint
{
    public static class FileApi
    {

        private const string URL_ADD_PDF_PRODUCTION = "http://localhost:5678/webhook-test/add-document";

        public static IEndpointRouteBuilder MapPdfUploadEndpoints(this IEndpointRouteBuilder app)
        {
            // POST endpoint per upload PDF
            app.MapPost("/upload", async (IFormFileCollection files, IHttpClientFactory httpClientFactory) =>
            {
                if (files == null || !files.Any())
                {
                    return Results.BadRequest("Nessun file caricato");
                }

                if (files.Count > 10)
                {
                    return Results.BadRequest("Massimo 10 file consentiti");
                }

                var httpClient = httpClientFactory.CreateClient();
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

                        await httpClient.PostAsync(URL_ADD_PDF_PRODUCTION, content);
                    }

                    return Results.Ok(new { files = results });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Errore durante la lettura dei PDF o invio al webhook: {ex.Message}");
                }
            })
            .WithName("UploadPdfs")
            .WithOpenApi()
            .DisableAntiforgery(); // Necessario per file upload

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