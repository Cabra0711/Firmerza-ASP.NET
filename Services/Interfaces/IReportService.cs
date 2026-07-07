using Firmeza.DTOs;
using Firmeza.Models;

namespace Firmeza.Services.Interfaces;

public interface IReportService
{
    public Task<ImportResultDto> ImportProducts(Stream excelStream);
    public Task<ImportResultDto> ImportCustomers(Stream excelStream);

    public byte[] ExportProductsToExcel(IEnumerable<Product> products);
    public byte[] ExportProductsToPdf(IEnumerable<Product> products);

    public byte[] ExportCustomersToExcel(IEnumerable<Customer> customers);
    public byte[] ExportCustomersToPdf(IEnumerable<Customer> customers);

    public byte[] ExportSalesToExcel(IEnumerable<Sale> sales);
    public byte[] ExportSalesToPdf(IEnumerable<Sale> sales);
}
