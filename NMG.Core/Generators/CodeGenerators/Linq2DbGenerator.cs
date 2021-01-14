using System;
using System.CodeDom;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generators.CodeGenerators
{
    public class Linq2DbGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences _appPrefs;

        public Linq2DbGenerator(ApplicationPreferences appPrefs, Table table) : base(appPrefs.FolderPath, "Mapping", appPrefs.TableName, appPrefs.NameSpaceMap, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            _appPrefs = appPrefs;
        }

        public override void Generate(bool writeToFile = true)
        {
            var pascalCaseTextFormatter = new PascalCaseTextFormatter { PrefixRemovalList = _appPrefs.FieldPrefixRemovalList };

            string className = $"{_appPrefs.ClassNamePrefix}{pascalCaseTextFormatter.FormatSingular(Table.Name)}Map";
            CodeCompileUnit compileUnit = GetCompleteCompileUnit(className);
            string generateCode = GenerateCode(compileUnit, className);

            if (writeToFile)
            {
                WriteToFile(generateCode, className);
            }
        }

        protected override string CleanupGeneratedFile(string generatedContent)
        {
            return generatedContent;
        }

        public CodeCompileUnit GetCompleteCompileUnit(string mapName)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeCompileUnit compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, mapName);

            CodeTypeDeclaration newType = compileUnit.Namespaces[0].Types[0];

            newType.IsPartial = _appPrefs.GeneratePartialClasses;

            string className = Formatter.FormatSingular(Table.Name);
            newType.BaseTypes.Add($"TableMapping");

            var constructor = new CodeConstructor { Attributes = MemberAttributes.Public, Parameters = { new CodeParameterDeclarationExpression("FluentMappingBuilder","mapper") }};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + $"mapper.Entity<{_appPrefs.ClassNamePrefix}{className}>()"));

            string columRequiered = ".IsColumnRequired()";
            constructor.Statements.Add(new CodeSnippetStatement($"{TABS}{columRequiered}"));

            if (!Table.Name.ToLower().Equals(className.ToLower()))
            {
                // .HasTableName("AbcRating")
                string tableAttribute = $".HasTableName(\"{Table.Name}\")";
                constructor.Statements.Add(new CodeSnippetStatement($"{TABS}{tableAttribute}"));
            }

            if (!string.IsNullOrEmpty(Table.Owner))
            {
                // .HasSchemaName("Analyst")
                string schemaName = $".HasSchemaName(\"{Table.Owner}\")";
                constructor.Statements.Add(new CodeSnippetStatement($"{TABS}{schemaName}"));
            }



            //if (UsesSequence)
            //{
            //    string fieldName = FixPropertyWithSameClassName(Table.PrimaryKey.Columns[0].Name, Table.Name);
            //    constructor.Statements.Add(new CodeSnippetStatement(
            //        TABS +
            //        $"Id(x => x.{Formatter.FormatText(fieldName)}).Column(x => x.{fieldName}).GeneratedBy.Sequence(\"{_appPrefs.Sequence}\")"));
            //}
            //else if (Table.PrimaryKey != null && Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            //{
            //    string fieldName = FixPropertyWithSameClassName(Table.PrimaryKey.Columns[0].Name, Table.Name);
            //    constructor.Statements.Add(GetIdMapCodeSnippetStatement(_appPrefs, Table, Table.PrimaryKey.Columns[0].Name, fieldName, Table.PrimaryKey.Columns[0].DataType, Formatter));
            //}
            //else if (Table.PrimaryKey != null)
            //{
            //    constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey, Table, Formatter));
            //}

            //int nKeyOrder = 0;
            //foreach (Column column in Table.PrimaryKey.Columns)
            //{
            //    string propertyName = Formatter.FormatText(column.Name);
            //    string fieldName = FixPropertyWithSameClassName(propertyName, Table.Name);
            //    string columnMapping = new Linq2DbColumnMapper().Map(column, fieldName, Formatter, _appPrefs.IncludeLengthAndScale, ++nKeyOrder);
            //    constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            //}

            // Property Map
            foreach (Column column in Table.Columns.Where(x => !x.IsForeignKey))
            {
                string propertyName = Formatter.FormatText(column.Name);
                string fieldName = FixPropertyWithSameClassName(propertyName, Table.Name);
                string columnMapping = new Linq2DbColumnMapper().Map(column, fieldName, Formatter, _appPrefs.IncludeLengthAndScale);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

            constructor.Statements.Add(new CodeSnippetStatement(TABS + ";"));

            newType.Members.Add(constructor);

            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            var builder = new StringBuilder();
            builder.AppendLine("#pragma warning disable 1591");
            builder.AppendLine();
            builder.AppendLine("using LinqToDB.Mapping;");
            builder.Append(entireContent);
            return builder.ToString();
        }

        private static string FixPropertyWithSameClassName(string property, string className)
        {
            return property.ToLowerInvariant() == className.ToLowerInvariant() ? property + "Val" : property;
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(ApplicationPreferences appPrefs, Table table, string pkColumnName, string propertyName, string pkColumnType, ITextFormatter formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(appPrefs.ServerType, pkColumnType, null, null, null)).IsTypeIntegral();

            string fieldName = FixPropertyWithSameClassName(propertyName, table.Name);

            int pkAlsoFkQty = (from fk in table.ForeignKeys.Where(fk => fk.UniquePropertyName == pkColumnName) select fk).Count();
            if (pkAlsoFkQty > 0)
                fieldName = fieldName + "Id";

            string snippet = TABS + $".Property(x => x.{formatter.FormatText(fieldName)}).IsPrimaryKey()";
            if (isPkTypeIntegral)
                snippet += Environment.NewLine + TABS + $".Property(x => x.{formatter.FormatText(fieldName)}).IsIdentity()";

            return new CodeSnippetStatement(snippet);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, Table table, ITextFormatter formatter)
        {
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            int count = 1;
            foreach (Column pkColumn in primaryKey.Columns)
            {
                string propertyName = formatter.FormatText(pkColumn.Name);
                string fieldName = FixPropertyWithSameClassName(propertyName, table.Name);
                int pkAlsoFkQty = (from fk in table.ForeignKeys.Where(fk => fk.UniquePropertyName == pkColumn.Name) select fk).Count();
                if (pkAlsoFkQty > 0) fieldName = fieldName + "Id";
                string tmp = $".Property(x => x.{fieldName}).IsPrimaryKey()";
                keyPropertyBuilder.Append("\n" + TABS + tmp);
                count++;
            }

            return new CodeSnippetStatement(keyPropertyBuilder.ToString());
        }

    }
}

