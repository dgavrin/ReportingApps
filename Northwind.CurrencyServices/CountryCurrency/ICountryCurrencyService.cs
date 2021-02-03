namespace Northwind.CurrencyServices.CountryCurrency
{
    using System.Threading.Tasks;

    public interface ICountryCurrencyService
    {
        Task<LocalCurrency> GetLocalCurrencyByCountry(string countryName);
    }
}