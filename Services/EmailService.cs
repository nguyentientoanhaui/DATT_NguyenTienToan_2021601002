using MailKit.Net.Smtp;
using MimeKit;
using Shopping_Demo.Models;

namespace Shopping_Demo.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly InvoiceExportService _invoiceExportService;

        public EmailService(IConfiguration configuration, InvoiceExportService invoiceExportService)
        {
            _configuration = configuration;
            _invoiceExportService = invoiceExportService;
        }

        /// <summary>
        /// Gửi hóa đơn qua email
        /// </summary>
        public async Task<bool> SendInvoiceEmailAsync(InvoiceViewModel invoiceData, string recipientEmail, string format = "pdf")
        {
            try
            {
                // Null checks
                if (invoiceData == null)
                {
                    Console.WriteLine("Error: Invoice data is null");
                    return false;
                }
                
                if (invoiceData.Order == null)
                {
                    Console.WriteLine("Error: Order is null");
                    return false;
                }
                
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    Console.WriteLine("Error: Recipient email is null or empty");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cửa hàng đồng hồ cao cấp", _configuration["EmailSettings:FromEmail"]));
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = $"Hóa đơn thanh toán - {invoiceData.Order.OrderCode}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateEmailBody(invoiceData);

                // Đính kèm file hóa đơn
                byte[] attachmentData;
                string fileName;
                string contentType;

                switch (format.ToLower())
                {
                    case "pdf":
                        attachmentData = _invoiceExportService.ExportToPdf(invoiceData);
                        fileName = $"HoaDon_{invoiceData.Order.OrderCode}.pdf";
                        contentType = "application/pdf";
                        break;
                    case "excel":
                        attachmentData = _invoiceExportService.ExportToExcel(invoiceData);
                        fileName = $"HoaDon_{invoiceData.Order.OrderCode}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case "csv":
                        attachmentData = _invoiceExportService.ExportToCsv(invoiceData);
                        fileName = $"HoaDon_{invoiceData.Order.OrderCode}.csv";
                        contentType = "text/csv";
                        break;
                    default:
                        attachmentData = _invoiceExportService.ExportToPdf(invoiceData);
                        fileName = $"HoaDon_{invoiceData.Order.OrderCode}.pdf";
                        contentType = "application/pdf";
                        break;
                }

                bodyBuilder.Attachments.Add(fileName, attachmentData, ContentType.Parse(contentType));
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], 
                        int.Parse(_configuration["EmailSettings:SmtpPort"]), 
                        MailKit.Security.SecureSocketOptions.StartTls);
                    
                    await client.AuthenticateAsync(_configuration["EmailSettings:Username"], 
                        _configuration["EmailSettings:Password"]);
                    
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi nhiều hóa đơn qua email
        /// </summary>
        public async Task<bool> SendMultipleInvoicesEmailAsync(List<InvoiceViewModel> invoicesData, string recipientEmail, string format = "excel")
        {
            try
            {
                // Null checks
                if (invoicesData == null || !invoicesData.Any())
                {
                    Console.WriteLine("Error: Invoices data is null or empty");
                    return false;
                }
                
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    Console.WriteLine("Error: Recipient email is null or empty");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cửa hàng đồng hồ cao cấp", _configuration["EmailSettings:FromEmail"]));
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = $"Báo cáo hóa đơn - {DateTime.Now:dd/MM/yyyy}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateMultipleInvoicesEmailBody(invoicesData);

                // Đính kèm file Excel chứa nhiều hóa đơn
                var attachmentData = _invoiceExportService.ExportMultipleToExcel(invoicesData);
                var fileName = $"BaoCaoHoaDon_{DateTime.Now:yyyyMMdd}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                bodyBuilder.Attachments.Add(fileName, attachmentData, ContentType.Parse(contentType));
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], 
                        int.Parse(_configuration["EmailSettings:SmtpPort"]), 
                        MailKit.Security.SecureSocketOptions.StartTls);
                    
                    await client.AuthenticateAsync(_configuration["EmailSettings:Username"], 
                        _configuration["EmailSettings:Password"]);
                    
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending multiple invoices email: {ex.Message}");
                return false;
            }
        }

        private string GenerateEmailBody(InvoiceViewModel invoiceData)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .invoice-info {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; margin-top: 20px; }}
                        .highlight {{ color: #007bff; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>CỬA HÀNG ĐỒNG HỒ CAO CẤP</h1>
                        <p>Hóa đơn thanh toán</p>
                    </div>
                    
                    <div class='content'>
                        <p>Xin chào quý khách,</p>
                        
                        <p>Cảm ơn quý khách đã mua hàng tại cửa hàng chúng tôi. Dưới đây là thông tin hóa đơn của quý khách:</p>
                        
                        <div class='invoice-info'>
                            <h3>Thông tin hóa đơn</h3>
                            <p><strong>Mã hóa đơn:</strong> <span class='highlight'>{invoiceData.Order.OrderCode}</span></p>
                            <p><strong>Ngày tạo:</strong> {invoiceData.Order.CreatedDate:dd/MM/yyyy HH:mm}</p>
                            <p><strong>Trạng thái:</strong> {(invoiceData.Order.Status == 1 ? "Đã xác nhận" : "Chờ xử lý")}</p>
                            <p><strong>Tổng tiền:</strong> <span class='highlight'>{invoiceData.TotalAmount:N0}₫</span></p>
                        </div>
                        
                        <p>Hóa đơn chi tiết đã được đính kèm trong email này. Quý khách có thể tải về và lưu trữ để làm chứng từ.</p>
                        
                        <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua:</p>
                        <ul>
                            <li>Hotline: 0388672928</li>
                            <li>Email: support@donghocaocap.com</li>
                        </ul>
                        
                        <p>Trân trọng cảm ơn!</p>
                    </div>
                    
                    <div class='footer'>
                        <p><strong>CỬA HÀNG ĐỒNG HỒ CAO CẤP</strong></p>
                        <p>01-05, Tầng 1 Tràng Tiền Plaza, 24 Hai Bà Trưng, Phường Cửa Nam, Quận Hoàn Kiếm, Hà Nội</p>
                        <p>Điện thoại: 0388672928 | Email: support@donghocaocap.com</p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMultipleInvoicesEmailBody(List<InvoiceViewModel> invoicesData)
        {
            var totalAmount = invoicesData.Sum(i => i.TotalAmount);
            var orderCodes = string.Join(", ", invoicesData.Select(i => i.Order.OrderCode));

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .summary {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; margin-top: 20px; }}
                        .highlight {{ color: #007bff; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>CỬA HÀNG ĐỒNG HỒ CAO CẤP</h1>
                        <p>Báo cáo hóa đơn</p>
                    </div>
                    
                    <div class='content'>
                        <p>Xin chào,</p>
                        
                        <p>Dưới đây là báo cáo tổng hợp các hóa đơn:</p>
                        
                        <div class='summary'>
                            <h3>Tổng kết</h3>
                            <p><strong>Số lượng hóa đơn:</strong> <span class='highlight'>{invoicesData.Count}</span></p>
                            <p><strong>Mã hóa đơn:</strong> {orderCodes}</p>
                            <p><strong>Tổng giá trị:</strong> <span class='highlight'>{totalAmount:N0}₫</span></p>
                            <p><strong>Ngày báo cáo:</strong> {DateTime.Now:dd/MM/yyyy}</p>
                        </div>
                        
                        <p>Chi tiết các hóa đơn đã được đính kèm trong file Excel. Quý khách có thể tải về để xem chi tiết.</p>
                        
                        <p>Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua:</p>
                        <ul>
                            <li>Hotline: 0388672928</li>
                            <li>Email: support@donghocaocap.com</li>
                        </ul>
                        
                        <p>Trân trọng cảm ơn!</p>
                    </div>
                    
                    <div class='footer'>
                        <p><strong>CỬA HÀNG ĐỒNG HỒ CAO CẤP</strong></p>
                        <p>01-05, Tầng 1 Tràng Tiền Plaza, 24 Hai Bà Trưng, Phường Cửa Nam, Quận Hoàn Kiếm, Hà Nội</p>
                        <p>Điện thoại: 0388672928 | Email: support@donghocaocap.com</p>
                    </div>
                </body>
                </html>";
        }
    }
}
