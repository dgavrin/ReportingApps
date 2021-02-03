using System;

namespace Northwind.CurrencyServices.CountryCurrency
{
    public class ProductLocalPrice
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Country { get; set; }
        public decimal LocalPrice { get; set; }
        public string CurrencySymbol { get; set; }
    }
}