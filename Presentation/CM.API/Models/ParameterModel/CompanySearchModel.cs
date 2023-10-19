namespace CM.API.Models.ParameterModel
{
    public class CompanySearchModel
    {
        public string Name { get; set; }
        public int NumberOfEmployees { get; set; }
        public Dictionary<string, object> DynamicFields { get; set; } = new Dictionary<string, object>();
    }
}
