using System;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;

namespace NMG.Core.Linq2db
{
    public class DBColumnMapper
    {
        public string Map(Column column, string fieldName, ITextFormatter Formatter, bool includeLengthAndScale = true)
        {
            var mappedStrBuilder = new StringBuilder($".Property(x => x.{fieldName})");
            mappedStrBuilder.Append(Constants.Dot);
            mappedStrBuilder.Append("HasColumnName(\"" + column.Name + "\")");

            mappedStrBuilder.Append(Constants.Dot);
            if (!column.IsNullable)
                mappedStrBuilder.Append($"IsNullable(false)");
            else
                mappedStrBuilder.Append($"IsNullable()");

            //if (column.IsUnique)
            //{
            //    mappedStrBuilder.Append(Constants.Dot);
            //    mappedStrBuilder.Append("Unique()");
            //}

            if (column.DataLength.GetValueOrDefault() > 0 & includeLengthAndScale)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("HasLength(" + column.DataLength + ")");
            }
            else
            {
                if (column.DataPrecision.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(Constants.Dot);
                    mappedStrBuilder.Append("HasPrecision(" + column.DataPrecision + ")");
                }

                if (column.DataScale.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(Constants.Dot);
                    mappedStrBuilder.Append("HasScale(" + column.DataScale + ")");
                }
            }

            return mappedStrBuilder.ToString();
        }
    }
}