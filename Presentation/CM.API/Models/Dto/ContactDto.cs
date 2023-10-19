using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CM.API.Models.Dto
{
    public class ContactDto
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public Dictionary<string, object> DynamicFields { get; set; }
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();


    }
}
