using System.Text.RegularExpressions;

public class DocumentChunk
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public bool IsChunked { get; set; }
    public string OriginalTitle { get; set; }
}

public class IntelligentChunking
{
    public static List<DocumentChunk> ChunkDocument(string title, string content, int maxChunkSize = 1000, int overlap = 200)
    {
        if (content.Length <= maxChunkSize)
        {
            return new List<DocumentChunk>
            {
                new DocumentChunk
                {
                    Title = title,
                    Content = content,
                    ChunkIndex = 0,
                    TotalChunks = 1,
                    IsChunked = false
                }
            };
        }

        var chunks = new List<DocumentChunk>();
        var sentences = SplitIntoSentences(content);
        
        string currentChunk = "";
        int chunkIndex = 0;

        foreach (var sentence in sentences)
        {
            string processedSentence = sentence.Trim() + ".";

            // Se aggiungendo questa frase superiamo la dimensione massima
            if (currentChunk.Length + processedSentence.Length > maxChunkSize && currentChunk.Length > 0)
            {
                // Salva il chunk corrente
                chunks.Add(new DocumentChunk
                {
                    Title = $"{title} - Parte {chunkIndex + 1}",
                    Content = currentChunk.Trim(),
                    ChunkIndex = chunkIndex,
                    TotalChunks = 0, // Sarà aggiornato dopo
                    IsChunked = true,
                    OriginalTitle = title
                });

                // Inizia nuovo chunk con overlap
                string overlapText = GetOverlapText(currentChunk, overlap);
                currentChunk = overlapText + processedSentence;
                chunkIndex++;
            }
            else
            {
                currentChunk += " " + processedSentence;
            }
        }

        // Aggiungi l'ultimo chunk se non vuoto
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(new DocumentChunk
            {
                Title = $"{title} - Parte {chunkIndex + 1}",
                Content = currentChunk.Trim(),
                ChunkIndex = chunkIndex,
                TotalChunks = 0,
                IsChunked = true,
                OriginalTitle = title
            });
        }

        // Aggiorna total_chunks
        foreach (var chunk in chunks)
        {
            chunk.TotalChunks = chunks.Count;
        }

        return chunks;
    }

    private static List<string> SplitIntoSentences(string content)
    {
        var sentences = Regex.Split(content, @"[.!?]+")
                           .Where(s => !string.IsNullOrWhiteSpace(s))
                           .ToList();
        return sentences;
    }

    private static string GetOverlapText(string text, int overlapSize)
    {
        if (text.Length <= overlapSize) 
            return text;

        // Trova l'ultimo punto prima della dimensione overlap
        string overlapText = text.Substring(text.Length - overlapSize);
        int lastPeriod = overlapText.LastIndexOf('.');

        if (lastPeriod > overlapSize / 2)
        {
            return overlapText.Substring(lastPeriod + 1).Trim() + " ";
        }

        return overlapText + " ";
    }
}
