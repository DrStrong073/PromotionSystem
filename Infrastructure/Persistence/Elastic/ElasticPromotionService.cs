using Application.Interfaces;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Elastic
{
    public class ElasticPromotionService : IElasticSearchService<Promotion>
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "promotions_index";

        public ElasticPromotionService(ElasticsearchClient client) => _client = client;

        public async Task IndexAsync(Promotion document, CancellationToken ct = default)
            => await _client.IndexAsync(document, idx => idx.Index(IndexName).Id(document.Id), ct);

        public async Task BulkIndexAsync(IEnumerable<Promotion> documents, CancellationToken ct = default)
            => await _client.BulkAsync(b => b.Index(IndexName).IndexMany(documents, (d, i) => d.Id(i.Id)), ct);

        public async Task DeleteAsync(string id, CancellationToken ct = default)
            => await _client.DeleteAsync(IndexName, id, ct);

        public async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _client.SearchAsync<Promotion>(s => s
                .Indices(IndexName)
                .Size(1000)
                .Query(q => q.MatchAll(m => { })), ct);

            return response.Documents;
        }

        public Task<(IEnumerable<Promotion> Items, long Total)> SearchAsync(string? keyword, string? promotionId, int page, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException("Promotion không cần search phân trang phức tạp.");
        }
    }
}
