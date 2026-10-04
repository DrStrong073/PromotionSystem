using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMongoRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);

        // Dùng cho tính năng Import (Thêm nhiều voucher cùng lúc)
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        Task UpdateAsync(string id, T entity, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
