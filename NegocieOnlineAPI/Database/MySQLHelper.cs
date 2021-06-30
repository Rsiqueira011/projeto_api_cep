using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NegocieOnlineAPI.Database
{
    public class MySqlHelper
    {
        public class Parameter
        {
            public string key;
            public object value;

            public Parameter(string key, object value)
            {
                this.key = key;
                this.value = value;
            }
        }
        public class QueryResponse
        {
            public object data = null;
            public int affected_rows;
            public bool error = false;
            public string error_message = null;

            public QueryResponse(object data, bool error = false, string error_message = null)
            {
                this.data = data;
                this.error = error;
                this.error_message = error_message;
            }
            public QueryResponse(int affected_rows)
            {
                this.affected_rows = affected_rows;
            }
        }

        protected readonly static string connectionString =
            "SERVER=localhost; DATABASE=world; UID=root; PWD=123456; PORT=; Allow User Variables=True;";

        public static QueryResponse Query(string query, bool readData, List<Parameter> parameters = null)
        {
            using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
            {
                try
                {
                    mySqlConnection.Open();
                    MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction();

                    try
                    {
                        using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection, mySqlTransaction))
                        {
                            if (parameters != null && parameters.Count() > 0)
                            {
                                foreach (Parameter parameter in parameters)
                                {
                                    mySqlCommand.Parameters.AddWithValue(parameter.key, parameter.value);
                                }
                            }

                            if (readData)
                            {
                                DataTable result = null;

                                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                                {
                                    if (mySqlDataReader.HasRows)
                                    {
                                        result = new DataTable();
                                        result.Load(mySqlDataReader);
                                    }
                                }

                                mySqlTransaction.Commit();
                                return new QueryResponse(result?.CreateDataReader());
                            }
                            else
                            {
                                int affected_rows = mySqlCommand.ExecuteNonQuery();
                                mySqlTransaction.Commit();
                                return new QueryResponse(affected_rows);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        mySqlTransaction.Rollback();
                        return new QueryResponse(null, true, exception.ToString());
                    }

                }
                catch (Exception exception)
                {
                    return new QueryResponse(null, true, exception.ToString());
                }
            }
        }
        public static string EscapeString(string value)
        {
            return MySqlHelper.EscapeString(value);
        }
    }
}