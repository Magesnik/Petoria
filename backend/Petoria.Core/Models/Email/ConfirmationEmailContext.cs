using System.Collections.Generic;

namespace Petoria.Core.Models.Email
{
    /// <summary>Контекст за имейл шаблон при потвърждение на резервация</summary>
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
