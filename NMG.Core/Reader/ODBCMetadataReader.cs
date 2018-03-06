using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using System.Data.Odbc;
using System.Data;

namespace NMG.Core.Reader
{
    public class ODBCMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public ODBCMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        #region IMetadataReader Members

        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();
            var conn = new OdbcConnection(connectionStr);
            conn.Open();
            //try
            {
                OdbcCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select top 10 * from " + owner + "." + table;
                OdbcDataReader reader = cmd.ExecuteReader((CommandBehavior.KeyInfo|CommandBehavior.SchemaOnly));
                DataTable details = reader.GetSchemaTable();
                string[] rest = { null,null,table.Name,null };

                DataTable schema = conn.GetSchema("Indexes",rest);

                foreach (DataRow row in schema.Rows)
                { 
                
                }

                var m = new DataTypeMapper();
                foreach (DataRow row in details.Rows)
                {
                    Column c = new Column();
                    c.Name = row["ColumnName"].ToString();
                    c.DataType = row["DataType"].ToString();
                    c.IsNullable = (bool)row["AllowDBNull"];
                    c.IsIdentity = (bool)row["IsAutoIncrement"];
                    c.IsPrimaryKey = (bool)row["IsKey"];
                    c.IsForeignKey = false;
                    c.IsUnique = (bool)row["IsUnique"];
                    c.MappedDataType = row["DataType"].ToString();
                    c.DataLength = (int)row["ColumnSize"];
                    c.DataScale = (short)row["NumericScale"];
                    c.DataPrecision = (int)row["ColumnSize"];
                    c.ConstraintName = "";

                    columns.Add(c);
                }



                table.Columns = columns;

                //table.Owner = owner;
                table.PrimaryKey = DeterminePrimaryKeys(table);

                // Need to find the table name associated with the FK
                foreach (var c in table.Columns)
                {
                    if (c.IsForeignKey)
                    {
                        string referencedTableName;
                        string referencedColumnName;
                        GetForeignKeyReferenceDetails(c.ConstraintName, out referencedTableName, out referencedColumnName);

                        c.ForeignKeyTableName = referencedTableName;
                        c.ForeignKeyColumnName = referencedColumnName;
                    }
                }
                table.ForeignKeys = DetermineForeignKeyReferences(table);
                table.HasManyRelationships = DetermineHasManyRelationships(table);
            }
            //finally
            {
                conn.Close();
            }

            return columns;
        }

        public IList<string> GetOwners()
        {
            var owners = new List<string>();


            var conn = new OdbcConnection(connectionStr);
            conn.Open();
            try
            {
                DataTable table = conn.GetSchema("Tables");
                foreach (DataRow row in table.Rows)
                {
                    if (row[3].ToString() != "SYSTEM TABLE")
                    {
                        if (owners.Find(x => x == row[1].ToString()) == null)
                            owners.Add(row[1].ToString());
                    }
                }

            }
            finally
            {
                conn.Close();
            }

            return owners;
        }

        public List<Table> GetTables(string owner)
        {

            var tables = new List<Table>();

            var conn = new OdbcConnection(connectionStr);
            conn.Open();
            try
            {
                DataTable table = conn.GetSchema("Tables");
                foreach (DataRow row in table.Rows)
                {
                    if ((row[3].ToString() != "SYSTEM TABLE") && (row[1].ToString() == owner))
                    {
                        Table t = new Table() { Name = row[2].ToString(), Owner = owner };
                        tables.Add(t);
                    }
                }

            }
            finally
            {
                conn.Close();
            }

            return tables;
        }

        public List<string> GetSequences(string owner)
        {
            return new List<string>();
        }

        #endregion

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true)).ToList();

            if (primaryKeys.Count() == 1)
            {
                var c = primaryKeys.First();
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.PrimaryKey,
                                  Columns = { c }
                              };
                return key;
            }

            if (primaryKeys.Count() > 1)
            {
                // Composite key
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.CompositeKey,
                                  Columns = primaryKeys
                              };

                return key;
            }

            return null;
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = (from c in table.Columns
                               where c.IsForeignKey
                               group c by new { c.ConstraintName, c.ForeignKeyTableName, c.IsNullable } into g
                               select new ForeignKey
                               {
                                   Name = g.Key.ConstraintName,
                                   IsNullable = g.Key.IsNullable,
                                   References = g.Key.ForeignKeyTableName,
                                   Columns = g.ToList(),
                                   UniquePropertyName = g.Key.ForeignKeyTableName
                               }).ToList();

            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }

        private void GetForeignKeyReferenceDetails(string constraintName, out string referencedTableName, out string referencedColumnName)
        {
            referencedTableName = string.Empty;
            referencedColumnName = string.Empty;
            /*
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            try
            {
                using (conn)
                {
                    using (var tableDetailsCommand = conn.CreateCommand())
                    {

                        SqlCommand tableCommand = conn.CreateCommand();
                        tableDetailsCommand.CommandText = String.Format(
                            @"
SELECT  
     KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME 
    ,KCU1.TABLE_NAME AS FK_TABLE_NAME 
    ,KCU1.COLUMN_NAME AS FK_COLUMN_NAME 
    ,KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION 
    ,KCU2.CONSTRAINT_NAME AS REFERENCED_CONSTRAINT_NAME 
    ,KCU2.TABLE_NAME AS REFERENCED_TABLE_NAME 
    ,KCU2.COLUMN_NAME AS REFERENCED_COLUMN_NAME 
    ,KCU2.ORDINAL_POSITION AS REFERENCED_ORDINAL_POSITION
    ,KU.Column_Name as REFERENCED_TABLE_PK_COL
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC 

LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 
    ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  
    AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA 
    AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME 

LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 
    ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  
    AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA 
    AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME 
    AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION 
    
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
	ON TC.TABLE_NAME = KCU2.TABLE_NAME AND
	   TC.TABLE_SCHEMA = KCU2.TABLE_SCHEMA
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU 
	ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND 
	   TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
WHERE KCU1.CONSTRAINT_NAME = '{0}'",
                            constraintName);

                        using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (sqlDataReader.Read())
                            {
                                referencedTableName = sqlDataReader["REFERENCED_TABLE_NAME"].ToString();
                                var referencedPkColumnName = sqlDataReader["REFERENCED_TABLE_PK_COL"].ToString();
                                var refColumnName = sqlDataReader["REFERENCED_COLUMN_NAME"].ToString();
                                referencedColumnName = refColumnName == referencedPkColumnName ? string.Empty : refColumnName;
                            }
                        }
                    }
                }
            }
            finally
            {
                conn.Close();
            }
             */
        }

        // http://blog.sqlauthority.com/2006/11/01/sql-server-query-to-display-foreign-key-relationships-and-name-of-the-constraint-for-each-table-in-database/
        private IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManyRelationships = new List<HasMany>();
            /*
             var conn = new SqlConnection(connectionStr);
            conn.Open();
            try
            {
                using (conn)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText =
                            String.Format(
                                @"
						SELECT DISTINCT 
							PK_TABLE = b.TABLE_NAME,
							FK_TABLE = c.TABLE_NAME,
							FK_COLUMN_NAME = d.COLUMN_NAME,
							CONSTRAINT_NAME = a.CONSTRAINT_NAME
						FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS a 
						  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS b ON a.CONSTRAINT_SCHEMA = b.CONSTRAINT_SCHEMA AND a.UNIQUE_CONSTRAINT_NAME = b.CONSTRAINT_NAME 
						  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS c ON a.CONSTRAINT_SCHEMA = c.CONSTRAINT_SCHEMA AND a.CONSTRAINT_NAME = c.CONSTRAINT_NAME
						  JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE d on a.CONSTRAINT_NAME = d.CONSTRAINT_NAME
						WHERE b.TABLE_NAME = '{0}'
						ORDER BY 1,2",
                                table.Name.Replace("'", "''"));
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var constraintName = reader["CONSTRAINT_NAME"].ToString();
                            var fkColumnName = reader["FK_COLUMN_NAME"].ToString();
                            var pkTableName = reader["PK_TABLE"].ToString();
                            var existing = hasManyRelationships.FirstOrDefault(hm => hm.ConstraintName == constraintName);
                            if (existing == null)
                            {
                                var newHasManyItem = new HasMany
                                                {
                                                    ConstraintName = constraintName,
                                                    Reference = reader.GetString(1),
                                                    PKTableName = pkTableName
                                                };
                                newHasManyItem.AllReferenceColumns.Add(fkColumnName);
                                hasManyRelationships.Add(newHasManyItem);

                            }
                            else
                            {
                                existing.AllReferenceColumns.Add(fkColumnName);
                            }
                        }
                    }
                }
            }
            finally
            {
                conn.Close();
            }
             */
            return hasManyRelationships;
        }
    }
}
