using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generators.CodeGenerators
{
    public class Linq2DbColumnMapper
    {
        public string Map(Column column, string fieldName, ITextFormatter formatter, bool includeLengthAndScale = true)
        {
            var sb = new StringBuilder($".Property(x => x.{fieldName})");

            if (column.Name != fieldName)
                sb.Append(".HasColumnName(\"" + column.Name + "\")");
            if (column.IsIdentity)
                sb.Append(".IsIdentity()");
            if (column.IsPrimaryKey)
                sb.Append(".IsPrimaryKey()");

            sb.Append(!column.IsNullable ? ".IsNullable(false)" : ".IsNullable()");

            if (includeLengthAndScale)
            {
                if (column.DataLength.GetValueOrDefault() > 0)
                {
                    sb.Append(".HasLength(" + column.DataLength + ")");
                }
                else
                {
                    if (column.DataPrecision.GetValueOrDefault(0) > 0)
                        sb.Append(".HasPrecision(" + column.DataPrecision + ")");

                    if (column.DataScale.GetValueOrDefault(0) > 0)
                        sb.Append(".HasScale(" + column.DataScale + ")");
                }
            }

            return sb.ToString();
        }
    }
}