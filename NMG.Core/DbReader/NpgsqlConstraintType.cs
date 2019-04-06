using System;

namespace NMG.Core.DbReader
{
    public sealed class NpgsqlConstraintType
    {
        public static readonly NpgsqlConstraintType PrimaryKey = new NpgsqlConstraintType(1, "PRIMARY KEY");
        public static readonly NpgsqlConstraintType ForeignKey = new NpgsqlConstraintType(2, "FOREIGN KEY");
        public static readonly NpgsqlConstraintType Check = new NpgsqlConstraintType(3, "CHECK");
        public static readonly NpgsqlConstraintType Unique = new NpgsqlConstraintType(4, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private NpgsqlConstraintType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }
    }
}
