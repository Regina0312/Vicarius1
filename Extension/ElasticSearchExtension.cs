using Nest;
using VicariusAssignment.Models;

namespace VicariusAssignment.Extension
{
    public static class ElasticSearchExtension
    {
        private static readonly IElasticClient _elasticClient;
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["ElasticSettings:baseUrl"];
            var index = configuration["ElasticSettings:defaultIndex"];
            var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("6b6a8c2ad2bc7b291a7363f7bb96a120b8de326914980c868c1c0bc6b3dc41fd").BasicAuthentication("elastic", "JbNb_unwrJy3W0OaZ07n").DefaultIndex(index);
            settings.EnableApiVersioningHeader();
            AddDefaultMappings(settings);
            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
            CreateIndex(client, index);
        }
        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.DefaultMappingFor<Document>(m => m.Ignore(d => d.Name));
        }
        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName, index => index.Map<Document>(x => x.AutoMap()));
        }

        public static IList<Document> GetSearch(string key)
        {
            var result = _elasticClient.SearchAsync<Document>(s => s.Query(q => q.QueryString(d => d.Query('*' + key + '*'))).Size(5000));
            var finalResult = result;
            var finalContent = finalResult.Result.Documents.ToList();
            return finalContent;
        }
    }
}
