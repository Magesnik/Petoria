using System.Collections.Generic;

namespace Petoria.Core.Models.Email
{
    public class ConfirmationEmailContext
    {
        public string UserLanguage { get; set; } = "bg";
        public decimal GrandTotalPaid { get; set; }
        public decimal GrandTotalOriginal { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public decimal? PromoCodeDiscountPercentage { get; set; }
        
        public List<ConfirmationEmailItem> Items { get; set; } = new();
    }
}
