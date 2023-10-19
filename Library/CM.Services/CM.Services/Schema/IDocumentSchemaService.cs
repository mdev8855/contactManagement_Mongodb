using CM.Core.Domain;
using CM.Data.Base;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CM.Services.Schema
{

    public partial interface IDocumentSchemaService
    {
        Task InsertAsync(DocumentSchema client);
        Task UpdateAsync(DocumentSchema client);
        Task DeleteAsync(string id);
        Task<List<DocumentSchema>> ListAllAsync();
        Task<List<DocumentSchema>> ListAsync(string collectionName, string? fieldName = null);
        Task<DocumentSchema> GetAsync(string id);
    }

    public partial class DocumentSchemaService : IDocumentSchemaService
    {
        #region Fields
        private readonly IMongoRepoistory<DocumentSchema> _repository;
        #endregion

        #region Ctor

        public DocumentSchemaService(IMongoRepoistory<DocumentSchema> repository)
        {
            _repository = repository;
        }
        #endregion

        #region Method
        public async Task InsertAsync(DocumentSchema documentSchema)
        {
            await ValidateSchema(documentSchema);

            await _repository.InsertAsync(documentSchema);
        }

        public async Task UpdateAsync(DocumentSchema documentSchema)
        {
            var entity = await GetAsync(documentSchema.Id);

            if (entity == null)
                throw new NullReferenceException(nameof(entity));

            await ValidateSchema(documentSchema);

            entity = documentSchema;

            await _repository.UpdateAsync(entity);

        }

        public async Task DeleteAsync(string id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<DocumentSchema> GetAsync(string id)
        {
            return await _repository.FindAsync(id);
        }

        public async Task<List<DocumentSchema>> ListAllAsync()
        {
            return await _repository.ListAllAsync();
        }

        public async Task<List<DocumentSchema>> ListAsync(string collectionName, string? fieldName = null)
        {
            var filter = Builders<DocumentSchema>.Filter.Empty;
            filter = Builders<DocumentSchema>.Filter.Eq(x => x.CollectionName, collectionName);
            if (!string.IsNullOrEmpty(fieldName))
                filter &= Builders<DocumentSchema>.Filter.Eq(x => x.CollectionName, collectionName);

            return await _repository.ListAsync(filter);
        }

        public async Task<bool> IsExists(string collectionName, string fieldName, string? id = null)
        {
            var filter = Builders<DocumentSchema>.Filter.Eq(t => t.CollectionName, collectionName.Trim());
            filter &= Builders<DocumentSchema>.Filter.Eq(t => t.FieldName, fieldName.Trim());

            if (!string.IsNullOrEmpty(id))
            {
                ObjectId.TryParse(id, out ObjectId objectId);
                var bsonId = new BsonObjectId(objectId);
                filter = filter & Builders<DocumentSchema>.Filter.Ne("_id", bsonId);
            }

            return await _repository.AnyAsync(filter);

        }

        private async Task ValidateSchema(DocumentSchema documentSchema)
        {
            var collectionType = Type.GetType($"CM.Core.Domain.{documentSchema.CollectionName}, CM.Core");
            if (collectionType is null)
                throw new Exception($" collection {documentSchema.CollectionName} not found.!");

            var excludeChar = @"/\""*<>:|?".ToCharArray();
            if (documentSchema.FieldName.Any(x => excludeChar.Contains(x)))
                throw new Exception($"field {documentSchema.FieldName} use invalid charactar.!");

            var collectionCommonFields = collectionType?.GetProperties().Select(x => x.Name).ToList();
            if (collectionCommonFields != null && collectionCommonFields.Any(x => x.ToLower() == documentSchema.FieldName.ToLower()))
                throw new Exception($"field {documentSchema.FieldName} in collection {documentSchema.CollectionName} use reserved field name.!");

            var id = documentSchema?.Id ?? null;
            if (await IsExists(collectionName: documentSchema.CollectionName, fieldName: documentSchema.FieldName, id: id))
                throw new Exception($"field {documentSchema.FieldName} in collection {documentSchema.CollectionName} existed.!");
        }

        #endregion
    }
}
