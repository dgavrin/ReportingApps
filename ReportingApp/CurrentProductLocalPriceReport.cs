namespace ReportingApp
{
    using System;
    using Northwind.CurrencyServices.CountryCurrency;
    using Northwind.CurrencyServices.CurrencyExchange;
    using Northwind.ReportingServices;
    using System.Threading.Tasks;

    public class CurrentProductLocalPriceReport
    {
        private readonly IProductReportService productReportService;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly ICountryCurrencyService countryCurrencyService;

        public CurrentProductLocalPriceReport(IProductReportService productReportService, ICurrencyExchangeService currencyExchangeService, ICountryCurrencyService countryCurrencyService)
        {
            this.productReportService = productReportService ?? throw new ArgumentNullException(nameof(productReportService));
            this.currencyExchangeService = currencyExchangeService ?? throw new ArgumentNullException(nameof(currencyExchangeService));
            this.countryCurrencyService = countryCurrencyService ?? throw new ArgumentNullException(nameof(countryCurrencyService));
        }

        public async Task PrintReport()
        {
            var report = await this.productReportService.GetCurrentProductsWithLocalCurrencyReport(this.countryCurrencyService, this.currencyExchangeService);
            Console.WriteLine($"Report - products with local prices:");
            foreach (var reportLine in report.Products)
            {
                Console.WriteLine($"{reportLine.Name}, " +
                                  $"{reportLine.Price}, " +
                                  $"{reportLine.Country}, " +
                                  $"{reportLine.LocalPrice}, " +
                                  $"{reportLine.CurrencySymbol}");
            }
        }
    }
}