using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NegocieOnlineAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebRequestHelper;
using WebRequestHelper.Classes;

namespace NegocieOnlineAPI.ViaCEP
{
    public class ViaCEPAPI
    {
        public static ViaCEPResult Consulta(string CEP)
        {
            string URL = String.Format("https://viacep.com.br/ws/{0}/json/", CEP);

            WebRequestHelperResult result = WebRequestHelperNS.Response(URL,
                WebRequestHelperNS.Method.GET,
                null, null,
                WebRequestHelperNS.ContentType.NONE,
                null);

            if(result.error == null)

                if (IsValidJson(result.data))
                {
                    return JsonConvert.DeserializeObject<ViaCEPResult>(result.data);
                }
                else
                {
                    return null;
                }                
            else
            {
                throw new Exception("erro ao consultar CEP");
            }
        }

        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false;}
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}