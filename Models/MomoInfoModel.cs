using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class MomoInfoModel
    {
        [Key]
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string OrderInfo { get; set; }
        public string FullName { get; set; }
        public string TransactionStatus { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }

        // Navigation Properties
        [ForeignKey("OrderCode")]
        public virtual OrderModel Order { get; set; }
    }
}
