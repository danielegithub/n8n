
public class TodoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Descrizione { get; set; }

}



public record QuestionRequestQwen(string Question, string Model = "qwen3:0.6b");

public record OllamaRequest(string model, string prompt, bool stream = false);