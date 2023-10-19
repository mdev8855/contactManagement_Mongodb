using CM.Core.Domain.Enum;

namespace CM.API.Models.ParameterModel
{
    public partial class DocumentSchemaAddUpdateModel
    {
        public string Id { get; set; }
        public required string CollectionName { get; set; }
        public required string FieldName { get; set; }
        public required FieldDataType FieldType { get; set; }
    }
}
