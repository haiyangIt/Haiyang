using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;

namespace SqlDbImpl.Storage
{
    public class TableDataAccess
    {
        private CloudTableClient _tableClient;

        private static HashSet<char> _validTableNameCharactors;

        private static HashSet<char> ValidTableNameCharactors
        {
            get
            {
                if(_validTableNameCharactors == null)
                {
                    _validTableNameCharactors = new HashSet<char>();
                    var number = "0123456789";
                    var alpha = "abcdefghijklmnopqrstuvwxyz";
                    foreach(char c in number)
                    {
                        _validTableNameCharactors.Add(c);
                    }
                    foreach(char c in alpha)
                    {
                        _validTableNameCharactors.Add(c);
                    }
                }
                return _validTableNameCharactors;
            }
        }

        private static HashSet<char> _rowPartitionInvalidKey;
        private static HashSet<char> RowPartitionInvalidKey
        {
            get
            {
                if(_rowPartitionInvalidKey == null)
                {
                    List<char> invalidChar = new List<char>(0x4F);
                    for (char controlLower = '\u0000'; controlLower <= '\u001F'; controlLower++)
                    {
                        invalidChar.Add(controlLower);
                    }
                    for (char controlLower = '\u007F'; controlLower <= '\u009F'; controlLower++)
                    {
                        invalidChar.Add(controlLower);
                    }
                    invalidChar.Add('/');
                    invalidChar.Add('\\');
                    invalidChar.Add('#');
                    invalidChar.Add('?');

                    _rowPartitionInvalidKey = new HashSet<char>(invalidChar);
                }
                return _rowPartitionInvalidKey;
            }
        }


        public TableDataAccess(CloudTableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public CloudTable CreateIfNotExist(string tableName)
        {
            CloudTable table = _tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
            
            return table;
        }

        public CloudTable GetTable(string tableName)
        {
            CloudTable table = _tableClient.GetTableReference(tableName);
            if (table.Exists())
                return table;
            else
                return null;
        }

        public TableResult InsertEntity(CloudTable table, TableEntity entity)
        {
            TableOperation insertOperation = TableOperation.Insert(entity);
            return table.Execute(insertOperation);
        }

        internal static string ValidateRowPartitionKey(string rowKey, bool isCheck = true)
        {
            if (string.IsNullOrEmpty(rowKey))
                throw new ArgumentNullException("rowKey");

            if (!isCheck)
                return rowKey;

            foreach(char c in rowKey)
            {
                if(_rowPartitionInvalidKey.Contains(c))
                {
                    throw new ArgumentException(string.Format("Invalid row/partition keys:{0}, can not contain charactor:{1}.", rowKey, (int)c));
                }
            }
            return rowKey;
        }

        public static string ValidateTableName(string tableName, bool isCheck = true)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            if (!isCheck)
                return tableName.ToLower();

            if (tableName.Length < 3 || tableName.Length > 63)
                throw new ArgumentException(string.Format("table name length must be between 3 and 63. but the {0} length is {1}.", tableName, tableName.Length), "tableName");

            StringBuilder sb = new StringBuilder(tableName.Length);
            tableName = tableName.ToLower();

            if (tableName[0] <= '9' && tableName[0] >= '0')
                throw new ArgumentException(string.Format("Invalid table name:{0}, can not begin with charactor:{1}.", tableName, tableName[0]));

            foreach(char c in tableName)
            {
                if(!ValidTableNameCharactors.Contains(c))
                {
                    throw new ArgumentException(string.Format("Invalid table name:{0}, can not contain charactor:{1}.", tableName, (int)c));
                }
            }
            return tableName;
        }
        
    }
}