using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace Cursovaya
{
    public class User
    {
        public string Name;
        public string Password;
        public int id;
        public User(string name, string password, int id_)
        {
            Name = name;
            Password = password;
            id = id_;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class db
    {
        private static string fullPath = @"C:\Users\Lolban\Projects\Cursovaya\db.db";
        static public List<User> GetUsersList()
        {
            List<User> Users = new List<User>();

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

                            Users.Add(new User(Name, password, id));
                        }
                    }
                }
            }

            return Users;
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

    }
}
