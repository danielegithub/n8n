
public class RagProcessor
{
    // Calcola similarità coseno (ora usa float[] come la tua classe Document)
    private static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null || vectorA.Length != vectorB.Length)
            return 0;

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        double magnitude = Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB);
        return magnitude > 0 ? dotProduct / magnitude : 0;
    }

    public static string ProcessRAGQuery(List<Document> documents, float[] vector, string question)
    {
        Console.WriteLine($"Processando {documents.Count} documenti");

        // Calcola similarità per ogni documento
        var results = new List<(Document doc, double similarity)>();
        
        foreach (var doc in documents)
        {
            var embedding = doc.GetEmbeddingVector(); // Usa il metodo della tua classe
            if (embedding != null && embedding.Length == vector.Length)
            {
                var similarity = CosineSimilarity(vector, embedding);
                results.Add((doc, similarity));
            }
        }

        if (!results.Any())
        {
            return "Errore: Nessun documento con embedding valido trovato.";
        }

        // Ordina per similarità e prendi i top 5
        var topResults = results
            .OrderByDescending(r => r.similarity)
            .Take(5)
            .ToList();

        // Calcola soglia dinamica
        var avgSimilarity = results.Average(r => r.similarity);
        var threshold = Math.Max(0.2, avgSimilarity * 0.7);

        // Filtra per soglia, ma prendi almeno i top 2
        var finalResults = topResults.Where(r => r.similarity > threshold).ToList();
        if (!finalResults.Any())
        {
            finalResults = topResults.Take(2).ToList();
        }

        Console.WriteLine($"Top documents (soglia: {threshold:F3}):");
        foreach (var result in finalResults)
        {
            var chunkInfo = result.doc.IsChunked 
                ? $"{result.doc.ChunkIndex + 1}/{result.doc.TotalChunks}" 
                : "single";
            Console.WriteLine($"- {result.doc.Title} - Similarità: {result.similarity:F4} - Chunk: {chunkInfo}");
        }

        // Costruisci il contesto
        var contextParts = finalResults.Select((result, idx) =>
        {
            var doc = result.doc;
            var similarity = result.similarity;
            
            const int maxLength = 1500;
            var content = doc.Content?.Length > maxLength
                ? doc.Content.Substring(0, maxLength) + "..."
                : doc.Content;

            var chunkInfo = doc.IsChunked 
                ? $"Sezione: {doc.ChunkIndex + 1}/{doc.TotalChunks}" 
                : "";

            return $"[Documento {idx + 1}] {doc.Title}\n" +
                   $"Contenuto: {content}\n" +
                   $"Rilevanza: {similarity * 100:F1}%\n" +
                   (doc.IsChunked ? $"{chunkInfo}\n" : "") +
                   $"---";
        });

        var context = string.Join("\n", contextParts);

        var prompt = $"Contesto dai documenti più rilevanti ({finalResults.Count} trovati):\n" +
                    $"{context}\n\n" +
                    $"Domanda dell'utente: {question}\n\n" +
                    "Istruzioni:\n" +
                    "- Rispondi utilizzando SOLO le informazioni del contesto sopra\n" +
                    "- Se le informazioni non sono sufficienti, dillo chiaramente\n" +
                    "- Cita i documenti specifici quando possibile";

        return prompt;
    }
}