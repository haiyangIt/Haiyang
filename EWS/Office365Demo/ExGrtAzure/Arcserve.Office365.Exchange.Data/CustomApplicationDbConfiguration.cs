using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Data
{
    public class CustomApplicationDbConfiguration : DbConfiguration
    {
        public CustomApplicationDbConfiguration()
        {
            SetMigrationSqlGenerator(
                SqlProviderServices.ProviderInvariantName,
                () => new CustomSqlServerMigrationSqlGenerator());
        }
    }

    /// <summary>
    /// Note: support no constraint column and primary constraint column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CaseSensitiveAttribute : Attribute
    {
        public CaseSensitiveAttribute()
        {
            PrimaryKeys = string.Empty;
        }

        /// <summary>
        /// Primary Key, like column1,column2.
        /// </summary>
        /// <param name="primaryKeys"></param>
        /// <remarks>
        /// use , to split the primary key and note the column order.
        /// </remarks>
        public CaseSensitiveAttribute(string primaryKeys)
        {
            PrimaryKeys = primaryKeys;
        }

        public bool IsEnabled { get; set; }
        public string PrimaryKeys { get; set; }

        public override string ToString()
        {
            return PrimaryKeys;
        }

        public static explicit operator CaseSensitiveAttribute(string value)
        {
            CaseSensitiveAttribute result = new CaseSensitiveAttribute();
            result.IsEnabled = true;
            result.PrimaryKeys = value;
            return result;
        }

        public static CaseSensitiveAttribute Convert(object value)
        {
            return (CaseSensitiveAttribute)((string)value);
        }
    }

    public class CustomSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        protected override void Generate(AlterColumnOperation alterColumnOperation)
        {
            base.Generate(alterColumnOperation);
            AlterCaseSensitive(alterColumnOperation.Column, alterColumnOperation.Table);
        }

        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            base.Generate(addColumnOperation);

            AlterCaseSensitive(addColumnOperation.Column, addColumnOperation.Table);
        }

        private void AlterCaseSensitive(ColumnModel column, string tableName)
        {
            AnnotationValues values;
            if (column.Annotations.TryGetValue("CaseSensitive", out values))
            {
                if (values.NewValue != null) // add case sensitive
                {
                    using (var writer = Writer())
                    {
                        var newValue = CaseSensitiveAttribute.Convert(values.NewValue);

                        if (!newValue.IsEnabled)
                        {
                            return;
                        }

                        if (!string.IsNullOrEmpty(newValue.PrimaryKeys))
                        {
                            writer.WriteLine("ALTER TABLE {0} DROP CONSTRAINT {1}", tableName, Quote(PrimaryKeyOperation.BuildDefaultName(tableName)));
                        }

                        string isNullable = string.Empty;
                        if(column.IsNullable.HasValue && !column.IsNullable.Value)
                        {
                            isNullable = " NOT NULL ";
                        }

                        if (column.MaxLength.HasValue)
                        {
                            writer.WriteLine(
                            "ALTER TABLE {0} ALTER COLUMN {1} NVARCHAR({2}) COLLATE SQL_Latin1_General_CP1_CS_AS {3}", 
                            tableName,
                            column.Name,
                            column.MaxLength.Value,
                            isNullable);
                        }
                        else
                        {
                            writer.WriteLine(
                                "ALTER TABLE {0} ALTER COLUMN {1} NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CS_AS {2} ", 
                                tableName,
                                column.Name,
                                isNullable);
                        }

                        if (!string.IsNullOrEmpty(newValue.PrimaryKeys))
                        {
                            writer.WriteLine("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})", tableName, Quote(PrimaryKeyOperation.BuildDefaultName(tableName)), newValue.PrimaryKeys);
                        }
                        Statement(writer);
                    }
                }
                // remove case sensitive we can ignore this change.
            }
        }

        private void DownCaseSensitive(ColumnModel column, string tableName)
        {
            AnnotationValues values;
            if (column.Annotations.TryGetValue("CaseSensitive", out values))
            {
                if (values.OldValue == null && values.NewValue != null)
                {
                    using (var writer = Writer())
                    {
                        var newValue = values.NewValue as CaseSensitiveAttribute;

                        if (!newValue.IsEnabled)
                        {
                            return;
                        }

                        if (!string.IsNullOrEmpty(newValue.PrimaryKeys))
                        {
                            writer.WriteLine("ALTER TABLE {0} DROP CONSTRAINT {1}", tableName, Quote(PrimaryKeyOperation.BuildDefaultName(tableName)));
                        }

                        string isNullable = string.Empty;
                        if (column.IsNullable.HasValue && !column.IsNullable.Value)
                        {
                            isNullable = " NOT NULL ";
                        }

                        if (column.MaxLength.HasValue)
                        {
                            writer.WriteLine(
                            "ALTER TABLE {0} ALTER COLUMN {1} NVARCHAR({2}) COLLATE SQL_Latin1_General_CP1_CS_AS {3} ",
                            tableName,
                            column.Name,
                            column.MaxLength.Value, isNullable);
                        }
                        else
                        {
                            writer.WriteLine(
                                "ALTER TABLE {0} ALTER COLUMN {1} NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CS_AS {2} ",
                                tableName,
                                column.Name, isNullable);
                        }

                        if (!string.IsNullOrEmpty(newValue.PrimaryKeys))
                        {
                            writer.WriteLine("ALTER TABLE {0} ADD CONSTRAINT {1} ({2})", tableName, Quote(PrimaryKeyOperation.BuildDefaultName(tableName)), newValue.PrimaryKeys);
                        }
                        Statement(writer);
                    }
                }
            }
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            base.Generate(createTableOperation);

            foreach (var column in createTableOperation.Columns)
            {
                AlterCaseSensitive(column, createTableOperation.Name);
            }
        }
    }

    
}
