using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NegocieOnlineAPI.Database
{
    public class DatabaseHelper
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
            "Server=no-db-dev-101.negocieonline.com.br;Username=test;Password=Sacapp@2020;Database=db_selecao_imdb;";

        public static QueryResponse Query(string query, bool readData, List<Parameter> parameters = null)
        {
            using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    npgsqlConnection.Open();
                    NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction();

                    try
                    {
                        using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(query, npgsqlConnection, npgsqlTransaction))
                        {
                            if (parameters != null && parameters.Count() > 0)
                            {
                                foreach (Parameter parameter in parameters)
                                {
                                    npgsqlCommand.Parameters.AddWithValue(parameter.key, parameter.value);
                                }
                            }

                            if (readData)
                            {
                                DataTable result = null;

                                using (NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader())
                                {
                                    if (npgsqlDataReader.HasRows)
                                    {
                                        result = new DataTable();
                                        result.Load(npgsqlDataReader);
                                    }
                                }

                                npgsqlTransaction.Commit();
                                return new QueryResponse(result?.CreateDataReader());
                            }
                            else
                            {
                                int affected_rows = npgsqlCommand.ExecuteNonQuery();
                                npgsqlTransaction.Commit();
                                return new QueryResponse(affected_rows);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        npgsqlTransaction.Rollback();
                        return new QueryResponse(null, true, exception.ToString());
                    }

                }
                catch (Exception exception)
                {
                    return new QueryResponse(null, true, exception.ToString());
                }
            }
        }
        /*
        public static string EscapeString(string value)
        {
            return NpgsqlHelper.EscapeString(value);
        }*/
    }
}