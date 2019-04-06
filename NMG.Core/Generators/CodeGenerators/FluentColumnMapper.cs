using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generators.CodeGenerators
{
	public class Constants
	{
		public static string SemiColon = ";";
		public static string Dot = ".";
	}

    public class FluentColumnMapper
    {
        public string Map(Column column, string fieldName, ITextFormatter Formatter, bool includeLengthAndScale = true)
        {
            var mappedStrBuilder = new StringBuilder($"Map(x => x.{fieldName})");
            mappedStrBuilder.Append(Constants.Dot);
            mappedStrBuilder.Append("Column(\"" + column.Name + "\")");

            if (!column.IsNullable)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Not.Nullable()");
            }

            if (column.IsUnique)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Unique()");
            }

            if (column.DataLength.GetValueOrDefault() > 0 & includeLengthAndScale)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Length(" + column.DataLength + ")");
            }
            else
            {
                if (column.DataPrecision.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(Constants.Dot);
                    mappedStrBuilder.Append("Precision(" + column.DataPrecision + ")");
                }

                if (column.DataScale.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(Constants.Dot);
                    mappedStrBuilder.Append("Scale(" + column.DataScale + ")");
                }
            }


            mappedStrBuilder.Append(Constants.SemiColon);
            return mappedStrBuilder.ToString();
        }
    }
}