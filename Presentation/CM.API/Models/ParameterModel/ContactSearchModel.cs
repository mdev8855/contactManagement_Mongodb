namespace CM.API.Models.ParameterModel
{
    public class ContactSearchModel
    {
        public string Name { get; set; }
        public Dictionary<string, object> DynamicFields { get; set; } = new Dictionary<string, object>();
    }
}
