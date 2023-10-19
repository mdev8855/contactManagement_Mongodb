namespace CM.API.Models.ParameterModel
{
    public class CompanyAddUpdateModel
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public int NumberOfEmployees { get; set; }
        public Dictionary<string, string> DynamicFields { get; set; } = new Dictionary<string, string>();
    }
}
