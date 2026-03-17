using System;
using System.IO;
using Petoria.Core.Models.Email;

namespace Petoria.Core.Utilities
{
    /// <summary>
    /// Генерира HTML имейл шаблони за потвърждение на резервация и анулиране.
    /// Поддържа двуезични шаблони (BG/EN) с вграден CSS стайлинг.
    /// </summary>
    public static class EmailTemplateBuilder
    {
        /// <summary>Зарежда CSS стиловете за имейл шаблоните от вграден ресурс.</summary>
        private static string GetEmailCss()
        {
            try
            {
                var assembly = typeof(EmailTemplateBuilder).Assembly;
                // Зареждане на CSS файла от вградените ресурси на асемблито
                using var stream = assembly.GetManifestResourceStream("Petoria.Core.Utilities.EmailStyles.css");
                if (stream == null) return string.Empty;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>Обвива HTML съдържанието с DOCTYPE, head и CSS стилове.</summary>
        private static string WrapHtml(string bodyContent)
        {
            var css = GetEmailCss();
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        {css}
                    </style>
                </head>
                <body>
                    {bodyContent}
                </body>
                </html>";
        }

        /// <summary>Генерира имейл шаблон за потвърждение на резервация с детайли за стаи, цени и отстъпки.</summary>
        public static (string Subject, string Body) BuildConfirmationEmail(ConfirmationEmailContext ctx)
        {
            bool isEn = ctx.UserLanguage?.ToLower() == "en";

            string subject = isEn ? "Successful Reservation on Petoria" : "Успешна резервация през Petoria";
            string title = isEn ? "Successful Reservation! 🎉" : "Успешна резервация! 🎉";
            string greeting = isEn ? "Hello," : "Здравейте,";
            string intro = isEn 
                ? "Thank you for choosing Petoria. Your reservations are successfully confirmed and paid. Here are the details:" 
                : "Благодарим ви, че избрахте Petoria. Вашите резервации са успешно потвърдени и платени. Ето детайлите:";

            string roomLabel = isEn ? "Room Type:" : "Стая:";
            string periodLabel = isEn ? "Period:" : "Период:";
            string nightsLabel = isEn ? "nights" : "нощувки";
            string roomsCountLabel = isEn ? "Number of rooms:" : "Брой стаи:";

            string totalBasePriceLabel = isEn ? "Total base price:" : "Обща базова цена:";
            string totalSavedLabel = isEn ? "Total saved:" : "Общо спестено:";
            string appliedCodeLabel = isEn ? "Applied promo code:" : "Приложен код:";
            string totalPaidLabel = isEn ? "Total amount paid:" : "Общо платена сума:";

            string footerText = isEn 
                ? "We look forward to seeing you! For questions, please contact us." 
                : "Очакваме ви с нетърпение! За въпроси, свържете се с нас.";
            string footerRights = isEn 
                ? $"&copy; {DateTime.UtcNow.Year} Petoria. All rights reserved." 
                : $"&copy; {DateTime.UtcNow.Year} Petoria. Всички права запазени.";

            string currency = isEn ? "BGN" : "лв.";

            string itemsHtml = "";
            foreach (var item in ctx.Items)
            {
                var nights = (int)(item.CheckOutDate.Date - item.CheckInDate.Date).TotalDays;
                itemsHtml += $@"
                    <!-- Details Card -->
                    <div class=""details-card"">
                        <h3 class=""details-title"">{item.HotelName}</h3>
                        
                        <table class=""table-common"">
                            <tr>
                                <td class=""td-label"">{roomLabel}</td>
                                <td class=""td-value"">{item.RoomTypeName}</td>
                            </tr>
                            <tr>
                                <td class=""td-label"">{periodLabel}</td>
                                <td class=""td-value"">{item.CheckInDate:dd.MM.yyyy} - {item.CheckOutDate:dd.MM.yyyy} ({nights} {nightsLabel})</td>
                            </tr>
                            <tr>
                                <td class=""td-label"">{roomsCountLabel}</td>
                                <td class=""td-value"">{item.NumberOfRooms}</td>
                            </tr>
                        </table>
                    </div>";
            }

            string totalsHtml = $@"
                <!-- Total Price -->
                <div class=""total-box"">
                    <table class=""table-common"" style=""margin-top: 0;"">
                        {(ctx.GrandTotalOriginal > ctx.GrandTotalPaid ? $@"
                        <tr>
                            <td class=""td-label"">{totalBasePriceLabel}</td>
                            <td class=""td-label"" style=""text-decoration: line-through;"">{ctx.GrandTotalOriginal} {currency}</td>
                        </tr>
                        <tr>
                            <td class=""td-label"" style=""color: #38a169;"">{totalSavedLabel}</td>
                            <td class=""td-label"" style=""color: #38a169;"">{ctx.GrandTotalOriginal - ctx.GrandTotalPaid} {currency}</td>
                        </tr>
                        " : "")}
                        {(!string.IsNullOrEmpty(ctx.PromoCode) ? $@"
                        <tr>
                            <td class=""td-label"" style=""color: #805ad5;"">{appliedCodeLabel}</td>
                            <td class=""td-value"" style=""color: #805ad5;"">{ctx.PromoCode} (-{ctx.PromoCodeDiscountPercentage}%)</td>
                        </tr>
                        " : "")}
                        <tr>
                            <td class=""td-label"" style=""color: #2b6cb0; font-size: 16px; font-weight: bold; padding-top: 8px;"">{totalPaidLabel}</td>
                            <td class=""td-value"" style=""color: #2b6cb0; font-size: 16px; font-weight: bold; padding-top: 8px;"">{ctx.GrandTotalPaid} {currency}</td>
                        </tr>
                    </table>
                </div>";

            string bodyContent = $@"
                <div class=""email-wrapper"">
                    <div class=""email-card"">
                        
                        <div class=""header-success"">
                            <h1 class=""header-title"">Petoria</h1>
                        </div>
                        
                        <div class=""body-content"">
                            <h2 class=""title"">{title}</h2>
                            <p class=""text"">
                                {greeting} <br><br>
                                {intro}
                            </p>
                            
                            {itemsHtml}
                            {totalsHtml}

                            <p class=""footer-text"">
                                {footerText}
                            </p>
                        </div>
                        
                        <div class=""footer"">
                            <p class=""footer-rights"">
                                {footerRights}
                            </p>
                        </div>
                        
                    </div>
                </div>";

            return (subject, WrapHtml(bodyContent));
        }

        /// <summary>Генерира имейл шаблон за анулиране на резервация с информация за възстановяване на сумата.</summary>
        public static (string Subject, string Body) BuildCancellationEmail(CancellationEmailContext ctx)
        {
            bool isEn = ctx.UserLanguage?.ToLower() == "en";
            string currency = isEn ? "BGN" : "лв.";

            string subject = isEn 
                ? $"Reservation Cancelled at {ctx.HotelName}" 
                : $"Отмяна на резервация в {ctx.HotelName}";
            
            string title = isEn ? "Reservation successfully cancelled" : "Успешно отменена резервация";
            string greeting = isEn ? "Hello," : "Здравейте,";
            string introMessage = isEn 
                ? $"Your reservation at <strong>{ctx.HotelName}</strong> for the period <strong>{ctx.CheckInDate:dd.MM.yyyy} - {ctx.CheckOutDate:dd.MM.yyyy}</strong> has been successfully cancelled." 
                : $"Вашата резервация в <strong>{ctx.HotelName}</strong> за периода <strong>{ctx.CheckInDate:dd.MM.yyyy} - {ctx.CheckOutDate:dd.MM.yyyy}</strong> беше успешно отменена.";
                
            string mistakeWarning = isEn 
                ? "If this was an error or you have any questions, please do not hesitate to contact us as soon as possible." 
                : "Ако това е станало по погрешка или имате въпроси, моля не се колебайте да се свържете с нас възможно най-скоро.";

            string penaltyWarning = "";
            if (ctx.RefundPercentage < 100 && ctx.RefundPercentage > 0)
            {
                penaltyWarning = isEn 
                    ? $@"<div class=""penalty-box"">
                            <p class=""penalty-text"">
                                In accordance with the cancellation policies for this property ({ctx.DaysBeforeCheckIn} days before check-in), you will be refunded <strong>{ctx.RefundPercentage}%</strong> of the total amount.
                            </p>
                         </div>"
                    : $@"<div class=""penalty-box"">
                            <p class=""penalty-text"">
                                Съгласно правилата за отмяна на този хотел ({ctx.DaysBeforeCheckIn} дни преди настаняване), ще ви бъде възстановена <strong>{ctx.RefundPercentage}%</strong> от общата сума.
                            </p>
                         </div>";
            }

            string approvedAmountLabel = isEn ? "Approved refund amount:" : "Одобрена сума за връщане:";
            string bankWaitDesc = ctx.RefundAmount > 0 
                ? (isEn ? "Depending on your bank, the funds will be available in your account within 5-10 business days." : "В зависимост от вашата банка, сумата ще бъде налична по сметката ви в рамките на 5-10 работни дни.")
                : (isEn ? "The cancellation policy within this timeframe does not support refunds for your paid booking." : "Политиката за отмяна в този срок не предвижда възстановяване на заплатената сума.");

            string regards = isEn ? "Regards,<br/>The Petoria Team" : "Поздрави,<br/>Екипът на Petoria";
            string footerRights = isEn ? $"&copy; {DateTime.UtcNow.Year} Petoria. All rights reserved." : $"&copy; {DateTime.UtcNow.Year} Petoria. Всички права запазени.";

            string bodyContent = $@"
                <div class=""email-wrapper"">
                    <div class=""email-card"">
                        
                        <div class=""header-danger"">
                            <h1 class=""header-title"">Petoria</h1>
                        </div>
                        
                        <div class=""body-content"">
                            <h2 class=""title"">{title}</h2>
                            <p class=""text"">
                                {greeting} <br><br>
                                {introMessage}
                            </p>
                            
                            <div class=""warning-box"">
                                <p class=""warning-text"">
                                    {mistakeWarning}
                                </p>
                            </div>
                            
                            {penaltyWarning}
                            
                            <div class=""refund-box"">
                                <p class=""refund-title"">
                                    {approvedAmountLabel} <strong>{ctx.RefundAmount} {currency}</strong>
                                </p>
                                <p class=""refund-desc"">
                                    {bankWaitDesc}
                                </p>
                            </div>
                            
                            <p class=""text"" style=""margin-bottom: 0;"">
                                {regards}
                            </p>
                        </div>
                        
                        <div class=""footer"">
                            <p class=""footer-rights"">
                                {footerRights}
                            </p>
                        </div>
                    </div>
                </div>";

            return (subject, WrapHtml(bodyContent));
        }
    }
}
