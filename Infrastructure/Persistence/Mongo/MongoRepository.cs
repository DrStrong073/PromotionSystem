using Application.Interfaces;
using Domain.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Mongo
{
    public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _collection.InsertOneAsync(entity, cancellationToken: ct);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            await _collection.InsertManyAsync(entities, cancellationToken: ct);
        }

        public async Task UpdateAsync(string id, T entity, CancellationToken ct = default)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter, ct);
        }
    }
}
