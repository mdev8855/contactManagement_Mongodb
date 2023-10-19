using CM.Core.Domain;
using CM.Data.Base;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CM.Services.Companies
{
    public partial interface ICompanyService
    {
        Task InsertAsync(Company client);
        Task UpdateAsync(Company client);
        Task DeleteAsync(string id);
        Task<List<Company>> ListAllAsync();
        Task<List<Company>> ListAsync(string name = "", Dictionary<string, object>? dynamicFields = null);
        Task<List<Company>> ListAllAsyncByIds(List<string> Ids);
        Task<Company> GetAsync(string id);

    }
    public partial class CompanyService : ICompanyService
    {
        #region Fields
        private readonly IMongoRepoistory<Company> _repository;
        #endregion

        #region Ctor

        public CompanyService(IMongoRepoistory<Company> repository)
        {
            _repository = repository;
        }
        #endregion

        #region Method
        public async Task InsertAsync(Company company)
        {
            if (await IsExists(company.Name))
                throw new ArgumentNullException(nameof(company.Name));

            await _repository.InsertAsync(company);
        }

        public async Task UpdateAsync(Company company)
        {
            var entity = await GetAsync(company.Id);

            if (entity == null)
                throw new NullReferenceException(nameof(entity));

            if (await IsExists(company.Name, company.Id))
                throw new NullReferenceException(nameof(entity));

            entity = company;

            await _repository.UpdateAsync(entity);

        }

        public async Task DeleteAsync(string id)
        {
            //var related = await CheciIfRelated(id);
            //if (related)
            //    throw new NullReferenceException(nameof(related));

            await _repository.DeleteAsync(id);


        }

        public async Task<Company> GetAsync(string id)
        {
            return await _repository.FindAsync(id);
        }

        public async Task<List<Company>> ListAllAsync()
        {
            return await _repository.ListAllAsync();
        }

        public async Task<List<Company>> ListAsync(string name = "", Dictionary<string, object>? dynamicFields = null)
        {

            var filterList = new List<FilterDefinition<Company>>();
            var filterBuilder = Builders<Company>.Filter;
            FilterDefinition<Company> fieldFilter;

            if (!string.IsNullOrEmpty(name))
            {
                fieldFilter = filterBuilder.Eq(x => x.Name, name);
                filterList.Add(fieldFilter);
            }


            if (dynamicFields is not null)
            {
                foreach (var kvp in dynamicFields)
                {
                    string fieldName = kvp.Key;
                    object fieldValue = kvp.Value;

                    if (DateTime.TryParse(fieldValue.ToString(), out var dateValue))
                        fieldFilter = filterBuilder.Eq(fieldName, dateValue);
                    else if (int.TryParse(fieldValue.ToString(), out var intValue))
                        fieldFilter = filterBuilder.Eq(fieldName, intValue);
                    else if (fieldValue.ToString() is string stringValue)
                        fieldFilter = filterBuilder.Eq(fieldName, stringValue);
                    else
                        continue;

                    filterList.Add(fieldFilter);
                }
            }

            if (!filterList.Any())
                filterList.Add(Builders<Company>.Filter.Empty);

            var finalFilter = filterBuilder.And(filterList);

            return await _repository.ListAsync(finalFilter);
        }

        public async Task<List<Company>> ListAllAsyncByIds(List<string> Ids)
        {
            var filter = Builders<Company>.Filter.Empty;
            if (Ids.Any())
            {
                var objectIdList = new List<ObjectId>();
                foreach (string id in Ids)
                    if (ObjectId.TryParse(id, out ObjectId objectId))
                        objectIdList.Add(objectId);

                var bsonArray = new BsonArray(objectIdList);
                filter = Builders<Company>.Filter.In("_id", bsonArray);
            }
            else
                return Enumerable.Empty<Company>().ToList();

            return await _repository.ListAsync(filter);
        }

        public async Task<bool> IsExists(string name, string? id = null)
        {
            var filter = Builders<Company>.Filter.Eq(t => t.Name, name.Trim().ToLower());

            if (!string.IsNullOrEmpty(id))
            {
                ObjectId.TryParse(id, out ObjectId objectId);
                var bsonId = new BsonObjectId(objectId);
                filter = filter & Builders<Company>.Filter.Ne("_id", bsonId);
            }

            return await _repository.AnyAsync(filter);

        }

        #endregion
    }
}
