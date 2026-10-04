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
    public class ElasticVoucherService : IElasticSearchService<Voucher>
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "vouchers_index";

        public ElasticVoucherService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task IndexAsync(Voucher document, CancellationToken ct = default)
        {
            await _client.IndexAsync(document, idx => idx.Index(IndexName).Id(document.Id), ct);
        }

        public async Task BulkIndexAsync(IEnumerable<Voucher> documents, CancellationToken ct = default)
        {
            await _client.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(documents, (descriptor, item) => descriptor.Id(item.Id)), ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            await _client.DeleteAsync(IndexName, id, ct);
        }

        public async Task<(IEnumerable<Voucher> Items, long Total)> SearchAsync(
        string? keyword,
        string? promotionId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        {
            var response = await _client.SearchAsync<Voucher>(s => s
                .Indices(IndexName)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Query(q =>
                {
                    if (string.IsNullOrWhiteSpace(keyword) && string.IsNullOrWhiteSpace(promotionId))
                    {
                        q.MatchAll(m => { }); 
                    }
                    else
                    {
                        q.Bool(b =>
                        {
                            if (!string.IsNullOrWhiteSpace(promotionId))
                            {    
                                b.Filter(f => f.Term(t => t.Field("promotionId.keyword").Value(promotionId)));
                            }

                            if (!string.IsNullOrWhiteSpace(keyword))
                            {
                                b.Must(m => m.Wildcard(w => w.Field("code.keyword").Value($"*{keyword.ToUpper()}*")));
                            }
                        });
                    }
                }), ct);


            if (!response.IsValidResponse)
            {
                Console.WriteLine($"\n ELASTICSEARCH QUERY ERROR:");
                Console.WriteLine(response.DebugInformation);
                Console.WriteLine("---------------------------\n");
            }
            // ========================================

            return (response.Documents, response.Total);
        }
        public async Task<IEnumerable<Voucher>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await SearchAsync(null, null, 1, 10000, ct);
            return result.Items;
        }
    }
}
