using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZadarmaAPI
{
    /// <summary>
    /// Class implements Zadarma API
    /// </summary>
    public class ZadarmaApi
    {

        private const string UrlApi = "https://api.zadarma.com";
        private const string UrlApiSandbox = "https://api-sandbox.zadarma.com";

        private readonly string _baseAddress;
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Key from personal
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Secret from personal
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Is this client using a sandbox api
        /// </summary>
        public bool IsSandbox { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Key from personal</param>
        /// <param name="secret">Secret from personal</param>
        /// <param name="isSandbox">Sandbox (true|false)</param>
        public ZadarmaApi(string key, string secret, bool isSandbox=false)
        {
            Key = key;
            Secret = secret;
            IsSandbox = isSandbox;

            _baseAddress = IsSandbox ? UrlApiSandbox : UrlApi;
        }


        /// <summary>
        /// Async call to API
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameter dictionary</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Response message</returns>
        public async Task<HttpResponseMessage> CallAsync(string method, IDictionary<string, string> paramDictionary = null,
            HttpMethod requestType = null, string format = "json", bool isAuth = true)
        {

            var request = GenerateRequest(method, paramDictionary, requestType, format, isAuth);
            return await _httpClient.SendAsync(request);
        }

        /// <summary>
        /// Synchronized call to API
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameter dictionary</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Response message</returns>
        public HttpResponseMessage Call(string method, IDictionary<string, string> paramDictionary = null,
            HttpMethod requestType = null, string format = "json", bool isAuth = true)
        {
            var request = GenerateRequest(method, paramDictionary, requestType, format, isAuth);
            var result = _httpClient.SendAsync(request);
            return result.Result;
        }

        /// <summary>
        /// Generate request string 
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameter dictionary</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Request message</returns>
        private HttpRequestMessage GenerateRequest(string method, IDictionary<string, string> paramDictionary = null, HttpMethod requestType = null, string format = "json", bool isAuth = true)
        {
            if (paramDictionary == null) paramDictionary = new SortedDictionary<string, string>();
            paramDictionary["format"] = format;

            if (requestType == null) requestType = HttpMethod.Get;

            var authStr = isAuth ? GetAuthStringForHeader(method, paramDictionary) : "";

            string urlString;

            if (requestType != HttpMethod.Get) urlString = $"{_baseAddress}{method}";
            else
            {
                var sortedDictParams = new SortedDictionary<string, string>(paramDictionary);
                var paramsString = string.Join("&", sortedDictParams.Select(GetUrlEncodedParameter));
                urlString = $"{_baseAddress}{method}?{paramsString}";
            }

            HttpRequestMessage request = new HttpRequestMessage(requestType, urlString);

            if (isAuth) request.Headers.TryAddWithoutValidation("Authorization", authStr);

            if (requestType == HttpMethod.Get) return request;

            var content = new FormUrlEncodedContent(paramDictionary);
            request.Content = content;

            return request;
        }

        /// <summary>
        /// Generate auth header
        /// </summary>
        /// <param name="method">API method, including version number</param>
        /// <param name="paramDictionary">Query params dict</param>
        /// <returns>auth header</returns>
        private string GetAuthStringForHeader(string method, IDictionary<string, string> paramDictionary)
        {
            var sortedDictParams = new SortedDictionary<string, string>(paramDictionary);

            var paramString = string.Join("&", sortedDictParams.Select(GetUrlEncodedParameter));

            var auth = "";
            using (var md5Hash = MD5.Create())
            using (var hmacHash = new HMACSHA1(Encoding.UTF8.GetBytes(Secret)))
            {
                var md5HashString = string.Join(string.Empty,
                    md5Hash.ComputeHash(Encoding.UTF8.GetBytes(paramString)).Select(b => b.ToString("x2")));

                var data = $"{method}{paramString}{md5HashString}";

                var hmacHashString = ToHexString(hmacHash.ComputeHash(Encoding.UTF8.GetBytes(data)));

                auth = $"{Key}:{Convert.ToBase64String(Encoding.UTF8.GetBytes(hmacHashString))}";
            }

            return auth;
        }

        /// <summary>
        /// Generate hex string from bytes array (python's hexdigit analog)
        /// </summary>
        /// <param name="array">Bytes array</param>
        /// <returns>Hexdigest string</returns>
        private static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (var b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// URL encode key-value parameter
        /// </summary>
        /// <param name="keyValuePair">key-value parameter</param>
        /// <returns>URL encoded string</returns>
        private static string GetUrlEncodedParameter(KeyValuePair<string, string> keyValuePair) =>
            $"{System.Net.WebUtility.UrlEncode(keyValuePair.Key)}={System.Net.WebUtility.UrlEncode(keyValuePair.Value)}";

    }
}
