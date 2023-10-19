using CM.API.Factories;
using CM.API.Models.ParameterModel;
using CM.Core.Domain.Enum;
using CM.Core.Domain;
using CM.Services.Companies;
using CM.Services.Schema;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Net;

namespace CM.API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CompanyController : ControllerBase
    {
        #region Fields
        private readonly ICompanyService _companyService;
        private readonly IDtoFactory _dtoFactory;
        private readonly IDocumentSchemaService _documentSchemaService;
        #endregion

        #region Ctor
        public CompanyController(ICompanyService companyService,
           IDtoFactory dtoFactory,
           IDocumentSchemaService documentSchemaService)
        {
            _companyService = companyService;
            _dtoFactory = dtoFactory;
            _documentSchemaService = documentSchemaService;
        }
        #endregion

        #region Actions
        [HttpGet()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Company), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(null);

            var data = await _companyService.GetAsync(id);

            if (data == null)
                return NotFound(null);

            var dto = _dtoFactory.PrepareCompanyDto(data);

            return Ok(dto);
        }


        /// <summary>
        /// Get all companies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _companyService.ListAllAsync();
            if (data == null)
                return NotFound(null);

            var dto = data.Select(x => _dtoFactory.PrepareCompanyDto(x)).ToList();
            return Ok(dto);
        }

        /// <summary>
        /// Get all companies filterd
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Search(CompanySearchModel model)
        {
            var data = await _companyService.ListAsync(model.Name, model.DynamicFields);
            var dto = data.Select(x => _dtoFactory.PrepareCompanyDto(x)).ToList();
            return Ok(dto);
        }

        /// <summary>
        /// Insert new company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(CompanyAddUpdateModel model)
        {
            #region Validation

            if (model == null)
            { return BadRequest("model is null"); }

            if (string.IsNullOrEmpty(model.Name))
            { return BadRequest("name required.!"); }

            (bool result, BsonDocument? bsonDynamicElements, List<string> errors) = await ValidateDynamicFields(model);

            #endregion

            //we can change the validation logic to add main or common properties even if dynamic field has some errors

            var entity = new Company()
            {
                Name = model.Name,
                NumberOfEmployees = model.NumberOfEmployees,
            };

            if (result && bsonDynamicElements is not null)
                entity.DynamicFields = bsonDynamicElements;

            await _companyService.InsertAsync(entity);

            return Ok(string.Join(", ", errors));
        }

        /// <summary>
        /// Update exsiting company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(CompanyAddUpdateModel model)
        {
            #region Validation
            if (model == null)
            { return BadRequest("model is null"); }

            if (string.IsNullOrEmpty(model.Name))
            { return BadRequest("name required.!"); }

            if (string.IsNullOrEmpty(model.Id))
            { return BadRequest("id required.!"); }

            #endregion
            var dbEntity = await _companyService.GetAsync(model.Id);
            if (dbEntity is null)
                return NotFound();

            (bool result, BsonDocument? bsonDynamicElements, List<string> errors) = await ValidateDynamicFields(model);

            dbEntity.Name = model.Name;
            
            dbEntity.NumberOfEmployees = model.NumberOfEmployees;

            //we can change the validation logic to add main or common properties even if dynamic field has some errors
            if (result && bsonDynamicElements is not null)
                dbEntity.DynamicFields = bsonDynamicElements;


            await _companyService.UpdateAsync(dbEntity);
            return Ok(string.Join(", ", errors));
        }

        /// <summary>
        /// delete exsiting company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string id)
        {
            await _companyService.DeleteAsync(id);

            return Ok();
        }
        #endregion

        #region Utilities
        [NonAction]
        private async Task<(bool result, BsonDocument? bsonDynamicElements, List<string> errors)> ValidateDynamicFields(CompanyAddUpdateModel model)
        {

            var bsonDynamicFields = new Dictionary<string, object>();
            var errors = new List<string>();
            var collectionSynemicFields = await _documentSchemaService.ListAsync(nameof(Company));

            if (!collectionSynemicFields.Any())
            {
                errors.Add($"Dynamic fields for collection : {nameof(Company)} not found.!");
                return (true, null, errors);
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
