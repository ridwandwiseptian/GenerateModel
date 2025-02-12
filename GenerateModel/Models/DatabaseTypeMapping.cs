namespace GenerateModel.Models
{
    public class DatabaseTypeMapping
    {
        public Dictionary<string, string>? SQLServer { get; set; }
        public Dictionary<string, string>? PostgreSQL { get; set; }
        public Dictionary<string, string>? MySQL { get; set; }
        public Dictionary<string, string>? Oracle { get; set; }
    }

    public class DatabaseType
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class GenerateModelInput
    {
        public string? DatabaseSelected { get; set; }

        public List<string>? ListDatabase { get; set; }

        public string? Input { get; set; }
        public string? OutputColumn { get; set; }
        public string? OutputClassModel { get; set; }
        public string? Pesan { get; set; }
    }
}
