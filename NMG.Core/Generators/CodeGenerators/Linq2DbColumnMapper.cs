using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generators.CodeGenerators
{
    public class Linq2DbColumnMapper
    {
        public string Map(Column column, string fieldName, ITextFormatter formatter, bool includeLengthAndScale = true, int keyOrder = 0)
        {
            var sb = new StringBuilder($".HasAttribute(x => x.{fieldName}, new ColumnAttribute(");

            if (column.Name != fieldName) sb.Append($"\"{column.Name}\"");
            sb.Append(") {");

            if (column.IsIdentity)    sb.Append("IsIdentity=true,");
            if (column.IsPrimaryKey)  sb.Append("IsPrimaryKey=true,");
            if (column.IsNullable)    sb.Append("CanBeNull = true,");

            if (includeLengthAndScale)
            {
                if (column.DataLength.GetValueOrDefault() > 0)
                {
                    sb.Append("Length=" + column.DataLength + ",");
                }
                else
                {
                    if (column.DataPrecision.GetValueOrDefault(0) > 0)
                        sb.Append("Precision=" + column.DataPrecision + ",");

                    if (column.DataScale.GetValueOrDefault(0) > 0)
                        sb.Append("Scale=" + column.DataScale + ",");
                }
            }

            sb.Append("})"); // //DataType=, DbType=, Order=, PrimaryKeyOrder=,  MemberName=");

            return sb.ToString();
        }
    }
}
