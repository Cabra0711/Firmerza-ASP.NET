using Firmeza.DTOs;
using Firmeza.Enums;
using Firmeza.Models;
using Firmeza.Services.Interfaces;
using Firmeza.Validators;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Services;

public class ReportService : IReportService
{
    private readonly IProductService _productService;
    private readonly ILoginService _loginService;
    private readonly ProductValidator _productValidator;
    private readonly CustomerValidator _customerValidator;

    public ReportService(IProductService productService, ILoginService loginService, ProductValidator productValidator, CustomerValidator customerValidator)
    {
        _productService = productService;
        _loginService = loginService;
        _productValidator = productValidator;
        _customerValidator = customerValidator;
    }

    public async Task<ImportResultDto> ImportProducts(Stream excelStream)
    {
        var result = new ImportResultDto();
        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension?.Rows ?? 1;
        var headers = BuildHeaderMap(worksheet);

        var nameCol = FindColumn(headers, "Nombre", "Name");
        var descCol = FindColumn(headers, "Descripción", "Descripcion", "Description");
        var priceCol = FindColumn(headers, "Precio", "Price");
        var skuCol = FindColumn(headers, "SKU");
        var qtyCol = FindColumn(headers, "Cantidad", "Quantity");
        var categoryCol = FindColumn(headers, "Categoría", "Categoria", "Category");
        var imageCol = FindColumn(headers, "ImageUrl", "Imagen");

        if (nameCol < 0 || descCol < 0 || priceCol < 0 || qtyCol < 0 || categoryCol < 0)
        {
            result.Errors.Add("El archivo no tiene el formato esperado para importar productos. Columnas requeridas: Nombre, Descripción, Precio, Cantidad, Categoría (SKU e ImageUrl son opcionales). Nota: el archivo exportado desde 'Ventas/Productos' no es el mismo formato que el de importación.");
            return result;
        }

        for (var row = 2; row <= rowCount; row++)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, nameCol].Text))
            {
                continue;
            }

            result.TotalRows++;

            var product = new Product
            {
                Name = worksheet.Cells[row, nameCol].Text.Trim(),
                Description = worksheet.Cells[row, descCol].Text.Trim(),
                Price = decimal.TryParse(worksheet.Cells[row, priceCol].Text, out var price) ? price : 0,
                Sku = skuCol >= 0 ? worksheet.Cells[row, skuCol].Text.Trim() : string.Empty,
                Quantity = int.TryParse(worksheet.Cells[row, qtyCol].Text, out var quantity) ? quantity : 0,
                ImageUrl = imageCol >= 0 ? worksheet.Cells[row, imageCol].Text.Trim() : string.Empty,
            };

            if (!Enum.TryParse<ProductCategory>(worksheet.Cells[row, categoryCol].Text.Trim(), true, out var category))
            {
                result.Errors.Add($"Fila {row}: la categoría '{worksheet.Cells[row, categoryCol].Text}' no es válida.");
                continue;
            }
            product.Category = category;

            var validation = await _productValidator.ValidateAsync(product);
            if (!validation.IsValid)
            {
                result.Errors.Add($"Fila {row}: {string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))}");
                continue;
            }

            var creationResponse = await _productService.CreateProduct(product);
            if (!creationResponse.Success)
            {
                result.Errors.Add($"Fila {row}: {creationResponse.Message}");
                continue;
            }

            result.SuccessCount++;
        }

        return result;
    }

    public async Task<ImportResultDto> ImportCustomers(Stream excelStream)
    {
        var result = new ImportResultDto();
        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension?.Rows ?? 1;
        var headers = BuildHeaderMap(worksheet);

        var userNameCol = FindColumn(headers, "UserName", "Usuario");
        var emailCol = FindColumn(headers, "Email", "Correo");
        var passwordCol = FindColumn(headers, "Password", "Contraseña", "Contrasena");
        var documentCol = FindColumn(headers, "Document", "Documento");

        if (userNameCol < 0 || emailCol < 0 || passwordCol < 0 || documentCol < 0)
        {
            result.Errors.Add("El archivo no tiene el formato esperado para importar clientes. Columnas requeridas: UserName, Email, Password, Document. Nota: el archivo exportado desde 'Clientes' no incluye contraseñas y no sirve como plantilla de importación.");
            return result;
        }

        for (var row = 2; row <= rowCount; row++)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, userNameCol].Text))
            {
                continue;
            }

            result.TotalRows++;

            var customer = new Customer
            {
                UserName = worksheet.Cells[row, userNameCol].Text.Trim(),
                Email = worksheet.Cells[row, emailCol].Text.Trim(),
                Password = worksheet.Cells[row, passwordCol].Text.Trim(),
                Document = worksheet.Cells[row, documentCol].Text.Trim(),
            };

            var validation = await _customerValidator.ValidateAsync(customer);
            if (!validation.IsValid)
            {
                result.Errors.Add($"Fila {row}: {string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))}");
                continue;
            }

            var creationResponse = await _loginService.CreateCustomer(customer);
            if (!creationResponse.Success)
            {
                result.Errors.Add($"Fila {row}: {creationResponse.Message}");
                continue;
            }

            result.SuccessCount++;
        }

        return result;
    }

    private static Dictionary<string, int> BuildHeaderMap(ExcelWorksheet worksheet)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var columns = worksheet.Dimension?.Columns ?? 0;
        for (var col = 1; col <= columns; col++)
        {
            var header = worksheet.Cells[1, col].Text.Trim();
            if (!string.IsNullOrWhiteSpace(header) && !map.ContainsKey(header))
            {
                map[header] = col;
            }
        }
        return map;
    }

    private static int FindColumn(Dictionary<string, int> headers, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            if (headers.TryGetValue(name, out var col))
            {
                return col;
            }
        }
        return -1;
    }

    public byte[] ExportProductsToExcel(IEnumerable<Product> products)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Productos");
        string[] headers = ["Nombre", "Descripción", "Precio", "SKU", "Cantidad", "Categoría", "Estado"];
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        var row = 2;
        foreach (var product in products)
        {
            ws.Cells[row, 1].Value = product.Name;
            ws.Cells[row, 2].Value = product.Description;
            ws.Cells[row, 3].Value = product.Price;
            ws.Cells[row, 4].Value = product.Sku;
            ws.Cells[row, 5].Value = product.Quantity;
            ws.Cells[row, 6].Value = product.Category.ToString();
            ws.Cells[row, 7].Value = product.Status.ToString();
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportProductsToPdf(IEnumerable<Product> products)
    {
        var data = products.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Reporte de Productos - Firmeza").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("SKU").Bold();
                        header.Cell().Text("Precio").Bold();
                        header.Cell().Text("Cantidad").Bold();
                        header.Cell().Text("Categoría").Bold();
                    });

                    foreach (var product in data)
                    {
                        table.Cell().Text(product.Name);
                        table.Cell().Text(product.Sku);
                        table.Cell().Text(product.Price.ToString("N2"));
                        table.Cell().Text(product.Quantity.ToString());
                        table.Cell().Text(product.Category.ToString());
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generado el ");
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportCustomersToExcel(IEnumerable<Customer> customers)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Clientes");
        string[] headers = ["Usuario", "Correo", "Documento", "Estado"];
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        var row = 2;
        foreach (var customer in customers)
        {
            ws.Cells[row, 1].Value = customer.UserName;
            ws.Cells[row, 2].Value = customer.Email;
            ws.Cells[row, 3].Value = customer.Document;
            ws.Cells[row, 4].Value = customer.IsActive ? "Activo" : "Inactivo";
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportCustomersToPdf(IEnumerable<Customer> customers)
    {
        var data = customers.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Reporte de Clientes - Firmeza").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Usuario").Bold();
                        header.Cell().Text("Correo").Bold();
                        header.Cell().Text("Documento").Bold();
                        header.Cell().Text("Estado").Bold();
                    });

                    foreach (var customer in data)
                    {
                        table.Cell().Text(customer.UserName);
                        table.Cell().Text(customer.Email);
                        table.Cell().Text(customer.Document);
                        table.Cell().Text(customer.IsActive ? "Activo" : "Inactivo");
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generado el ");
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportSalesToExcel(IEnumerable<Sale> sales)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Ventas");
        string[] headers = ["N° Venta", "Cliente", "Subtotal", "Impuesto", "Total", "Estado", "Fecha"];
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        var row = 2;
        foreach (var sale in sales)
        {
            ws.Cells[row, 1].Value = sale.SaleNumber;
            ws.Cells[row, 2].Value = sale.Customer?.UserName ?? "N/A";
            ws.Cells[row, 3].Value = sale.SubTotal;
            ws.Cells[row, 4].Value = sale.Tax;
            ws.Cells[row, 5].Value = sale.Total;
            ws.Cells[row, 6].Value = sale.Status.ToString();
            ws.Cells[row, 7].Value = sale.SaleDate.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportSalesToPdf(IEnumerable<Sale> sales)
    {
        var data = sales.ToList();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Reporte de Ventas - Firmeza").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("N° Venta").Bold();
                        header.Cell().Text("Cliente").Bold();
                        header.Cell().Text("Total").Bold();
                        header.Cell().Text("Estado").Bold();
                        header.Cell().Text("Fecha").Bold();
                    });

                    foreach (var sale in data)
                    {
                        table.Cell().Text(sale.SaleNumber);
                        table.Cell().Text(sale.Customer?.UserName ?? "N/A");
                        table.Cell().Text(sale.Total.ToString("N2"));
                        table.Cell().Text(sale.Status.ToString());
                        table.Cell().Text(sale.SaleDate.ToString("yyyy-MM-dd"));
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generado el ");
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }).GeneratePdf();
    }
}
