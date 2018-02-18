using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace LinkShortener.Models.Database
{
    public partial class LinkDatabase : AbstractDatabase
    {
        private LinkDatabase() { }

        public static LinkDatabase getInstance()
        {
            if(instance == null)
            {
                instance = new LinkDatabase();
            }
            return instance;
        }

        public string saveReview(string url)
        {
            string body = url;
            string remove = "\"review\":";
            if (body.Contains(remove))
            {
                body = url.Substring(remove.Length + 1, url.Length - (remove.Length + 2));
            }


            dynamic data = JObject.Parse(body);

            string query = @"INSERT INTO " + dbname + ".Reviews(companyName, username, review, stars, timestamp)" +
                @"VALUES('" + data.companyName + @"','" + data.username + @"','" + data.review + @"','" + data.stars + @"','" +
                   data.timestamp + @"');";

            if (openConnection() == true)
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM " + dbname + ".Reviews WHERE id = LAST_INSERT_ID();";

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read() == true)
                {
                    string result = reader.GetInt64("id").ToString();
                    closeConnection();
                    return "{\"response\":\"success\"}";
                }
                else
                {
                    closeConnection();
                    throw new Exception("Error: LAST_INSERT_ID() did not work as intended.");
                }
            }
            else
            {
                throw new Exception("Could not connect to database");
            }
        }

   
        


        /// <summary>
        /// Gets a long URL based on the id of the short url
        /// </summary>
        /// <param name="id">The id of the short url</param>
        /// <throws type="ArgumentException">Throws an argument exception if the short url id does not refer to anything in the database</throws>
        /// <returns>The long url the given short url refers to</returns>
        public string getLongUrl(string id)
        {
            string query = @"SELECT * FROM " + dbname + ".shortenedLinks "
                + "WHERE id=" + id + ";";

            if(openConnection() == true)
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                if(reader.Read() == true)
                {
                    string originalUrl = reader.GetString("original");
                    reader.Close();
                    closeConnection();
                    return originalUrl;

                }
                else
                {
                    //Throw an exception indicating no result was found
                    throw new ArgumentException("No url in the database matches that id.");
                }
            }
            else
            {
                throw new Exception("Could not connect to database.");
            }
        }

        /// <summary>
        /// Saves the longURL to the database to be accessed later via the id that is returned.
        /// </summary>
        /// <param name="longURL">The longURL to be saved</param>
        /// <returns>The id of the url</returns>
        public string saveLongURL(string longURL)
        {
            string query = @"INSERT INTO " + dbname + ".shortenedLinks(original) "
                + @"VALUES('" + longURL + @"');";

            if(openConnection() == true)
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM " + dbname + ".shortenedLinks WHERE id = LAST_INSERT_ID();";

                MySqlDataReader reader = command.ExecuteReader();

                if(reader.Read() == true)
                {
                    string result = reader.GetInt64("id").ToString();
                    closeConnection();
                    return result.ToString();
                }
                else
                {
                    closeConnection();
                    throw new Exception("Error: LAST_INSERT_ID() did not work as intended.");
                }
            }
            else
            {
                throw new Exception("Could not connect to database");
            }
        }
    }

    public partial class LinkDatabase : AbstractDatabase
    {
        private static LinkDatabase instance = null;

        private const String dbname = "Links";
       // private const String dbname_2 = "Reviews";
        public override String databaseName { get; } = dbname;
        //public override String databaseName_2 { get; } = dbname_2;

        protected override Table[] tables { get; } =
        {
            // This represents the database schema
            new Table
            (
                dbname,
                "shortenedLinks",
                new Column[]
                {
                    new Column
                    (
                        "id", "INT(64)",
                        new string[]
                        {
                            "NOT NULL",
                            "UNIQUE",
                            "AUTO_INCREMENT"
                        }, true
                    ),
                    new Column
                    (
                        "original", "VARCHAR(300)",
                        new string[]
                        {
                            "NOT NULL"
                        }, false
                    )
                }
            ), new Table(dbname, "Reviews", new Column[] {new Column ("id", "INT(64)", new string[] { "NOT NULL", "UNIQUE", "AUTO_INCREMENT"}, true),
                                                           new Column ("companyName", "VARCHAR(300)", new string[] { "NOT NULL" }, false),
                                                           new Column ("username", "VARCHAR(300)", new string[] { "NOT NULL"}, false),
                                                           new Column ("review", "VARCHAR(400)", new string[] { "NOT NULL"}, false),
                                                           new Column ("stars", "VARCHAR(300)", new string[] { "NOT NULL"}, false),
                                                           new Column ("timestamp", "VARCHAR(300)", new string[] { "NOT NULL"}, false)
            })
        };
    }
}