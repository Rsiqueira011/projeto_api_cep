using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using WebRequestHelper.Classes;

namespace NegocieOnlineAPI.ViaCEP
{
    public class WebRequestHelperNS
    {
        public enum Method
        {
            GET, POST
        }
        public enum ContentType
        {
            NONE, JSON, URLEncoded
        }

        public static WebRequestHelperResult Response(
            string URL,
            Method method,
            Dictionary<string, string> headers = null,
            Dictionary<string, object> parameters = null,
            ContentType contentType = ContentType.NONE,
            CookieContainer cookieContainer = null
            )
        {
            try
            {
                string data_string_write = null;

                if (contentType == ContentType.JSON)
                {
                    data_string_write = ConvertKeyValuePairToJSONString(parameters);
                }
                else if (contentType == ContentType.URLEncoded)
                {
                    data_string_write = GetURLEncoded(parameters);
                }

                HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
                request.Method = method.ToString();

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                if (contentType == ContentType.JSON)
                {
                    request.ContentType = "application/json";
                }
                else if (contentType == ContentType.URLEncoded)
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }

                //request.PreAuthenticate = true;
                //request.KeepAlive = true;
                //request.UserAgent = "coinmaster/122350 CFNetwork/1209 Darwin/20.2.0";
                //request.Host = "vik-game.moonactive.net";
                //request.AutomaticDecompression = DecompressionMethods.GZip; 
                //request.Accept = "application/json";
                //request.Timeout = 3600;
                //request.ServicePoint.ConnectionLimit = 30;
                //request.ServicePoint.Expect100Continue = true;

                if (data_string_write != null)
                {
                    byte[] data_byte_write = Encoding.UTF8.GetBytes(data_string_write);
                    request.ContentLength = data_byte_write.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data_byte_write, 0, data_byte_write.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    string data = ReadStream(response.GetResponseStream());

                    if (response != null)
                    {
                        return new WebRequestHelperResult(data, response.StatusCode);
                    }
                    else
                    {
                        return new WebRequestHelperResult(data, HttpStatusCode.ExpectationFailed);
                    }
                }
            }
            catch (WebException webException)
            {
                //INTERNAL ERROR SERVER 500 is not showed ??? maybe because ProtocolError is no hadling the +500
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    string data = ReadStream(webException.Response.GetResponseStream());

                    HttpStatusCode responseCode =
                        ((HttpWebResponse)webException.Response).StatusCode;

                    return new WebRequestHelperResult(data, responseCode);
                }
                else //(HttpWebResponse)webException.Response is null
                {
                    Console.WriteLine("Request.cs WebException supprimed: " + webException.Status);
                    return new WebRequestHelperResult(
                        new WebRequestHelperError(HttpStatusCode.ExpectationFailed, webException.Status.ToString()));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Request.cs Exception supprimed: " + exception.Message);
                return new WebRequestHelperResult(new WebRequestHelperError(exception.Message));
            }
        }

        private static string ReadStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetURLEncoded(Dictionary<string, object> parameters)
        {
            return string.Format("{0}", parameters.Count == 0 ? "" :
                string.Join("&", parameters.Select(
                        e => string.Format("{0}={1}",
                        HttpUtility.HtmlEncode(e.Key),
                        HttpUtility.HtmlEncode(e.Value.ToString())))
                        ));
        }
        private static string ConvertKeyValuePairToJSONString(Dictionary<string, object> parameters)
        {
            JObject json = new JObject();

            foreach (KeyValuePair<string, object> item in parameters)
            {
                if (item.Value.GetType() == typeof(JObject))
                {
                    json.Add(item.Key, (JObject)item.Value);
                }
                if (item.Value.GetType() == typeof(JArray))
                {
                    json.Add(item.Key, (JArray)item.Value);
                }
                else if (item.Value.GetType() == typeof(string))
                {
                    json.Add(item.Key, (string)item.Value);
                }
                else if (item.Value.GetType() == typeof(int))
                {
                    json.Add(item.Key, (int)item.Value);
                }
                else
                {
                    throw new Exception();
                }
            }

            return json.ToString();
        }
    }
}