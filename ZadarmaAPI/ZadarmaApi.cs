using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private string _baseAddress;
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Key from personal
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Secret from personal
        /// </summary>
        public string Secret { get; set; }

        private bool _isSandbox;

        /// <summary>
        /// Is this client using a sandbox api
        /// </summary>
        public bool IsSandbox
        {
            get => _isSandbox;
            set
            {
                _isSandbox = value;
                _baseAddress = _isSandbox ? UrlApiSandbox : UrlApi;
            }
        }

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
            return await CallAsync(request);
        }

        /// <summary>
        /// Async call to API
        /// </summary>
        /// <param name="request">Request message</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> CallAsync(HttpRequestMessage request)
        {
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
            return Call(request);
        }

        /// <summary>
        /// Synchronized call to API
        /// </summary>
        /// <param name="request">Request message</param>
        /// <returns></returns>
        public HttpResponseMessage Call(HttpRequestMessage request)
        {
            var response = _httpClient.SendAsync(request);
            return response.Result;
        }

        /// <summary>
        /// Generate request message with content
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameter dictionary</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Request message</returns>
        public HttpRequestMessage GenerateRequest(string method, IDictionary<string, string> paramDictionary = null, HttpMethod requestType = null, string format = "json", bool isAuth = true)
        {
            var request = GenerateBaseRequest(method, paramDictionary, requestType, format, isAuth);

            request.Content = new FormUrlEncodedContent(paramDictionary);

            return request;
        }

        /// <summary>
        /// Generate request message with content type multipart/form-data
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameter dictionary</param>
        /// <param name="multipartContent">Multipart content (byte array)</param>
        /// <param name="multipartContentType">Multipart content type</param>
        /// <param name="multipartContentParamName">Multipart content parameter name</param>
        /// <param name="multipartContentName">Multipart content name (filename)</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Request message with multipart content</returns>
        public HttpRequestMessage GenerateMultipartRequest(string method, IDictionary<string, string> paramDictionary, byte[] multipartContent, string multipartContentType, string multipartContentParamName, string multipartContentName = "", HttpMethod requestType = null, string format = "json", bool isAuth = true)
        {
            requestType = requestType ?? HttpMethod.Post;

            var request = GenerateBaseRequest(method, paramDictionary, requestType, format);

            var content = new MultipartFormDataContent();

            var byteHttpContent = new ByteArrayContent(multipartContent);
            byteHttpContent.Headers.ContentType = new MediaTypeHeaderValue(multipartContentType);
            content.Add(byteHttpContent, multipartContentParamName, multipartContentName);

            foreach (var param in paramDictionary)
            {
                content.Add(new StringContent(param.Value), param.Key);
            }

            request.Content = content;

            return request;
        }

        /// <summary>
        /// Generate basic request with no content
        /// </summary>
        /// <param name="method">API method (i.e. "/v1/tariff/")</param>
        /// <param name="paramDictionary">Parameters for auth string calculation</param>
        /// <param name="requestType">Type of request (PUT, POST, GET, DELETE)</param>
        /// <param name="format">Response format (xml or json)</param>
        /// <param name="isAuth">Is auth needed (True or False)</param>
        /// <returns>Request message with no content set</returns>
        public HttpRequestMessage GenerateBaseRequest(string method, IDictionary<string, string> paramDictionary = null, HttpMethod requestType = null, string format = "json", bool isAuth = true)
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
                var paramsString = GetUrlEncodedStringOfParameters(sortedDictParams);
                urlString = $"{_baseAddress}{method}?{paramsString}";
            }

            var request = new HttpRequestMessage(requestType, urlString);

            if (isAuth) request.Headers.TryAddWithoutValidation("Authorization", authStr);

            if (requestType == HttpMethod.Get) return request;

            return request;
        }

        /// <summary>
        /// Generate auth header
        /// </summary>
        /// <param name="method">API method, including version number</param>
        /// <param name="paramDictionary">Query params dict</param>
        /// <returns>auth header</returns>
        public string GetAuthStringForHeader(string method, IDictionary<string, string> paramDictionary)
        {
            var sortedDictParams = new SortedDictionary<string, string>(paramDictionary);

            var paramString = GetUrlEncodedStringOfParameters(sortedDictParams);

            var auth = "";
            using (var md5Hash = MD5.Create())
            using (var hmacHash = new HMACSHA1(Encoding.UTF8.GetBytes(Secret)))
            {
                var md5HashString = HexDigestFunc(md5Hash.ComputeHash, paramString);

                var data = $"{method}{paramString}{md5HashString}";

                var hmacHashString = HexDigestFunc(hmacHash.ComputeHash, data);

                auth = $"{Key}:{Convert.ToBase64String(Encoding.UTF8.GetBytes(hmacHashString))}";
            }

            return auth;
        }

        /// <summary>
        /// Uses function on data (bytes) and then transforms result to hex string
        /// </summary>
        /// <param name="function">Function</param>
        /// <param name="data">Data</param>
        /// <returns>Hex string</returns>
        private static string HexDigestFunc(Func<byte[], byte[]> function, string data) => string.Join(string.Empty,
            function(Encoding.UTF8.GetBytes(data)).Select(b => b.ToString("x2")));

        /// <summary>
        /// URL encode all key-value parameters of IDictionary
        /// </summary>
        /// <param name="parameters">Parameters (dict or another key-value collection)</param>
        /// <returns>URL encoded string</returns>
        private static string GetUrlEncodedStringOfParameters(IDictionary<string, string> parameters) =>
            string.Join("&", parameters.Select(GetUrlEncodedParameter));

        /// <summary>
        /// URL encode key-value parameter
        /// </summary>
        /// <param name="keyValuePair">key-value parameter</param>
        /// <returns>URL encoded string</returns>
        private static string GetUrlEncodedParameter(KeyValuePair<string, string> keyValuePair) =>
            $"{System.Net.WebUtility.UrlEncode(keyValuePair.Key)}={System.Net.WebUtility.UrlEncode(keyValuePair.Value)}";

    }
}
