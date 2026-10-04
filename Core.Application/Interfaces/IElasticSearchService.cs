using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IElasticSearchService<T> where T : class
    {
        // Đồng bộ 1 document sang ES khi Create/Update
        Task IndexAsync(T document, CancellationToken ct = default);

        // Đồng bộ hàng loạt sang ES (Dùng khi Import CSV)
        Task BulkIndexAsync(IEnumerable<T> documents, CancellationToken ct = default);

        Task DeleteAsync(string id, CancellationToken ct = default);

        Task<(IEnumerable<T> Items, long Total)> SearchAsync(
            string? keyword,
            string? promotionId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    }
}
