using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Northwind.CurrencyServices.CurrencyExchange
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private static HttpClient httpClient = new HttpClient();
        private static CurrencyRateInfo currencyRateInfo;
        private readonly string accessKey;
        private readonly string source = "http://api.currencylayer.com/live?access_key=";

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyExchangeService"/> class.
        /// </summary>
        /// <param name="accesskey">The assecc key.</param>
        public CurrencyExchangeService(string accesskey)
        {
            this.accessKey = !string.IsNullOrWhiteSpace(accesskey) ? accesskey : throw new ArgumentException("Access key is invalid.", nameof(accesskey));
        }

        public async Task<decimal> GetCurrencyExchangeRate(string baseCurrency, string exchangeCurrency)
        {
            if (CurrencyExchangeService.currencyRateInfo is null)
            {
                CurrencyExchangeService.currencyRateInfo = await this.GetCurrencyRateInfo(baseCurrency, exchangeCurrency);
            }

            return CurrencyExchangeService.currencyRateInfo.Quotes[baseCurrency + exchangeCurrency];
        }

        private async Task<CurrencyRateInfo> GetCurrencyRateInfo(string baseCurrency, string exchangeCurrency)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var stream = await httpClient.GetStreamAsync(this.source + this.accessKey
                + "&source"+ baseCurrency);

            return await JsonSerializer.DeserializeAsync<CurrencyRateInfo>(stream);
        }
    }
}