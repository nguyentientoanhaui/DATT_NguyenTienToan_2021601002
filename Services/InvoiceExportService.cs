using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using ClosedXML.Excel;
using Shopping_Demo.Models;
using System.Text;

namespace Shopping_Demo.Services
{
    public class InvoiceExportService
    {
        private readonly IWebHostEnvironment _environment;

        public InvoiceExportService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// Xuất hóa đơn ra file PDF
        /// </summary>
        public byte[] ExportToPdf(InvoiceViewModel invoiceData, string template = "default")
        {
            try
            {
                Console.WriteLine($"DEBUG - ExportToPdf called with OrderCode: {invoiceData?.Order?.OrderCode}");
                
                // Null checks
                if (invoiceData == null)
                {
                    Console.WriteLine("DEBUG - invoiceData is null");
                    throw new ArgumentNullException(nameof(invoiceData), "Invoice data cannot be null");
                }
                
                if (invoiceData.Order == null)
                {
                    Console.WriteLine("DEBUG - invoiceData.Order is null");
                    throw new ArgumentNullException(nameof(invoiceData.Order), "Order cannot be null");
                }
                
                if (invoiceData.Order.OrderDetails == null || !invoiceData.Order.OrderDetails.Any())
                {
                    Console.WriteLine("DEBUG - OrderDetails is null or empty");
                    throw new InvalidOperationException("Order must have at least one order detail");
                }

                Console.WriteLine($"DEBUG - Creating PDF for order: {invoiceData.Order.OrderCode}");

                using (var memoryStream = new MemoryStream())
                {
                    Console.WriteLine("DEBUG - Creating PdfWriter");
                    var writer = new PdfWriter(memoryStream);
                    
                    Console.WriteLine("DEBUG - Creating PdfDocument");
                    var pdf = new PdfDocument(writer);
                    
                    Console.WriteLine("DEBUG - Creating Document");
                    var document = new Document(pdf);

                    Console.WriteLine("DEBUG - Creating font");
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    Console.WriteLine("DEBUG - Adding title");
                    document.Add(new Paragraph("INVOICE")
                        .SetFont(font).SetFontSize(16));
                    
                    Console.WriteLine("DEBUG - Adding order code");
                    document.Add(new Paragraph($"Order Code: {invoiceData.Order.OrderCode}")
                        .SetFont(font).SetFontSize(12));
                    
                    Console.WriteLine("DEBUG - Adding customer name");
                    document.Add(new Paragraph($"Customer: {invoiceData.Order.UserName ?? "N/A"}")
                        .SetFont(font).SetFontSize(12));
                    
                    Console.WriteLine("DEBUG - Adding date");
                    document.Add(new Paragraph($"Date: {invoiceData.Order.CreatedDate:yyyy-MM-dd}")
                        .SetFont(font).SetFontSize(12));
                    
                    document.Add(new Paragraph(""));

                    Console.WriteLine("DEBUG - Adding products header");
                    document.Add(new Paragraph("Products:")
                        .SetFont(font).SetFontSize(12));
                    
                    Console.WriteLine($"DEBUG - Adding {invoiceData.Order.OrderDetails.Count} products");
                    int index = 1;
                    foreach (var item in invoiceData.Order.OrderDetails)
                    {
                        var productName = item.Product?.Name ?? "Product";
                        var quantity = item.Quantity;
                        var price = item.Price;
                        
                        Console.WriteLine($"DEBUG - Adding product {index}: {productName}");
                        
                        document.Add(new Paragraph($"{index}. {productName} - Qty: {quantity} - Price: {price}")
                            .SetFont(font).SetFontSize(10));
                        index++;
                    }
                    
                    document.Add(new Paragraph(""));
                    
                    Console.WriteLine("DEBUG - Adding total");
                    document.Add(new Paragraph($"Total: {invoiceData.TotalAmount:N0} VND")
                        .SetFont(font).SetFontSize(12));

                    Console.WriteLine("DEBUG - Closing document");
                    document.Close();
                    
                    var result = memoryStream.ToArray();
                    Console.WriteLine($"DEBUG - PDF created successfully, size: {result.Length} bytes");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportToPdf: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Try to create a minimal PDF as fallback
                try
                {
                    Console.WriteLine("DEBUG - Attempting fallback PDF creation");
                    using (var memoryStream = new MemoryStream())
                    {
                        var writer = new PdfWriter(memoryStream);
                        var pdf = new PdfDocument(writer);
                        var document = new Document(pdf);
                        
                        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        document.Add(new Paragraph("INVOICE")
                            .SetFont(font).SetFontSize(16));
                        document.Add(new Paragraph($"Order: {invoiceData?.Order?.OrderCode ?? "N/A"}")
                            .SetFont(font).SetFontSize(12));
                        document.Add(new Paragraph($"Total: {invoiceData?.TotalAmount:N0 ?? 0} VND")
                            .SetFont(font).SetFontSize(12));
                        
                        document.Close();
                        var fallbackResult = memoryStream.ToArray();
                        Console.WriteLine($"DEBUG - Fallback PDF created, size: {fallbackResult.Length} bytes");
                        return fallbackResult;
                    }
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Fallback PDF creation failed: {fallbackEx.Message}");
                    throw new InvalidOperationException($"Lỗi khi tạo PDF: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Xuất hóa đơn ra file Excel
        /// </summary>
        public byte[] ExportToExcel(InvoiceViewModel invoiceData)
        {
            // Null checks
            if (invoiceData == null)
                throw new ArgumentNullException(nameof(invoiceData), "Invoice data cannot be null");
            
            if (invoiceData.Order == null)
                throw new ArgumentNullException(nameof(invoiceData.Order), "Order cannot be null");
            
            if (invoiceData.Order.OrderDetails == null || !invoiceData.Order.OrderDetails.Any())
                throw new InvalidOperationException("Order must have at least one order detail");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hóa đơn");

                // Header
                worksheet.Cell("A1").Value = "CỬA HÀNG ĐỒNG HỒ CAO CẤP";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Cell("A1").Style.Font.FontColor = XLColor.Blue;

                worksheet.Cell("A2").Value = "01-05, Tầng 1 Tràng Tiền Plaza";
                worksheet.Cell("A3").Value = "24 Hai Bà Trưng, Phường Cửa Nam";
                worksheet.Cell("A4").Value = "Quận Hoàn Kiếm, Thành phố Hà Nội";
                worksheet.Cell("A5").Value = "Điện thoại: 0388672928";
                worksheet.Cell("A6").Value = "Email: support@donghocaocap.com";

                worksheet.Cell("E1").Value = "HÓA ĐƠN BÁN HÀNG";
                worksheet.Cell("E1").Style.Font.Bold = true;
                worksheet.Cell("E1").Style.Font.FontSize = 16;
                worksheet.Cell("E1").Style.Font.FontColor = XLColor.Blue;

                worksheet.Cell("E2").Value = $"Mã hóa đơn: {invoiceData.Order.OrderCode}";
                worksheet.Cell("E3").Value = $"Ngày tạo: {invoiceData.Order.CreatedDate:dd/MM/yyyy HH:mm}";
                worksheet.Cell("E4").Value = $"Trạng thái: {(invoiceData.Order.Status == 1 ? "Đã xác nhận" : "Chờ xử lý")}";

                // Customer info
                int startRow = 8;
                worksheet.Cell($"A{startRow}").Value = "THÔNG TIN KHÁCH HÀNG";
                worksheet.Cell($"A{startRow}").Style.Font.Bold = true;
                worksheet.Cell($"A{startRow}").Style.Font.FontSize = 12;

                startRow++;
                worksheet.Cell($"A{startRow}").Value = $"Tên khách hàng: {invoiceData.Order.UserName}";
                startRow++;
                worksheet.Cell($"A{startRow}").Value = $"Địa chỉ giao hàng: {invoiceData.Order.ShippingAddress}";
                startRow++;
                worksheet.Cell($"A{startRow}").Value = $"{invoiceData.Order.ShippingWard}, {invoiceData.Order.ShippingDistrict}";
                startRow++;
                worksheet.Cell($"A{startRow}").Value = invoiceData.Order.ShippingCity;

                // Products table
                startRow += 2;
                worksheet.Cell($"A{startRow}").Value = "CHI TIẾT SẢN PHẨM";
                worksheet.Cell($"A{startRow}").Style.Font.Bold = true;
                worksheet.Cell($"A{startRow}").Style.Font.FontSize = 12;

                startRow++;
                worksheet.Cell($"A{startRow}").Value = "STT";
                worksheet.Cell($"B{startRow}").Value = "Tên sản phẩm";
                worksheet.Cell($"C{startRow}").Value = "Số lượng";
                worksheet.Cell($"D{startRow}").Value = "Đơn giá";
                worksheet.Cell($"E{startRow}").Value = "Thành tiền";
                worksheet.Range($"A{startRow}:E{startRow}").Style.Font.Bold = true;

                startRow++;
                int index = 1;
                foreach (var item in invoiceData.Order.OrderDetails)
                {
                    worksheet.Cell($"A{startRow}").Value = index;
                    worksheet.Cell($"B{startRow}").Value = item.Product?.Name ?? "N/A";
                    worksheet.Cell($"C{startRow}").Value = item.Quantity;
                    worksheet.Cell($"D{startRow}").Value = item.Price;
                    worksheet.Cell($"E{startRow}").Value = item.Price * item.Quantity;
                    startRow++;
                    index++;
                }

                // Summary
                startRow++;
                worksheet.Cell($"F{startRow}").Value = "Tạm tính:";
                worksheet.Cell($"G{startRow}").Value = invoiceData.Subtotal;

                if (invoiceData.DiscountAmount > 0)
                {
                    startRow++;
                    worksheet.Cell($"F{startRow}").Value = "Giảm giá:";
                    worksheet.Cell($"G{startRow}").Value = -invoiceData.DiscountAmount;
                }

                startRow++;
                worksheet.Cell($"F{startRow}").Value = "Phí vận chuyển:";
                worksheet.Cell($"G{startRow}").Value = invoiceData.ShippingCost;

                startRow++;
                worksheet.Cell($"F{startRow}").Value = "TỔNG CỘNG:";
                worksheet.Cell($"G{startRow}").Value = invoiceData.TotalAmount;
                worksheet.Cell($"F{startRow}:G{startRow}").Style.Font.Bold = true;

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Xuất hóa đơn ra file CSV
        /// </summary>
        public byte[] ExportToCsv(InvoiceViewModel invoiceData)
        {
            // Null checks
            if (invoiceData == null)
                throw new ArgumentNullException(nameof(invoiceData), "Invoice data cannot be null");
            
            if (invoiceData.Order == null)
                throw new ArgumentNullException(nameof(invoiceData.Order), "Order cannot be null");
            
            if (invoiceData.Order.OrderDetails == null || !invoiceData.Order.OrderDetails.Any())
                throw new InvalidOperationException("Order must have at least one order detail");

            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("CUA HANG DONG HO CAO CAP");
            csv.AppendLine("01-05, Tang 1 Trang Tien Plaza");
            csv.AppendLine("24 Hai Ba Trung, Phuong Cua Nam");
            csv.AppendLine("Quan Hoan Kiem, Thanh pho Ha Noi");
            csv.AppendLine("Dien thoai: 0388672928");
            csv.AppendLine("Email: support@donghocaocap.com");
            csv.AppendLine("");
            csv.AppendLine("HOA DON BAN HANG");
            csv.AppendLine($"Ma hoa don: {invoiceData.Order.OrderCode}");
            csv.AppendLine($"Ngay tao: {invoiceData.Order.CreatedDate:dd/MM/yyyy HH:mm}");
            csv.AppendLine($"Trang thai: {(invoiceData.Order.Status == 1 ? "Da xac nhan" : "Cho xu ly")}");
            csv.AppendLine("");
            csv.AppendLine("THONG TIN KHACH HANG");
            csv.AppendLine($"Ten khach hang: {invoiceData.Order.UserName}");
            csv.AppendLine($"Dia chi giao hang: {invoiceData.Order.ShippingAddress}");
            csv.AppendLine($"{invoiceData.Order.ShippingWard}, {invoiceData.Order.ShippingDistrict}");
            csv.AppendLine(invoiceData.Order.ShippingCity);
            csv.AppendLine("");
            csv.AppendLine("CHI TIET SAN PHAM");
            csv.AppendLine("STT,Ten san pham,So luong,Don gia,Thanh tien");
            
            int index = 1;
            foreach (var item in invoiceData.Order.OrderDetails)
            {
                csv.AppendLine($"{index},{item.Product?.Name ?? "N/A"},{item.Quantity},{item.Price},{item.Price * item.Quantity}");
                index++;
            }
            
            csv.AppendLine("");
            csv.AppendLine($"Tam tinh: {invoiceData.Subtotal:N0} VND");
            
            if (invoiceData.DiscountAmount > 0)
            {
                csv.AppendLine($"Giam gia: -{invoiceData.DiscountAmount:N0} VND");
            }
            
            csv.AppendLine($"Phi van chuyen: {invoiceData.ShippingCost:N0} VND");
            csv.AppendLine($"TONG CONG: {invoiceData.TotalAmount:N0} VND");

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        /// <summary>
        /// Xuất nhiều hóa đơn ra file Excel
        /// </summary>
        public byte[] ExportMultipleToExcel(List<InvoiceViewModel> invoicesData)
        {
            // Null checks
            if (invoicesData == null || !invoicesData.Any())
                throw new ArgumentNullException(nameof(invoicesData), "Invoices data cannot be null or empty");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh sách hóa đơn");

                // Header
                worksheet.Cell("A1").Value = "DANH SÁCH HÓA ĐƠN";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Cell("A1").Style.Font.FontColor = XLColor.Blue;

                // Table headers
                int startRow = 3;
                worksheet.Cell($"A{startRow}").Value = "STT";
                worksheet.Cell($"B{startRow}").Value = "Mã hóa đơn";
                worksheet.Cell($"C{startRow}").Value = "Tên khách hàng";
                worksheet.Cell($"D{startRow}").Value = "Ngày tạo";
                worksheet.Cell($"E{startRow}").Value = "Trạng thái";
                worksheet.Cell($"F{startRow}").Value = "Tổng tiền";
                worksheet.Range($"A{startRow}:F{startRow}").Style.Font.Bold = true;

                startRow++;
                int index = 1;
                foreach (var invoice in invoicesData)
                {
                    if (invoice?.Order != null)
                    {
                        worksheet.Cell($"A{startRow}").Value = index;
                        worksheet.Cell($"B{startRow}").Value = invoice.Order.OrderCode;
                        worksheet.Cell($"C{startRow}").Value = invoice.Order.UserName;
                        worksheet.Cell($"D{startRow}").Value = invoice.Order.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell($"E{startRow}").Value = invoice.Order.Status == 1 ? "Đã xác nhận" : "Chờ xử lý";
                        worksheet.Cell($"F{startRow}").Value = invoice.TotalAmount;
                        startRow++;
                        index++;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}