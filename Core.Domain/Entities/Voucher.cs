using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public string PromotionId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; } 
        public decimal MinOrderValue { get; set; } // Giá trị đơn hàng tối thiểu để được áp dụng
        public int UsageLimit { get; set; } 
        public int UsedCount { get; set; } = 0; 
        public bool IsActive { get; set; } = true;
    }
}
