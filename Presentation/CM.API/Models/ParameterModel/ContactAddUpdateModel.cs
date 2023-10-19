namespace CM.API.Models.ParameterModel
{
    public partial class ContactAddUpdateModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> DynamicFields { get; set;} = new Dictionary<string, string>();
        public List<string> Companies { get; set; } = new List<string>();
    }
}
