using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Northwind.CurrencyServices.CountryCurrency
{
    public class CountryInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("currencies")]
        public Dictionary<string, string>[] Currencies { get; set; }
    }
}
