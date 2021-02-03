namespace Northwind.ReportingServices
{
    using System.Threading.Tasks;
    using Northwind.CurrencyServices.CountryCurrency;
    using Northwind.CurrencyServices.CurrencyExchange;

    public interface IProductReportService
    {
        public Task<ProductReport<ProductPrice>> GetCurrentProductsReport();
        public Task<ProductReport<ProductPrice>> GetMostExpensiveProductsReport(int count);
        public Task<ProductReport<ProductPrice>> GetProductsWithPriceLessThen(decimal price);
        public Task<ProductReport<ProductPrice>> GetProductPriceBetween(decimal moreThan, decimal lessThan);
        public Task<ProductReport<ProductPrice>> GetProductsWithPriceAboveAverage();
        public Task<ProductReport<ProductPrice>> GetProductsInDeficit();
        public Task<ProductReport<ProductLocalPrice>> GetCurrentProductsWithLocalCurrencyReport(ICountryCurrencyService countryCurrencyService, ICurrencyExchangeService currencyExchangeService);
    }
}