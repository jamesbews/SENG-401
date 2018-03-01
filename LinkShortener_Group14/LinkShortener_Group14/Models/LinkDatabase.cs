using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            string remove = "\"review\":{";
            string query = "";
            if (body.Contains(remove))
            {
                body = url.Substring(remove.Length, url.Length - (remove.Length + 1));
            }


            dynamic data = JObject.Parse(body);

            if (data.companyName == null || data.username == null || data.review == null || data.review == null || data.stars == null || data.timestamp == null)
            {
                return "{\"response\":\"Invalid Json\"}";
            }
                query = @"INSERT INTO " + dbname + ".Reviews(companyName, username, review, stars, timestamp)" +
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
        /// Class for the serialization and deserialization of JSON data.
        /// </summary>
        private class CompanyReviewClass
        {
            public string companyName;
            public string username;
            public string review;
            public string stars;
            public string timestamp;
        }

        /// <summary>
        /// Method To get a review from the database.
        /// </summary>
        /// <param name="request">the url fed string representing the company name</param>
        /// <returns>a series of json objects that </returns>
        public string getReview(string request)
        {
            dynamic data;

            try
            {
                data = JObject.Parse(request);
            } catch (Exception e)
            {
                return "{\"response\":\"Invalid JSon\"}";
            }
            string output = "{\"response\":\"Success\",\"reviews\":[";
            int count = 0;
            string query = "SELECT * FROM " + dbname + ".Reviews WHERE companyname = '" + data.companyName + "';";
            if (openConnection() == true)
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                CompanyReviewClass CRC;
                while(reader.Read() == true)
                {
                    CRC = new CompanyReviewClass();
                    CRC.companyName = reader["companyname"].ToString();
                    CRC.username = reader["username"].ToString();
                    CRC.review = reader["review"].ToString();
                    CRC.stars = reader["stars"].ToString();
                    CRC.timestamp = reader["timestamp"].ToString();
                    if(count > 0)
                    {
                        output = output + "," + JsonConvert.SerializeObject(CRC);
                    } else
                    {
                        output = output + JsonConvert.SerializeObject(CRC);
                    }
                    
                    count++;
                }
                if(count <= 0)
                {
                    //no result was found
                    reader.Close();
                    closeConnection();
                    return "{\"response\":\"Failed\"}";
                }
                else
                {
                    reader.Close();
                    closeConnection();
                }
            }
            else
            {
                throw new Exception("Could not connect to database.");
            }
            output = output + "]}";
            return output;
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
