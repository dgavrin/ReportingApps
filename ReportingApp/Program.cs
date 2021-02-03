using System;
using System.Globalization;
using System.Threading.Tasks;
using Northwind.CurrencyServices.CountryCurrency;
using Northwind.CurrencyServices.CurrencyExchange;
using Northwind.ReportingServices;
using Northwind.ReportingServices.OData.ProductReports;

namespace ReportingApp
{
    /// <summary>
    /// Program class.
    /// </summary>
    public sealed class Program
    {
        private const string NorthwindServiceUrl = "https://services.odata.org/V3/Northwind/Northwind.svc";
        private const string CurrentProductsReport = "current-products";
        private const string MostExpensiveProductsReport = "most-expensive-products";
        private const string PriceLessThenProductsReport = "price-less-then-products";
        private const string PriceBetweenProductsReport = "price-between-products";
        private const string PriceAboveAverageProductsReport = "price-above-average-products";
        private const string UnitsInStockDeficitReport = "units-in-stock-deficit";
        private const string CurrentProductsLocalPricesReport = "current-products-local-prices";

        /// <summary>
        /// A program entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                ShowHelp();
                return;
            }

            var reportName = args[0];

            if (string.Equals(reportName, CurrentProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                await ShowCurrentProducts();
                return;
            }
            else if (string.Equals(reportName, MostExpensiveProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length > 1 && int.TryParse(args[1], out int count))
                {
                    await ShowMostExpensiveProducts(count);
                    return;
                }
            }
            else if (string.Equals(reportName, PriceLessThenProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length > 1)
                {
                    var price = Convert.ToDecimal(args[1], CultureInfo.InvariantCulture);
                    await ShowProductsWithPriceLessThen(price);
                    return;
                }
            }
            else if (string.Equals(reportName, PriceBetweenProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length > 2)
                {
                    var moreThan = Convert.ToDecimal(args[1], CultureInfo.InvariantCulture);
                    var lessThan = Convert.ToDecimal(args[2], CultureInfo.InvariantCulture);
                    await ShowProductsPriceBetween(moreThan, lessThan);
                    return;
                }
            }
            else if (string.Equals(reportName, PriceAboveAverageProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                await ShowPriceAboveAverageProducts();
                return;
            }
            else if (string.Equals(reportName, UnitsInStockDeficitReport, StringComparison.InvariantCultureIgnoreCase))
            {
                await ShowProductsInDeficit();
                return;
            }
            else if (string.Equals(reportName, CurrentProductsLocalPricesReport, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length > 1)
                {
                    await ShowLocalPricesCurrentProducts(args[1]);
                    return;
                }
            }
            else
            {
                ShowHelp();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tReportingApp.exe <report> <report-argument1> <report-argument2> ...");
            Console.WriteLine();
            Console.WriteLine("Reports:");
            Console.WriteLine($"\t{CurrentProductsReport}\t\tShows current products.");
            Console.WriteLine($"\t{MostExpensiveProductsReport}\t\tShows specified number of the most expensive products.");
        }

        private static async Task ShowCurrentProducts()
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetCurrentProductsReport();
            PrintProductReport("current products:", report);
        }

        private static async Task ShowMostExpensiveProducts(int count)
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetMostExpensiveProductsReport(count);
            PrintProductReport($"{count} most expensive products:", report);
        }

        private static async Task ShowProductsWithPriceLessThen(decimal price)
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetProductsWithPriceLessThen(price);
            PrintProductReport($"products with price less then {price}:", report);
        }

        private static async Task ShowProductsPriceBetween(decimal moreThan, decimal lessThan)
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetProductPriceBetween(moreThan, lessThan);
            PrintProductReport($"products with price between {moreThan} and {lessThan}:", report);
        }

        private static async Task ShowPriceAboveAverageProducts()
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetProductsWithPriceAboveAverage();
            PrintProductReport($"products with price above average:", report);
        }

        private static async Task ShowProductsInDeficit()
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetProductsInDeficit();
            PrintProductReport($"products in deficit:", report);
        }

        private static async Task ShowLocalPricesCurrentProducts(string asseccKey)
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var countryCurrencyService = new CountryCurrencyService();
            var currencyExchangeService = new CurrencyExchangeService(asseccKey);
            var report = new CurrentProductLocalPriceReport(service, currencyExchangeService, countryCurrencyService);
            await report.PrintReport();
        }

        private static void PrintProductReport(string header, ProductReport<ProductPrice> productReport)
        {
            Console.WriteLine($"Report - {header}");
            foreach (var reportLine in productReport.Products)
            {
                Console.WriteLine("{0}, {1}", reportLine.Name, reportLine.Price);
            }
        }
    }
}