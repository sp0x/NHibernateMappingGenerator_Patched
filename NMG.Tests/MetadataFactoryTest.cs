using NMG.Core;
using NMG.Core.DbReader;
using NMG.Core.Domain;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace NMG.Tests
{
    [TestFixture]
    public class MetadataFactoryTest
    {
        [Test]
        public void ShouldCreateTheAppropriateMetadataReader()
        {
            IMetadataReader metadataReader = MetadataFactory.GetReader(ServerType.Oracle, "conn");
            Assert.That(metadataReader, Is.TypeOf(typeof (OracleMetadataReader)));

            metadataReader = MetadataFactory.GetReader(ServerType.SqlServer, "conn");
            Assert.That(metadataReader, Is.TypeOf(typeof (SqlServerMetadataReader)));
        }
    }
}