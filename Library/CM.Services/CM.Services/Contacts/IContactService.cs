using CM.Core.Domain;
using CM.Data.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CM.Core.Domain.Enum;

namespace CM.Services.Contacts
{
    public partial interface IContactService
    {
        Task InsertAsync(Contact client);
        Task UpdateAsync(Contact client);
        Task DeleteAsync(string id);
        Task<List<Contact>> ListAllAsync();
        Task<List<Contact>> ListAsync(string name = "", Dictionary<string, object>? dynamicFields = null);
        Task<Contact> GetAsync(string id);
    }

    public partial class ContactService : IContactService
    {
        #region Fields
        private readonly IMongoRepoistory<Contact> _repository;
        #endregion

        #region Ctor

        public ContactService(IMongoRepoistory<Contact> repository)
        {
            _repository = repository;
        }
        #endregion

        #region Method
        public async Task InsertAsync(Contact contact)
        {
            //if (await IsExists(contact.Name))
            //    throw new ArgumentNullException(nameof(contact.Name));

            await _repository.InsertAsync(contact);
        }

        public async Task UpdateAsync(Contact contact)
        {
            var entity = await GetAsync(contact.Id);

            if (entity == null)
                throw new NullReferenceException(nameof(entity));

            if (await IsExists(contact.Name, contact.Id))
                throw new NullReferenceException(nameof(entity));

            entity = contact;

            await _repository.UpdateAsync(entity);

        }

        public async Task DeleteAsync(string id)
        {
            //var related = await CheciIfRelated(id);
            //if (related)
            //    throw new NullReferenceException(nameof(related));

            await _repository.DeleteAsync(id);


        }

        public async Task<Contact> GetAsync(string id)
        {
            return await _repository.FindAsync(id);

        }
        public async Task<List<Contact>> ListAllAsync()
        {
            return await _repository.ListAllAsync();
        }
        public async Task<List<Contact>> ListAsync(string name = "", Dictionary<string, object>? dynamicFields = null)
        {

            var filterList = new List<FilterDefinition<Contact>>();
            var filterBuilder = Builders<Contact>.Filter;
            FilterDefinition<Contact> fieldFilter;

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
                filterList.Add(Builders<Contact>.Filter.Empty);
            
            var finalFilter = filterBuilder.And(filterList);

            return await _repository.ListAsync(finalFilter);
        }
                       
        public async Task<bool> IsExists(string name, string? id = null)
        {
            var filter = Builders<Contact>.Filter.Eq(t => t.Name, name.Trim().ToLower());

            if (!string.IsNullOrEmpty(id))
                filter = filter & Builders<Contact>.Filter.Ne(t => t.Id, id);

            return await _repository.AnyAsync(filter);

        }

        //private async Task<bool> CheciIfRelated(string id)
        //{

        //    var usersExited = _userManager.Users.Any(x => x.ContactesIds.Contains(id));

        //    var filter = Builders<Order>.Filter.Eq("ContactId", id);
        //    return await _orderRepo.AnyAsync(filter) && usersExited;
        //}

        #endregion
    }
}
