using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Northwind.CurrencyServices.CurrencyExchange
{
    public class CurrencyRateInfo
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("quotes")]
        public Dictionary<string, decimal> Quotes { get; set; }
    }
}
