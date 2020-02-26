using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ZadarmaAPI;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var zadarma = new ZadarmaApi("YOUR_KEY", "YOUR_SECRET");

            var response = zadarma.Call("/v1/tariff");
            var str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);

            var parameters = new SortedDictionary<string, string>()
            {
                { "number", "71234567890" },
                { "id", "123456" }
            };
            response = zadarma.Call("/v1/sip/callerid/", parameters, HttpMethod.Put);
            str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);

            parameters = new SortedDictionary<string, string>()
            {
                {"number", "71234567890"},
                {"caller_id", "70987654321"}
            };
            response = zadarma.Call("/v1/info/price/", parameters);
            str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);

            parameters = new SortedDictionary<string, string>()
            {
                {"end", "2020-02-24 05:00:00"},
                {"start", "2020-02-13 10:00:00"},
            };
            response = zadarma.Call("/v1/statistics/", parameters);
            str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);

            AsyncRequest(zadarma);

            response = zadarma.Call("/v1/info/balance/", format:"xml");
            str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);

        }

        static async Task AsyncRequest(ZadarmaApi zadarma)
        {
            var response = await zadarma.CallAsync("/v1/tariff");
            var str = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(str);
        }
    }
}
