using System;

namespace NMG.Core.DbReader
{
    public sealed class ODBCProgressConstraintType
    {
        public static readonly ODBCProgressConstraintType PrimaryKey = new ODBCProgressConstraintType(1, "PRIMARY KEY");
        public static readonly ODBCProgressConstraintType ForeignKey = new ODBCProgressConstraintType(2, "FOREIGN KEY");
        public static readonly ODBCProgressConstraintType Check = new ODBCProgressConstraintType(3, "CHECK");
        public static readonly ODBCProgressConstraintType Unique = new ODBCProgressConstraintType(4, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private ODBCProgressConstraintType(int value, String name)
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
