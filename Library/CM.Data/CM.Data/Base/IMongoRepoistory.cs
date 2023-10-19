using CM.Core.Base;
using CM.Core.Domain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace CM.Data.Base
{
    public interface IMongoRepoistory<TEntity> where TEntity : IBaseDocument
    {
        IQueryable<TEntity> Table { get; }
        IMongoCollection<TEntity> Collection { get; }
        Task InsertAsync(TEntity entity);
        Task InsertRangeAsync(List<TEntity> entities);

        Task UpdateAsync(TEntity entity);
        //Task UpdateRangeAsync(List<TEntity> entities);

        Task DeleteAsync(string id);
        Task DeleteManyAsync(List<string> ids);
        Task<TEntity> FirstOrDefaultAsync();
        Task<TEntity> FirstOrDefaultAsync(BsonDocument filter);

        TEntity Find(string id);
        TEntity Find(Expression<Func<TEntity, bool>> filter);

        Task<TEntity> FindAsync(string id);
        Task<TEntity> FindAsync(FilterDefinition<TEntity> filter);
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter);

        Task<TEntity> Max(Expression<Func<TEntity, object>> expression);
        List<TEntity> ListAll();
        Task<List<TEntity>> ListAllAsync();
        //Task<PaginatedList<TEntity>> ListGridAsync(FilterDefinition<TEntity> filter = default, SortDefinition<TEntity> sort = default, int pageIndex = 0, int pageSize = 10);
        Task<List<TEntity>> ListAsync(FilterDefinition<TEntity> filter = default, SortDefinition<TEntity> sort = default);
        List<TEntity> List(Expression<Func<TEntity, bool>> filter);
        Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> filter);
        Task<List<TEntity>> ListAsync(FilterDefinition<TEntity> filter);

        Task<List<TEntity>> ListInAsync<TField>(Expression<Func<TEntity, TField>> expression, List<TField> array);

        Task<bool> AnyAsync();
        Task<bool> AnyAsync(FilterDefinition<TEntity> filter);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
    }

    public class MongoRepoistory<TEntity> : IMongoRepoistory<TEntity> where TEntity : IBaseDocument
    {
        private readonly IMongoDatabase db;
        private readonly string tableName;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IQueryable<TEntity> Table
        {
            get { return GetCollection(); }
        }
        public IMongoCollection<TEntity> Collection
        {
            get
            {
                return db.GetCollection<TEntity>(tableName);
            }
        }
        public MongoRepoistory(IConfiguration configuration,
            MongoDatabaseConfig mongoDbConfig,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var dbName = mongoDbConfig.Name;

            var client = new MongoClient();

            // this will create the database if not exists or retrieve the database if exists
            db = client.GetDatabase(dbName);
            tableName = typeof(TEntity).Name;
        }

        private IQueryable<TEntity> GetCollection()
        {
            return db.GetCollection<TEntity>(tableName).AsQueryable();

        }

        public async Task InsertAsync(TEntity entity)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            await collection.InsertOneAsync(entity);
        }

        public async Task InsertRangeAsync(List<TEntity> entities)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            await collection.InsertManyAsync(entities);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }

        //public async Task UpdateRangeAsync(List<TEntity> entities)
        //{
        //    var collection = db.GetCollection<TEntity>(tableName);

        //    await collection.UpdateManyAsync(entities);
        //}

        public async Task DeleteAsync(string id)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            await collection.DeleteOneAsync(t => t.Id == id);
        }
        public async Task DeleteManyAsync(List<string> ids)
        {
            var collection = db.GetCollection<TEntity>(tableName);
            await collection.DeleteManyAsync(t => ids.Contains(t.Id));
        }


        public async Task<TEntity> FirstOrDefaultAsync()
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(new BsonDocument())
                   .FirstOrDefaultAsync();
        }
        public async Task<TEntity> FirstOrDefaultAsync(BsonDocument filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .FirstOrDefaultAsync();
        }


        public TEntity Find(Expression<Func<TEntity, bool>> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return collection.Find(filter)
                   .FirstOrDefault();
        }
        public TEntity Find(string id)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return collection.Find(t => t.Id == id)
                   .FirstOrDefault();
        }

        public async Task<TEntity> FindAsync(string id)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(t => t.Id == id)
                   .FirstOrDefaultAsync();
        }

        public async Task<TEntity> FindAsync(FilterDefinition<TEntity> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .FirstOrDefaultAsync();
        }

        public async Task<TEntity> Max(Expression<Func<TEntity, object>> expression)
        {
            var options = new FindOptions<TEntity, TEntity>
            {
                Limit = 1,
                Sort = Builders<TEntity>.Sort.Descending(expression)
            };

            var collection = db.GetCollection<TEntity>(tableName);

            var find = await collection.Find(t => true).SortByDescending(expression).Limit(1).FirstOrDefaultAsync();

            return find;
        }
        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .FirstOrDefaultAsync();
        }


        public List<TEntity> ListAll()
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return collection.Find(new BsonDocument())
                   .ToList();
        }
        public async Task<List<TEntity>> ListAllAsync()
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(new BsonDocument())
                   .ToListAsync();
        }
        public async Task<List<TEntity>> ListAllProjectAsync()
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(new BsonDocument())
                   .ToListAsync();
        }
        public async Task<List<TEntity>> ListAsync(FilterDefinition<TEntity> filter = default, SortDefinition<TEntity> sort = default)
        {
            sort = sort ?? Builders<TEntity>.Sort.Descending(t => t.Id);
            filter = filter ?? Builders<TEntity>.Filter.Empty;

            var collection = db.GetCollection<TEntity>(tableName);

            var aggregation = await collection.Aggregate()
                                              .Match(filter)
                                              .ToListAsync();

            var data = aggregation.ToList();

            return data;
        }

        public async Task<List<TEntity>> ListGridAsync(FilterDefinition<TEntity> filter = default, SortDefinition<TEntity> sort = default, int pageIndex = 0, int pageSize = 10)
        {
            sort = sort ?? Builders<TEntity>.Sort.Descending(t => t.Id);
            filter = filter ?? Builders<TEntity>.Filter.Empty;

            if (pageIndex < 0)
                pageIndex = 0;

            if (pageSize < 1)
                pageSize = 10;

            var collection = db.GetCollection<TEntity>(tableName);

            var countFacet = GetCountFacet();

            var dataFacet = AggregateFacet.Create("data",
                    PipelineDefinition<TEntity, TEntity>.Create(new[]
                    {
                        PipelineStageDefinitionBuilder.Sort(sort),

                    }));


            var aggregation = await collection.Aggregate()
                                              .Match(filter)
                                              .Facet(countFacet, dataFacet)
                                              .ToListAsync();

            var count = aggregation.First()
                                    .Facets.First(x => x.Name == "count")
                                    .Output<AggregateCountResult>()
                                    ?.FirstOrDefault()
                                    ?.Count ?? 0;

            var data = aggregation.First()
                           .Facets.First(x => x.Name == "data")
                           .Output<TEntity>();

            return data.ToList();
        }


        public List<TEntity> List(Expression<Func<TEntity, bool>> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return collection.Find(filter)
                   .ToList();
        }
        public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .ToListAsync();
        }
        public async Task<List<TEntity>> ListAsync(FilterDefinition<TEntity> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .ToListAsync();
        }


        public async Task<List<TEntity>> ListInAsync<TField>(Expression<Func<TEntity, TField>> expression, List<TField> array)
        {
            var filter = Builders<TEntity>.Filter.AnyIn("_id", array);

            var collection = db.GetCollection<TEntity>(tableName);

            return await (await collection.FindAsync(filter))
                   .ToListAsync();

            //return await ListAsync(filter);
        }

        public async Task<bool> AnyAsync()
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(new BsonDocument())
                   .AnyAsync();
        }
        public async Task<bool> AnyAsync(FilterDefinition<TEntity> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .AnyAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
        {
            var collection = db.GetCollection<TEntity>(tableName);

            return await collection.Find(filter)
                   .AnyAsync();
        }



        private AggregateFacet<TEntity, AggregateCountResult> GetCountFacet()
        {
            var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<TEntity, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<TEntity>()
            }));

            return countFacet;
        }
    }
}
