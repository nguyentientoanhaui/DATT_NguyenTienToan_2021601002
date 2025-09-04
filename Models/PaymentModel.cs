using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class PaymentModel
    {
        public int Id { get; set; }
        
        [Required]
        public string OrderId { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } // bank-transfer, credit-card, installment, cash
        
        public decimal Amount { get; set; }
        
        public decimal ProcessingFee { get; set; }
        
        public decimal Discount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public string Status { get; set; } // pending, completed, failed, cancelled
        
        public string TransactionId { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public string CustomerPhone { get; set; }
        
        public string ShippingAddress { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string PaymentGateway { get; set; } // vnpay, momo, manual
        
        public string PaymentUrl { get; set; } // URL để redirect đến gateway
        
        public string ReturnUrl { get; set; } // URL callback
        
        public string IpAddress { get; set; }
        
        public string UserAgent { get; set; }
    }
}


