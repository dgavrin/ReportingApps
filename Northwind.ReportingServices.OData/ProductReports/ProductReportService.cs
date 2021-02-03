using Northwind.CurrencyServices.CountryCurrency;
using Northwind.CurrencyServices.CurrencyExchange;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Threading.Tasks;
using NorthwindProduct = NorthwindModel.Product;
using NorthwindSupplier = NorthwindModel.Supplier;

namespace Northwind.ReportingServices.OData.ProductReports
{
    /// <summary>
    /// Represents a service that produces product-related reports.
    /// </summary>
    public class ProductReportService : IProductReportService
    {
        private readonly NorthwindModel.NorthwindEntities entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductReportService"/> class.
        /// </summary>
        /// <param name="northwindServiceUri">An URL to Northwind OData service.</param>
        public ProductReportService(Uri northwindServiceUri)
        {
            this.entities = new NorthwindModel.NorthwindEntities(northwindServiceUri ?? throw new ArgumentNullException(nameof(northwindServiceUri)));
        }

        /// <summary>
        /// Gets a product report with all current products.
        /// </summary>
        /// <returns>Returns <see cref="ProductReport{T}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetCurrentProductsReport()
        {
            var prices = from p in await this.GetAllProducts()
                         where !p.Discontinued
                         orderby p.ProductName
                         select new ProductPrice
                         {
                             Name = p.ProductName,
                             Price = p.UnitPrice ?? 0,
                         };

            return new ProductReport<ProductPrice>(prices);
        }

        /// <summary>
        /// Gets a product report with most expensive products.
        /// </summary>
        /// <param name="count">Items count.</param>
        /// <returns>Returns <see cref="ProductReport{ProductPrice}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetMostExpensiveProductsReport(int count)
        {
            var prices = (await this.GetAllProducts()).
                            Where(p => p.UnitPrice != null).
                            OrderByDescending(p => p.UnitPrice.Value).
                            Take(count).
                            Select(p => new ProductPrice { Name = p.ProductName, Price = p.UnitPrice ?? 0 });

            return new ProductReport<ProductPrice>(prices);
        }

        /// <summary>
        /// Gets a product report with products with price less then price limit.
        /// </summary>
        /// <param name="price">Price limit.</param>
        /// <returns>Returns <see cref="ProductReport{ProductPrice}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetProductsWithPriceLessThen(decimal price)
        {
            var prices = from p in await this.GetAllProducts()
                         where p.UnitPrice < price
                         orderby p.UnitPrice
                         select new ProductPrice
                         {
                             Name = p.ProductName,
                             Price = p.UnitPrice ?? 0,
                         };

            return new ProductReport<ProductPrice>(prices);
        }

        /// <summary>
        /// Get a report of products with prices between two values.
        /// </summary>
        /// <param name="moreThan">Lower price limit.</param>
        /// <param name="lessThan">Upper price limit.</param>
        /// <returns>Returns <see cref="ProductReport{ProductPrice}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetProductPriceBetween(decimal moreThan, decimal lessThan)
        {
            var prices = from p in await this.GetAllProducts()
                         where p.UnitPrice > moreThan && p.UnitPrice < lessThan
                         orderby p.UnitPrice
                         select new ProductPrice
                         {
                             Name = p.ProductName,
                             Price = p.UnitPrice ?? 0,
                         };

            return new ProductReport<ProductPrice>(prices);
        }

        /// <summary>
        /// Get a report of products with price above average.
        /// </summary>
        /// <returns>Returns <see cref="ProductReport{ProductPrice}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetProductsWithPriceAboveAverage()
        {
            var products = await this.GetAllProducts();
            var averageProductsPrice = (from p in products select p.UnitPrice ?? 0).Average();
            var prices = from p in products
                         where p.UnitPrice > averageProductsPrice
                         orderby p.UnitPrice
                         select new ProductPrice
                         {
                             Name = p.ProductName,
                             Price = p.UnitPrice ?? 0,
                         };

            return new ProductReport<ProductPrice>(prices);
        }

        /// <summary>
        /// Get a report of products in deficit.
        /// </summary>
        /// <returns>Returns <see cref="ProductReport{ProductPrice}"/>.</returns>
        public async Task<ProductReport<ProductPrice>> GetProductsInDeficit()
        {
            var prices = from p in await this.GetAllProducts()
                         where p.UnitsInStock < p.UnitsOnOrder
                         orderby p.UnitPrice
                         select new ProductPrice
                         {
                             Name = p.ProductName,
                             Price = p.UnitPrice ?? 0,
                         };

            return new ProductReport<ProductPrice>(prices);
        }

        /// <inheritdoc/>
        public async Task<ProductReport<ProductLocalPrice>> GetCurrentProductsWithLocalCurrencyReport(ICountryCurrencyService countryCurrencyService, ICurrencyExchangeService currencyExchangeService)
        {
            var products = await this.GetAllProducts();
            var suppliers = await this.GetAllSupplies();
            var productsWithLocalPrices = products.Select(p => new ProductLocalPrice
            {
                Name = p.ProductName,
                Price = p.UnitPrice ?? 0,
                Country = suppliers.Where(s => s.SupplierID == p.SupplierID).Select(s => s.Country).First(),
            }).ToArray();

            for (int i = 0; i < productsWithLocalPrices.Length; i++)
            {
                var countryInfo = await countryCurrencyService.GetLocalCurrencyByCountry(productsWithLocalPrices[i].Country);
                var currencyExchangeRate = await currencyExchangeService.GetCurrencyExchangeRate("USD", countryInfo.CurrencyCode);
                productsWithLocalPrices[i].Country = countryInfo.CountryName;
                productsWithLocalPrices[i].CurrencySymbol = countryInfo.CurrencySymbol;
                productsWithLocalPrices[i].LocalPrice = productsWithLocalPrices[i].Price * currencyExchangeRate;
            }

            return new ProductReport<ProductLocalPrice>(productsWithLocalPrices);
        }

        private async Task<List<NorthwindProduct>> GetAllProducts()
        {
            DataServiceQueryContinuation<NorthwindProduct> token = null;
            var query = this.entities.Products;
            var products = new List<NorthwindProduct>();
            var result = await Task<IEnumerable<NorthwindProduct>>.Factory.FromAsync(query.BeginExecute(null, null), (ar) =>
            {
                return query.EndExecute(ar);
            }) as QueryOperationResponse<NorthwindProduct>;

            products.AddRange(result);
            token = result.GetContinuation();
            do
            {
                if (token is not null)
                {
                    result = await Task<IEnumerable<NorthwindProduct>>.Factory.FromAsync(this.entities.BeginExecute<NorthwindProduct>(token.NextLinkUri, null, null), (ar) =>
                    {
                        return this.entities.EndExecute<NorthwindProduct>(ar);
                    }) as QueryOperationResponse<NorthwindProduct>;
                }

                products.AddRange(result);
            }
            while ((token = result.GetContinuation()) is not null);

            return products;
        }

        private async Task<List<NorthwindSupplier>> GetAllSupplies()
        {
            DataServiceQueryContinuation<NorthwindSupplier> token = null;
            var query = this.entities.Suppliers;
            var suppliers = new List<NorthwindSupplier>();

            var result = await Task<IEnumerable<NorthwindSupplier>>.Factory.FromAsync(query.BeginExecute(null, null), (ar) =>
            {
                return query.EndExecute(ar);
            }) as QueryOperationResponse<NorthwindSupplier>;

            suppliers.AddRange(result);
            token = result.GetContinuation();
            do
            {
                if (token is not null)
                {
                    result = await Task<IEnumerable<NorthwindSupplier>>.Factory.FromAsync(query.BeginExecute(null, null), (ar) =>
                    {
                        return query.EndExecute(ar);
                    }) as QueryOperationResponse<NorthwindSupplier>;

                    suppliers.AddRange(result);
                }
            }
            while ((token = result.GetContinuation()) is not null);

            return suppliers;
        }
    }
}