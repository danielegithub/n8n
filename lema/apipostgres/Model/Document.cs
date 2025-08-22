
using System.Text.Json;

[Table("documents")]
public class Document
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [MaxLength(255)]
    public string? Title { get; set; }

    [Column("original_title")]
    [MaxLength(255)]
    public string? OriginalTitle { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("category")]
    [MaxLength(100)]
    public string Category { get; set; } = "generale";

    [Column("model")]
    [MaxLength(100)]
    public string? Model { get; set; }

    [Column("embedding_vector", TypeName = "jsonb")]
    public string? EmbeddingVector { get; set; }

    [Column("chunk_index")]
    public int ChunkIndex { get; set; } = 0;

    [Column("total_chunks")]
    public int TotalChunks { get; set; } = 1;

    [Column("is_chunked")]
    public bool IsChunked { get; set; } = false;

    [Column("file_name")]
    [MaxLength(255)]
    public string? FileName { get; set; }

    [Column("file_path")]
    public string? FilePath { get; set; }

    [Column("mime_type")]
    [MaxLength(100)]
    public string? MimeType { get; set; }

    [Column("language")]
    [MaxLength(8)]
    public string? Language { get; set; }

    [Column("doc_length")]
    public int? DocLength { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Helper methods per gestire JSON
    public T? GetMetadata<T>() where T : class
    {
        if (string.IsNullOrEmpty(Metadata))
            return null;

        return JsonSerializer.Deserialize<T>(Metadata);
    }

    public void SetMetadata<T>(T data) where T : class
    {
        Metadata = JsonSerializer.Serialize(data);
    }

    public float[]? GetEmbeddingVector()
    {
        if (string.IsNullOrEmpty(EmbeddingVector))
            return null;

        return JsonSerializer.Deserialize<float[]>(EmbeddingVector);
    }

    public void SetEmbeddingVector(float[] vector)
    {
        EmbeddingVector = JsonSerializer.Serialize(vector);
    }
}