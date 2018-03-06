using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMG.Core.Reader
{
    public sealed class ODBCConstraintType
    {
        public static readonly ODBCConstraintType PrimaryKey = new ODBCConstraintType(1, "PRIMARY KEY");
        public static readonly ODBCConstraintType ForeignKey = new ODBCConstraintType(2, "FOREIGN KEY");
        public static readonly ODBCConstraintType Check = new ODBCConstraintType(3, "CHECK");
        public static readonly ODBCConstraintType Unique = new ODBCConstraintType(4, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private ODBCConstraintType(int value, String name)
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
