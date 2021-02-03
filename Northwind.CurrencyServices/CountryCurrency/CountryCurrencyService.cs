namespace Northwind.CurrencyServices.CountryCurrency
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CountryCurrencyService : ICountryCurrencyService
    {
        private const string Source = "https://restcountries.eu/rest/v2/name/";
        private HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets the currency of the country.
        /// </summary>
        /// <param name="countryName">The country name.</param>
        /// <returns>Currency of the country.</returns>
        public async Task<LocalCurrency> GetLocalCurrencyByCountry(string countryName)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var stream = await httpClient.GetStreamAsync(new Uri(Source + countryName + "?fullText=true"));

            var country = (await JsonSerializer.DeserializeAsync<CountryInfo[]>(stream))[0];

            return new LocalCurrency
            {
                CountryName = country.Name,
                CurrencyCode = country.Currencies[0]["code"],
                CurrencySymbol = country.Currencies[0]["symbol"],
            };
        }
    }
}