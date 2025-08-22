namespace api.utils
{
    public class ResponseOllama
    {
        public string answer { get; set; }
        public DateTime timestamp { get; set; }
    }
    public interface IRequestOllama
    {
        Task<float[]> GetEmbeddingWithOllama(string question);
        Task<ResponseOllama> GetRequestWithRag(string prompt);
    }

    public class RequestOllama : IRequestOllama
    {
        private readonly IHttpClientFactory httpClientFactory;
        private HttpClient httpClient;
        private readonly IOptions<MyConst> myconst;


        public RequestOllama(IHttpClientFactory httpClientFactory, IOptions<MyConst> myconst)
        {
            this.httpClientFactory = httpClientFactory;
            this.httpClient = this.httpClientFactory.CreateClient("ollama");
            this.myconst = myconst;
        }

        public async Task<ResponseOllama> GetRequestWithRag(string prompt)
        {
            // Prepara la richiesta per Ollama
            var ollamaRequest = new
            {
                model = myconst.Value.gemma34b,
                prompt = prompt,
                stream = false
            };
            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/generate", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResponseOllama>(responseContent);
        }

        public async Task<float[]> GetEmbeddingWithOllama(string question)
        {
            var embeddingRequest = new
            {
                model = myconst.Value.nomicembedtext,
                prompt = $"{question}"
            };
            var json = JsonSerializer.Serialize(embeddingRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/api/embeddings", content);
            var embeddingResponse = await response.Content.ReadAsStringAsync();

            // 2. Estrai il vettore
            var embeddingDoc = JsonDocument.Parse(embeddingResponse);
            var vector = embeddingDoc.RootElement.GetProperty("embedding").EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
            return vector;
        }
    }
}

