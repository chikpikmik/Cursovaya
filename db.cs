using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using static Cursovaya.ColumnInfo;
using System.Windows.Markup;
using System.Web;

namespace Cursovaya
{
    public class User
    {
        public string Name;
        public string Password;
        public int id;
        public List<int> ДоступныеОтделы;
        public User(string name, string password, int id_, List<int> доступныеотделы)
        {
            Name = name;
            Password = password;
            id = id_;
            ДоступныеОтделы = доступныеотделы;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class TableInfo
    {
        public string TableName;
        public string PrimaryKey;
        public string СвязанныйРегистр;
        public string РесурсСвязянногоРегистра;
        public enum Types {table, view};   
        public Types Type;
        
        public Dictionary<string, ColumnInfo> ColumnName_ColumnInfo;
        public Dictionary<string, string> LinkedTableName_LinkedTablePrimaryKey;

        public TableInfo(string tableName, string primarykey, string связанныйрегистр, string ресурссвязянногорегистра, Types type, Dictionary<string, ColumnInfo> columnName_ColumnInfo, Dictionary<string, string> linkedTableName_LinkedTablePrimaryKey)
        {
            TableName = tableName;
            PrimaryKey = primarykey;
            СвязанныйРегистр = связанныйрегистр;
            РесурсСвязянногоРегистра = ресурссвязянногорегистра;
            Type = type;
            ColumnName_ColumnInfo = columnName_ColumnInfo;
            LinkedTableName_LinkedTablePrimaryKey = linkedTableName_LinkedTablePrimaryKey;
        }
    }
    public class ColumnInfo
    {
        public bool NotNull;
        public bool ЭтоРесурсСвязанногоРегистра;
        public enum Types{
            INTEGER, 
            REAL,
            TEXT, 
            DATE
        };
        public Types Type;
        public bool IsItPrimaryKey;
        public string LinkedTableName;
        public string ColumnName;

        public ColumnInfo(string columnname, Types type, bool notNull, bool эторесурссвязанногорегистра, bool isItPrimaryKey, string linkedTableName=null)
        {
            ColumnName = columnname;
            Type = type;
            NotNull = notNull;
            ЭтоРесурсСвязанногоРегистра = эторесурссвязанногорегистра;
            IsItPrimaryKey = isItPrimaryKey;
            LinkedTableName = linkedTableName;
        }
    }

    public class ReportInfo 
    {
        public string Name;
        public string Query;
        public Dictionary<string, ParameterInfo> Parameters = new Dictionary<string,ParameterInfo>();
        public int id;
        public ReportInfo(string name, string query, int id_)
        {
            Name = name;
            Query = query;
            id = id_;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class ParameterInfo 
    {
        public string Name;
        public ColumnInfo.Types Type;
        public ParameterInfo(string name, ColumnInfo.Types type)
        {
            Name = name;
            Type = type;
        }
    }

    public enum AccessRights { НетДоступа, Чтение, Запись };

    public class db
    {
        //private static string fullPath = @"C:\Users\Lolban\Projects\Cursovaya\db.db";
        private static string fullPath = "db.db";
        static public List<User> GetUsersList()
        {
            List<User> Users = new List<User>();

            DataTable СвязьОтделовИПользователей = GetDataTableByQuery("SELECT User_id, Отдел_id FROM Users_Отделы_through");

            string queryString = "SELECT Name, Password, id FROM Users";

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            string Name = reader.GetString(0);
                            string password = reader.GetString(1);
                            int id = reader.GetInt32(2);

                            DataRow[] ДоступныеОтделы = СвязьОтделовИПользователей.Select($"User_id = '{id}'");
                            List<int> ДоступныеОтделыIds = new List<int>();

                            foreach (DataRow row in ДоступныеОтделы){
                                if(row.IsNull("Отдел_id")){
                                    ДоступныеОтделыIds = null; // значит есть доступ ко всем отделам
                                    break;
                                }
                                else{
                                    int ОтделId = Convert.ToInt32(row["Отдел_id"]);
                                    ДоступныеОтделыIds.Add(ОтделId);
                                }
                            }

                            Users.Add(new User(Name, password, id, ДоступныеОтделыIds));
                        }
                    }
                }



            }

            return Users;
        }


        static public List<ReportInfo> GetReportsList()
        {
            List<ReportInfo> Reports = new List<ReportInfo>();

            string queryString = "" +
                "SELECT a.Отчет_id, b.Название AS Отчет, b.Запрос, a.Название AS Параметр, a.Тип " +
                "FROM ПараметрыОтчетов a " +
                "RIGHT JOIN " +
                "Отчеты b ON a.Отчет_id = b.id";


            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int Отчет_id = reader.GetInt32(0);
                            string Отчет = reader.GetString(1);
                            string Запрос = reader.GetString(2);
                            string Параметр = reader.GetString(3);
                            ColumnInfo.Types ТипПараметра = (reader.GetString(4) == "INTEGER" ? ColumnInfo.Types.INTEGER : (reader.GetString(4) == "REAL" ? ColumnInfo.Types.REAL : (reader.GetString(4) == "DATE" ? ColumnInfo.Types.DATE : ColumnInfo.Types.TEXT)));
                            
                            ReportInfo report = Reports.Find(p => p.id == Отчет_id);

                            if (report == null) {
                                report = new ReportInfo(Отчет, Запрос, Отчет_id);
                                Reports.Add(report);
                            }
                            
                            report.Parameters.Add(Параметр, new ParameterInfo(Параметр, ТипПараметра));
                        }
                    }
                }
            }

            return Reports;
        }

        static public Dictionary<string, AccessRights> GetUserAccessRights(int UserId)
        {
            string query = "" +
                "SELECT " +
                    "sm.name AS TableName, " +
                    "arn.Name AS AccesRightName " +
                "FROM Users u " +
                "CROSS JOIN sqlite_master sm " +
                "LEFT JOIN Roles_AccessRights_Tables_through ar " +
                    "ON sm.name = ar.Table_name AND ar.Role_id = u.Role_id " +
                "LEFT JOIN AccessRights arn " +
                    "ON arn.id = ar.AccessRight_id " +
                "WHERE (sm.type='table' OR sm.type='view') " +
                    "AND sm.name NOT LIKE 'sqlite_%' " +
                    $"AND u.id = '{UserId}' ";

            Dictionary<string, AccessRights> result = new Dictionary<string, AccessRights>();

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string TableName = reader.GetString(0);
                            AccessRights accessRight = reader.IsDBNull(1) ? AccessRights.НетДоступа : ( reader.GetString(1)=="Запись" ? AccessRights.Запись : AccessRights.Чтение);

                            result.Add(TableName, accessRight);
                        }
                    }
                }
            }

            return result;
        }

        static public TableInfo GetTableInfo(string TableName) 
        {
            string queryAboutTableType = "" +
                "SELECT sm.type, l.Регистр, l.Ресурс " +
                "FROM sqlite_master sm " +
                "LEFT JOIN СвязьТаблицИРегистров l ON l.Таблица=sm.name " +
                $"WHERE (sm.type='table' OR sm.type='view') AND sm.name = '{TableName}' ";

            string queryAboutColumns = "" +
                "SELECT " +
                    "table_info.name AS [Column], " +
                    "table_info.type AS Type, " +
                    "table_info.[notnull] AS NotNullVal, " +
                    "table_info.pk AS IsItPrimaryKey, " +
                    "foreign_key_list.[table] AS Linked_Table," +
                    "foreign_key_list.[to] AS Linked_Table_Primary_Key " +
                "FROM " +
                    $"pragma_table_info('{TableName}') AS table_info " +
                "LEFT JOIN " +
                    $"pragma_foreign_key_list('{TableName}') AS foreign_key_list " +
                "ON foreign_key_list.[from] = table_info.name ";

            Dictionary<string, ColumnInfo> ColumnName_ColumnInfo             = new Dictionary<string, ColumnInfo>();
            Dictionary<string, string> LinkedTableName_LinkedTablePrimaryKey = new Dictionary<string, string>();
            string PrimaryKey = null;
            string СвязанныйРегистр=null;
            string РесурсСвязянногоРегистра=null;
            TableInfo.Types TableType = TableInfo.Types.table;

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command1 = new SQLiteCommand(queryAboutTableType, connection))
                {
                    using (SQLiteDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TableType = reader.GetString(0)=="view" ? TableInfo.Types.view : TableInfo.Types.table;
                            if (!reader.IsDBNull(1))
                            {
                                СвязанныйРегистр = reader.GetString(1);
                                РесурсСвязянногоРегистра = reader.GetString(2);
                            }
                        }
                    }
                }

                using (SQLiteCommand command2 = new SQLiteCommand(queryAboutColumns, connection))
                {
                    using (SQLiteDataReader reader = command2.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            string Column       = reader.GetString(0);
                            string Type         = reader.GetString(1);
                            bool NotNullVal     = reader.GetBoolean(2);
                            bool IsItPrimaryKey = reader.GetBoolean(3);

                            string LinkedTable           = reader.IsDBNull(4) ? null : reader.GetString(4);
                            string LinkedTablePrimaryKey = reader.IsDBNull(5) ? null : reader.GetString(5);

                            var ColInf = new ColumnInfo(
                                Column,
                                (Type == "INTEGER" ? ColumnInfo.Types.INTEGER : (Type == "REAL" ? ColumnInfo.Types.REAL :(Type == "DATE" ? ColumnInfo.Types.DATE : ColumnInfo.Types.TEXT))),
                                NotNullVal,
                                Column == РесурсСвязянногоРегистра,
                                IsItPrimaryKey,
                                LinkedTable);
                            
                            ColumnName_ColumnInfo.Add(Column, ColInf);

                            if (LinkedTable != null)
                                LinkedTableName_LinkedTablePrimaryKey.Add(LinkedTable, LinkedTablePrimaryKey);

                            if (IsItPrimaryKey)
                                PrimaryKey = Column;

                        }
                    }
                }
            }

            return new TableInfo(TableName, PrimaryKey, СвязанныйРегистр, РесурсСвязянногоРегистра, TableType, ColumnName_ColumnInfo, LinkedTableName_LinkedTablePrimaryKey);

        }

        static public int GetAutoIncrement(string TableName, string Column) 
        {
            string queryString = "" +
                "WITH RECURSIVE cte_numbers AS ( " +
                    "SELECT 1 AS num " +
                    "UNION ALL " +
                    "SELECT num + 1 " +
                    "FROM cte_numbers " +
                    $"WHERE num <= (SELECT MAX({Column}) FROM {TableName})) " +
                "SELECT MIN(num) AS missing_num " +
                "FROM cte_numbers " +
                $"WHERE num NOT IN (SELECT {Column} FROM {TableName})";

            int result = -1;

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            result = reader.GetInt32(0);

                        }
                    }
                }
            }
            return result;

        }

        static public DataTable GetDataTableByQuery(string query) 
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        return dataTable;
                    }
                }
            }

        }

        static public int ExecuteNonQuery(string query) 
        {

            using (var connection = new SQLiteConnection($"Data Source={fullPath}; Version=3;"))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                {

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected;
                }
            }
        }

        static public void ДобавитьЗаписьВРегистр(string RegisterName, string TableName, string TablePrimaryKeyValue, string ИмяРесурса , string ЗначениеРесурса, DateTime date) 
        {
            var RegInfo = GetTableInfo(RegisterName);
            string TableForeignKeyName = RegInfo.ColumnName_ColumnInfo.FirstOrDefault(pair => pair.Value.LinkedTableName==TableName).Key;

            string query = "" +
                $"INSERT INTO {RegisterName}(Дата, {TableForeignKeyName}, {ИмяРесурса}) " +
                $"VALUES ('{date.ToShortDateString()}', '{TablePrimaryKeyValue}', '{ЗначениеРесурса}')";

            ExecuteNonQuery(query);
        }

    }
}
