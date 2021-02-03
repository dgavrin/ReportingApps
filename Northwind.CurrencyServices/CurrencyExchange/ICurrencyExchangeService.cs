namespace Northwind.CurrencyServices.CurrencyExchange
{
    using System.Threading.Tasks;

    public interface ICurrencyExchangeService
    {
        Task<decimal> GetCurrencyExchangeRate(string baseCurrency, string exchangeCurrency);
    }
}