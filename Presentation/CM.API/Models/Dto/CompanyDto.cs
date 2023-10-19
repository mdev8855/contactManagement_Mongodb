using MongoDB.Bson;

namespace CM.API.Models.Dto
{
    public class CompanyDto
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public int NumberOfEmployees { get; set; }
    }
}
