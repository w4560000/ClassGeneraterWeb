using ClassGeneraterWeb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClassGeneraterWeb.Services
{
    public class GeneraterClassService : IGeneraterClassService
    {
        /// <summary>
        /// 建立Class poco
        /// </summary>
        /// <param name="connection">連線字串</param>
        /// <param name="spExec">執行SP script</param>
        /// <returns>Class poco</returns>
        public (string pocoClass, string ErrorMessage) GeneraterClass(GeneraterClassAction generaterClassAction)
        {
            string errorMessage = string.Empty;

            string pocoClass;

            try
            {
                string queryText = CreateSqlQuery(generaterClassAction.ConnectionString, generaterClassAction.SpExec.Trim());
                pocoClass = GenerateCSharpCode(generaterClassAction.ConnectionString, queryText, generaterClassAction.ClassName);
            }
            catch (Exception ex)
            {
                pocoClass = "";
                errorMessage = ex.Message;
            }

            return (pocoClass, errorMessage);
        }

        private string CreateSqlQuery(string connection, string procedureName)
        {
            string sqlQuery = string.Empty;
            //if (!procedureName.StartsWith("exec", StringComparison.OrdinalIgnoreCase) && !procedureName.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            //{
            //    StringBuilder completeProc = new StringBuilder();
            //    completeProc.Append("Exec " + procedureName + " ");
            //    using (SqlConnection conn = new SqlConnection(connection))
            //    {
            //        SqlCommand cmd = new SqlCommand(procedureName, conn);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        conn.Open();
            //        SqlCommandBuilder.DeriveParameters(cmd);
            //        foreach (SqlParameter p in cmd.Parameters)
            //        {
            //            if (p.Direction != ParameterDirection.ReturnValue)
            //            {
            //                completeProc.Append(p.ParameterName + "=null,");
            //            }
            //        }
            //        sqlQuery = completeProc.ToString().TrimEnd(',');
            //    }
            //}
            //else
                sqlQuery = procedureName;
            return sqlQuery;
        }

        private string GenerateCSharpCode(string connection, string query, string className)
        {
            List<List<SchemaField>> ColumnsDSetList = GetSchemaFields(connection, query);

            if (ColumnsDSetList.Count > 1)
                return "";

            className = string.IsNullOrEmpty(className) ? "pocoClass" : className;

            StringBuilder sbuilder = new StringBuilder();
            foreach (var Columns in ColumnsDSetList)
            {
                sbuilder.Append($"public class {className}");
                sbuilder.Append("\r\n{");
                sbuilder.Append("\r\n");
                for (int i = 0; i < Columns.Count; i++)
                {
                    string IsNullableProperty = string.Empty;
                    string columnName = Columns[i].ColumnName;
                    string typename = ConvertSqlTypetoCSharp(Columns[i].DataTypeName);
                    if (typename != "string")
                    {
                        IsNullableProperty = Columns[i].AllowDBNull ? "?" : string.Empty;
                    }
                    sbuilder.Append(string.Format("\t public {0}{1} {2} {{ get; set; }}", typename, IsNullableProperty, columnName));
                    sbuilder.Append("\r\n");
                }
                sbuilder.Append("}");
                sbuilder.Append("\r\n");
                sbuilder.Append("\r\n");
            }
            return sbuilder.ToString();
        }

        private List<List<SchemaField>> GetSchemaFields(string ConnectionString, string Query)
        {
            DataSet SchemaDataSet = GetSchema(Query, ConnectionString);

            List<List<SchemaField>> resultDataSet = new List<List<SchemaField>>();

            foreach (DataTable SchemaTable in SchemaDataSet.Tables)
            {
                List<SchemaField> result = new List<SchemaField>();
                for (int i = 0; i < SchemaTable.Rows.Count; i++)
                {
                    var schemaFieldRow = new SchemaField();
                    for (int j = 0; j < SchemaTable.Columns.Count; j++)
                    {
                        if (SchemaTable.Rows[i][j] != System.DBNull.Value)
                        {
                            switch (SchemaTable.Columns[j].ColumnName)
                            {
                                case "ColumnName":
                                    schemaFieldRow.ColumnName = SchemaTable.Rows[i][j].ToString();
                                    break;

                                case "ColumnSize":
                                    schemaFieldRow.ColumnSize = Convert.ToInt32(SchemaTable.Rows[i][j]);
                                    break;

                                case "DataType":
                                    schemaFieldRow.DataType = ((System.Type)SchemaTable.Rows[i][j]).FullName;
                                    break;

                                case "AllowDBNull":
                                    schemaFieldRow.AllowDBNull = Convert.ToBoolean(SchemaTable.Rows[i][j]);
                                    break;

                                case "DataTypeName":
                                    schemaFieldRow.DataTypeName = SchemaTable.Rows[i][j].ToString();
                                    break;
                            }
                        }
                    }
                    result.Add(schemaFieldRow);
                }
                resultDataSet.Add(result);
            }

            return resultDataSet;
        }

        private DataSet GetSchema(string storedProcedureText, string connection)
        {
            DataSet dtSet = new DataSet();
            int i = 0;
            using (SqlConnection sqlCon = new SqlConnection(connection))
            {
                SqlCommand sqlCmd = new SqlCommand(storedProcedureText, sqlCon);
                sqlCon.Open();
                SqlDataReader sqlReader = sqlCmd.ExecuteReader(CommandBehavior.Default);
                while (sqlReader.FieldCount != 0)
                {
                    i++;
                    DataTable dt = new DataTable();
                    dt = sqlReader.GetSchemaTable();
                    dt.TableName = "dtTable" + i;
                    sqlReader.NextResult();
                    dtSet.Tables.Add(dt);
                }
            }
            return dtSet;
        }

        private string ConvertSqlTypetoCSharp(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "string": return "string";
                case "bigint": return "long";
                case "binary": return "byte[]";
                case "bit": return "bool";
                case "char": return "string";
                case "date": return "DateTime";
                case "datetime": return "DateTime";
                case "datetime2": return "DateTime";
                case "datetimeoffset": return "DateTimeOffset";
                case "decimal": return "decimal";
                case "float": return "float";
                case "image": return "byte[]";
                case "int": return "int";
                case "money": return "decimal";
                case "nchar": return "string";
                case "ntext": return "string";
                case "numeric": return "decimal";
                case "nvarchar": return "string";
                case "real": return "double";
                case "smalldatetime": return "DateTime";
                case "smallint": return "short";
                case "smallmoney": return "decimal";
                case "text": return "string";
                case "time": return "TimeSpan";
                case "timestamp": return "DateTime";
                case "tinyint": return "byte";
                case "uniqueidentifier": return "Guid";
                case "varbinary": return "byte[]";
                case "varchar": return "string";
                default:
                    return "string";
            }
        }
    }
}