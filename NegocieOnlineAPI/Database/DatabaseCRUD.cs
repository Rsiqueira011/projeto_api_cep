using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NegocieOnlineAPI.Models;
using Newtonsoft.Json;

namespace NegocieOnlineAPI.Database
{
    public class DatabaseCRUD
    {
        public static bool Insere(ViaCEPResult result)
        {
            string query = "INSERT INTO cadastrocep (cep, logradouro, complemento, bairro, localidade, uf, ibge, gia, ddd, siafi) " +
                "VALUES(@cep, @logradouro, @complemento, @bairro, @localidade, @uf, @ibge, @gia, @ddd, @siafi);";

            List<DatabaseHelper.Parameter> parameters = new List<DatabaseHelper.Parameter>();
            parameters.Add(new DatabaseHelper.Parameter("@cep", result.cep));        
            parameters.Add(new DatabaseHelper.Parameter("@logradouro", result.logradouro));        
            parameters.Add(new DatabaseHelper.Parameter("@complemento", result.complemento ?? ""));        
            parameters.Add(new DatabaseHelper.Parameter("@bairro", result.bairro));        
            parameters.Add(new DatabaseHelper.Parameter("@localidade", result.localidade));        
            parameters.Add(new DatabaseHelper.Parameter("@uf", result.uf));        
            parameters.Add(new DatabaseHelper.Parameter("@ibge", result.ibge));        
            parameters.Add(new DatabaseHelper.Parameter("@gia", result.gia ?? ""));        
            parameters.Add(new DatabaseHelper.Parameter("@ddd", result.ddd));        
            parameters.Add(new DatabaseHelper.Parameter("@siafi", result.siafi));

            DatabaseHelper.QueryResponse response = DatabaseHelper.Query(query, false, parameters);
            if (response.error)
            {
                Debug.WriteLine(response.error_message);
                return false;
            }
            else
            {
                return true;
            }
        }
        public static ViaCEPResult Consulta(string CEP)
        {
            if (!CEP.Contains("-"))
            {
                CEP = CEP.Insert(CEP.Length - 3, "-");
            }

            //mysql JSON_OBJECT
            //postgree json_build_object
            string query =
                "SELECT json_build_object('cep', cep, 'logradouro', logradouro, 'complemento', complemento, 'bairro', bairro, 'localidade', localidade, 'uf', uf, 'ibge', ibge, 'gia', gia, 'ddd', ddd, 'siafi', siafi) AS ENDERECO " +
                "FROM cadastrocep " +
                "WHERE cep = @CEP;";

            List<DatabaseHelper.Parameter> parameters = new List<DatabaseHelper.Parameter>();
            parameters.Add(new DatabaseHelper.Parameter("@CEP", CEP));

            DatabaseHelper.QueryResponse response = DatabaseHelper.Query(query, true, parameters);
            if (!response.error)
            {
                IDataReader data = (IDataReader)response.data;

                ViaCEPResult result = new ViaCEPResult();

                if (data != null)
                {
                    while (data.Read())
                    {
                        result = JsonConvert.DeserializeObject<ViaCEPResult>((string)data["ENDERECO"]);
                    }

                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Debug.WriteLine(response.error_message);
                return null;
            }
        }
    }
}
