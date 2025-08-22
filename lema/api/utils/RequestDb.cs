namespace api.utils
{
    public interface IRequestDb
    {
        Task InsertDocumentAsync(Document document);
        Task<List<Document>> GetAllDocumentAsync();
    }
    public class RequestDb : IRequestDb
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOptions<MyConst> options;

        private HttpClient httpClient;


        public RequestDb(IHttpClientFactory httpClientFactory, IOptions<MyConst> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.options = options;
            this.httpClient = httpClientFactory.CreateClient("postgres");
        }

        public async Task InsertDocumentAsync(Document document)
        {
            var jsonContent = JsonSerializer.Serialize(document);
            var documentJson = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(options.Value.InsertDocument, documentJson);
        }
        public async Task<List<Document>> GetAllDocumentAsync()
        {

            var documents = await httpClient.GetFromJsonAsync<List<Document>>(options.Value.GetDocument);

            return documents;
        }

    }
}