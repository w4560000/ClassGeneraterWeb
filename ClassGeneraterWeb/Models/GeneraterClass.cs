using System.ComponentModel.DataAnnotations;

namespace ClassGeneraterWeb.Models
{
    public class CheckConnection
    {
        [Required(ErrorMessage = "請輸入 ConnectionString")]
        public string ConnectionString { get; set; }
    }

    public class GeneraterClassAction : CheckConnection
    {
        public string ClassName { get; set; }

        [Required(ErrorMessage = "請輸入 SP Script")]
        public string SpExec { get; set; }
    }

    public class SchemaField
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool AllowDBNull { get; set; }
        public string DataTypeName { get; set; }
        public int ColumnSize { get; set; }
    }

    public class GeneraterClassViewModel
    {
        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        public string PocoClass { get; set; }
    }

}