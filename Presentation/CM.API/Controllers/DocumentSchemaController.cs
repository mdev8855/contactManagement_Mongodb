using CM.API.Models.ParameterModel;
using CM.Core.Domain;
using CM.Services.Schema;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CM.API.Controllers
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class DocumentSchemaController : ControllerBase
    {
        private readonly IDocumentSchemaService _documentSchemaService;

        public DocumentSchemaController(IDocumentSchemaService documentSchemaService)
        {
            _documentSchemaService = documentSchemaService;
        }

        [HttpGet()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(DocumentSchema), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(null);

            var data = await _documentSchemaService.GetAsync(id);

            if (data == null)
                return NotFound(null);

            return Ok(data);
        }


        /// <summary>
        /// Get all documentSchemas
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentSchema>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var data = await _documentSchemaService.ListAllAsync();

            return Ok(data);
        }

        ///// <summary>
        ///// Get filterd documentSchemaes
        ///// </summary>
        ///// <param name="seachModel"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ProducesResponseType(typeof(PaginatedListDto<DocumentSchemaDto>), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> Search(DocumentSchemaSearchModel searchModel)
        //{
        //    var data = await _documentSchemaService.ListGridAsyncByRole(searchModel);

        //    var dto = data.MapToPaginatedList<DocumentSchema, DocumentSchema>();

        //    return Ok(dto);
        //}

        /// <summary>
        /// Insert new documentSchema
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(DocumentSchemaAddUpdateModel model)
        {
            var entity = new DocumentSchema()
            {
                CollectionName = model.CollectionName,
                FieldName = model.FieldName,
                FieldType = model.FieldType,
            };

            await _documentSchemaService.InsertAsync(entity);

            return Ok();
        }

        /// <summary>
        /// Update exsiting documentSchema
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Dictionary<string, string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(DocumentSchemaAddUpdateModel model)
        {
            var entity = new DocumentSchema()
            {
                Id = model.Id,
                CollectionName = model.CollectionName,
                FieldName = model.FieldName,    
                FieldType = model.FieldType,  
            };

            await _documentSchemaService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// delete exsiting documentSchema
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string id)
        {
            await _documentSchemaService.DeleteAsync(id);

            return Ok();
        }
    }
}
