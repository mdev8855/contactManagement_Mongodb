using CM.API.Factories;
using CM.API.Models.ParameterModel;
using CM.Core.Domain;
using CM.Core.Domain.Enum;
using CM.Services.Companies;
using CM.Services.Contacts;
using CM.Services.Schema;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace CM.API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ContactController : ControllerBase
    {
        #region Fields
        private readonly IContactService _contactService;
        private readonly ICompanyService _companyService;
        private readonly IDtoFactory _dtoFactory;
        private readonly IDocumentSchemaService _documentSchemaService;
        #endregion

        #region Ctor
        public ContactController(IContactService contactService,
           ICompanyService companyService,
           IDtoFactory dtoFactory,
           IDocumentSchemaService documentSchemaService)
        {
            _companyService = companyService;
            _dtoFactory = dtoFactory;
            _documentSchemaService = documentSchemaService;
            _contactService = contactService;
        }
        #endregion

        #region Actions
        [HttpGet()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Contact), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(null);

            var data = await _contactService.GetAsync(id);

            if (data == null)
                return NotFound(null);

            var dto = _dtoFactory.PrepareContactDto(data);

            return Ok(dto);
        }


        /// <summary>
        /// Get all contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _contactService.ListAllAsync();
            if (data == null)
                return NotFound(null);

            var dto = data.Select(x => _dtoFactory.PrepareContactDto(x)).ToList();
            return Ok(dto);
        }

        /// <summary>
        /// Get all contacts filterd
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Search(ContactSearchModel model)
        {
            var data = await _contactService.ListAsync(model.Name, model.DynamicFields);
            var dto = data.Select(x => _dtoFactory.PrepareContactDto(x)).ToList();
            return Ok(dto);
        }

        /// <summary>
        /// Insert new contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(ContactAddUpdateModel model)
        {
            #region Validation

            if (model == null)
            { return BadRequest("model is null"); }

            if (string.IsNullOrEmpty(model.Name))
            { return BadRequest("name required.!"); }

            (bool result, BsonDocument? bsonDynamicElements, List<string> errors) = await ValidateDynamicFields(model);

            //handle companies
            List<string> relatedComapnyIds = new List<string>();
            if (model.Companies.Any())
            {
                var companies = await _companyService.ListAllAsyncByIds(model.Companies);
                if (companies.Any())
                    relatedComapnyIds.AddRange(companies.Select(x => x.Id).ToList());
            }

            #endregion
            //if (!result && errors.Any())
            //    return BadRequest(string.Join(", ", errors));

            //we can change the validation logic to add main or common properties even if dynamic field has some errors
            var entity = new Contact()
            {
                Name = model.Name,
                //DynamicFields = JObject.FromObject(new
                //{
                //    Age = 30,
                //    Email = "john.doe@example.com",
                //    Birthday = "1990-01-01T00:00:00Z",
                //    Description = "Description"
                //}),

                Companies = relatedComapnyIds,
            };

            if (result && bsonDynamicElements is not null)
                entity.DynamicFields = bsonDynamicElements;

            await _contactService.InsertAsync(entity);

            return Ok(string.Join(", ", errors));
        }

        /// <summary>
        /// Update exsiting contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(ContactAddUpdateModel model)
        {
            #region Validation
            if (model == null)
            { return BadRequest("model is null"); }

            if (string.IsNullOrEmpty(model.Name))
            { return BadRequest("name required.!"); }

            if (string.IsNullOrEmpty(model.Id))
            { return BadRequest("id required.!"); }

            //handle companies
            List<string> relatedComapnyIds = new List<string>();
            if (model.Companies.Any())
            {
                var companies = await _companyService.ListAllAsyncByIds(model.Companies);
                if (companies.Any())
                    relatedComapnyIds.AddRange(companies.Select(x => x.Id).ToList());
            }

            #endregion
            var dbEntity = await _contactService.GetAsync(model.Id);
            if (dbEntity is null)
                return NotFound();

            (bool result, BsonDocument? bsonDynamicElements, List<string> errors) = await ValidateDynamicFields(model);

            //we can change the validation logic to add main or common properties even if dynamic field has some errors
            dbEntity.Name = model.Name;
            dbEntity.Companies = relatedComapnyIds;


            if (result && bsonDynamicElements is not null)
                dbEntity.DynamicFields = bsonDynamicElements;

            await _contactService.UpdateAsync(dbEntity);

            return Ok(string.Join(", ", errors));
        }

        /// <summary>
        /// delete exsiting contact
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string id)
        {
            await _contactService.DeleteAsync(id);

            return Ok();
        }
        #endregion

        #region Utilities
        [NonAction]
        private async Task<(bool result, BsonDocument? bsonDynamicElements, List<string> errors)> ValidateDynamicFields(ContactAddUpdateModel model)
        {

            var bsonDynamicFields = new Dictionary<string, object>();
            var errors = new List<string>();
            var collectionSynemicFields = await _documentSchemaService.ListAsync(nameof(Contact));

            if (!collectionSynemicFields.Any())
            {
                errors.Add($"Dynamic fields for collection : {nameof(Contact)} not found.!");
                return (false, null, errors);
            }

            foreach (var kvp in model.DynamicFields)
            {
                var dynamicField = collectionSynemicFields.FirstOrDefault(x => x.FieldName.ToLowerInvariant() == kvp.Key.ToLowerInvariant());
                if (dynamicField is null)
                {
                    errors.Add($"Dynamic field {kvp.Key} not found.!");
                    continue;
                }
                string key = kvp.Key;
                string value = kvp.Value;

                int intValue;
                DateTime dateTimeValue;


                if (dynamicField.FieldType == FieldDataType.Integer && int.TryParse(value, out intValue))
                {
                    bsonDynamicFields.Add(dynamicField.FieldName, intValue);
                }
                else if (dynamicField.FieldType == FieldDataType.Date && DateTime.TryParse(value, out dateTimeValue))
                {
                    bsonDynamicFields.Add(dynamicField.FieldName, dateTimeValue);
                }
                else if (dynamicField.FieldType == FieldDataType.String)
                {
                    bsonDynamicFields.Add(dynamicField.FieldName, kvp.Value);
                }
                else
                    // Value is not a valid data type
                    errors.Add($"Field: {key}, with value: {value} (Invalid data type)");

            }
            return (true, new BsonDocument(bsonDynamicFields), errors);
        }
        #endregion

    }
}
