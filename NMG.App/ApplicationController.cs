using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generators;
using NMG.Core.Generators.CodeGenerators;
using NMG.Core.Generators.MappingGenerators;

namespace NHibernateMappingGenerator
{
    public class ApplicationController
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly CastleGenerator castleGenerator;
        private readonly CodeGenerator codeGenerator;
        private readonly FluentGenerator fluentGenerator;
        private readonly MappingGenerator mappingGenerator;
        private readonly ContractGenerator contractGenerator;
        private readonly ByCodeGenerator byCodeGenerator;
        private EntityFrameworkGenerator entityFrameworkGenerator;
        private Linq2DbGenerator linq2dbkGenerator;

        public ApplicationController(ApplicationPreferences applicationPreferences, Table table)
        {
            this.applicationPreferences = applicationPreferences;
            codeGenerator = new CodeGenerator(applicationPreferences, table);
            fluentGenerator = new FluentGenerator(applicationPreferences, table);
            entityFrameworkGenerator = new EntityFrameworkGenerator(applicationPreferences, table);
            linq2dbkGenerator = new Linq2DbGenerator(applicationPreferences, table);
            castleGenerator = new CastleGenerator(applicationPreferences, table);
            contractGenerator = new ContractGenerator(applicationPreferences, table);
            byCodeGenerator = new ByCodeGenerator(applicationPreferences, table);
            if (applicationPreferences.ServerType == ServerType.Oracle)
            {
                mappingGenerator = new OracleMappingGenerator(applicationPreferences, table);
            }
            else
            {
                mappingGenerator = new SqlMappingGenerator(applicationPreferences, table);
            }
        }
        public string GeneratedDomainCode { get; set; }
        public string GeneratedMapCode { get; set; }

        public void Generate(bool writeToFile = true)
        {
            codeGenerator.Generate(writeToFile);
            GeneratedDomainCode = codeGenerator.GeneratedCode;

            if (applicationPreferences.IsFluent)
            {
                fluentGenerator.Generate(writeToFile);
                GeneratedMapCode = fluentGenerator.GeneratedCode;
            }
            else if (applicationPreferences.IsEntityFramework)
            {
                entityFrameworkGenerator.Generate(writeToFile);
                GeneratedMapCode = entityFrameworkGenerator.GeneratedCode;
            }
            else if (applicationPreferences.IsLinq2db)
            {
                linq2dbkGenerator.Generate(writeToFile);
                GeneratedMapCode = linq2dbkGenerator.GeneratedCode;
            }
            else if (applicationPreferences.IsCastle)
            {
                castleGenerator.Generate(writeToFile);
                GeneratedMapCode = castleGenerator.GeneratedCode;
            }
            else if (applicationPreferences.IsByCode)
            {
                byCodeGenerator.Generate(writeToFile);
                GeneratedMapCode = byCodeGenerator.GeneratedCode;
            }
            else
            {
                mappingGenerator.Generate(writeToFile);
                GeneratedMapCode = mappingGenerator.GeneratedCode;
            }

            if (applicationPreferences.GenerateWcfDataContract)
            {
                contractGenerator.Generate(writeToFile);
            }
        }
    }
}