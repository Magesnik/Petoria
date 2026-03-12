namespace Petoria.Constants;

public static class ValidationConstants
{
    public static class Hotel
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 2000;
        public const int LocationMaxLength = 300;
        public const int CityMaxLength = 100;
        public const int CountryMaxLength = 100;
        public const int ImageUrlMaxLength = 500;
        public const int ImagesMaxLength = 2000;
        public const int AmenitiesMaxLength = 1000;
        public const int RoomTypesMaxLength = 1000;
        public const int CancellationPoliciesMaxLength = 2000;
        public const int RatingMin = 0;
        public const int RatingMax = 5;
        public const int StarRatingMin = 1;
        public const int StarRatingMax = 5;

        public const string NameRequired = "Името на хотела е задължително";
        public const string NameMaxLengthError = "Името не може да надвишава {1} символа";
        public const string DescriptionMaxLengthError = "Описанието не може да надвишава {1} символа";
        public const string LocationRequired = "Локацията е задължителна";
        public const string LocationMaxLengthError = "Локацията не може да надвишава {1} символа";
        public const string CityMaxLengthError = "Градът не може да надвишава {1} символа";
        public const string CountryMaxLengthError = "Държавата не може да надвишава {1} символа";
        public const string StarRatingRangeError = "Звездният рейтинг трябва да бъде между {1} и {2}";
    }

    public static class RoomType
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int ImageUrlMaxLength = 500;
        public const double PricePerNightMin = 0.01;
        public const double PricePerNightMax = 100000;
        public const int CapacityMin = 1;
        public const int CapacityMax = 20;
        public const int TotalRoomsMin = 1;
        public const int TotalRoomsMax = 1000;

        public const string NameRequired = "Името на типа стая е задължително";
        public const string NameMaxLengthError = "Името не може да надвишава {1} символа";
        public const string DescriptionMaxLengthError = "Описанието не може да надвишава {1} символа";
        public const string PricePerNightRequired = "Цената на нощувка е задължителна";
        public const string PricePerNightRangeError = "Цената трябва да бъде между {1} и {2}";
        public const string CapacityRequired = "Капацитетът е задължителен";
        public const string CapacityRangeError = "Капацитетът трябва да бъде между {1} и {2}";
        public const string TotalRoomsRequired = "Броят стаи е задължителен";
        public const string TotalRoomsRangeError = "Броят стаи трябва да бъде между {1} и {2}";
    }

    public static class Reservation
    {
        public const int StatusMaxLength = 50;
        public const int NotesMaxLength = 500;
        public const int RoomsMin = 1;
        public const int RoomsMax = 100;

        public const string HotelRequired = "HotelId е задължителен";
        public const string RoomTypeRequired = "RoomTypeId е задължителен";
        public const string CheckInDateRequired = "Датата на настаняване е задължителна";
        public const string CheckOutDateRequired = "Датата на напускане е задължителна";
        public const string RoomsRangeError = "Броят стаи трябва да бъде между {1} и {2}";
        public const string NotesMaxLengthError = "Бележките не могат да надвишават {1} символа";
    }

    public static class HotelReview
    {
        public const int CommentMaxLength = 500;
        public const int RatingMin = 1;
        public const int RatingMax = 5;

        public const string RatingRequired = "Рейтингът е задължителен";
        public const string RatingRangeError = "Рейтингът трябва да бъде между {1} и {2}";
        public const string CommentMaxLengthError = "Ревюто не може да надвишава {1} символа";
    }

    public static class Comment
    {
        public const int ContentMaxLength = 500;
        public const int ContentMinLength = 1;

        public const string ContentRequired = "Текстът на коментара е задължителен";
        public const string ContentMaxLengthError = "Коментарът не може да надвишава {1} символа";
        public const string ContentMinLengthError = "Коментарът не може да бъде празен";
    }

    public static class HotelMessage
    {
        public const int SubjectMaxLength = 200;
        public const int MessageMaxLength = 2000;
        public const int MessageMinLength = 10;
        public const int AnswerMaxLength = 2000;
        public const int AnswerMinLength = 10;
    }

    public static class SupportMessage
    {
        public const int SubjectMaxLength = 200;
        public const int MessageMinLength = 10;

        public const string SubjectRequired = "Темата е задължителна";
        public const string SubjectMaxLengthError = "Темата не може да надвишава {1} символа";
        public const string MessageRequired = "Съобщението е задължително";
        public const string MessageMinLengthError = "Съобщението трябва да бъде поне {1} символа";
    }

    public static class User
    {
        public const int FirstNameMinLength = 2;
        public const int FirstNameMaxLength = 100;
        public const int LastNameMinLength = 4;
        public const int LastNameMaxLength = 100;
        public const int PasswordMinLength = 6;
        public const string NameRegex = @"^[a-zA-Zа-яА-Я]+$";
        public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";

        public const string EmailRequired = "Имейлът е задължителен";
        public const string EmailInvalid = "Невалиден имейл адрес";
        public const string PasswordRequired = "Паролата е задължителна";
        public const string PasswordMinLengthError = "Паролата трябва да бъде поне {1} символа";
        public const string PasswordRegexError = "Паролата трябва да съдържа поне една главна буква, една малка буква, една цифра и един специален символ";
        public const string FirstNameRequired = "Името е задължително";
        public const string FirstNameMinLengthError = "Името трябва да бъде поне {1} символа";
        public const string FirstNameMaxLengthError = "Името не може да надвишава {1} символа";
        public const string FirstNameRegexError = "Името може да съдържа само букви (без интервали, тирета или специални символи)";
        public const string LastNameRequired = "Фамилията е задължителна";
        public const string LastNameMinLengthError = "Фамилията трябва да бъде поне {1} символа";
        public const string LastNameMaxLengthError = "Фамилията не може да надвишава {1} символа";
        public const string LastNameRegexError = "Фамилията може да съдържа само букви (без интервали, тирета или специални символи)";
    }

    public static class Availability
    {
        public const int AvailableCountMin = 0;
        public const int AvailableCountMax = 1000;

        public const string RoomTypeRequired = "RoomTypeId е задължителен";
        public const string StartDateRequired = "Началната дата е задължителна";
        public const string EndDateRequired = "Крайната дата е задължителна";
        public const string AvailableCountRequired = "Броят свободни стаи е задължителен";
        public const string AvailableCountRangeError = "Броят стаи трябва да бъде между {1} и {2}";
    }

    public static class PromoCode
    {
        public const int DiscountPercentageMin = 0;
        public const int DiscountPercentageMax = 100;
        public const int MaxUsesMin = 1;
        public const int ValidDaysMin = 1;
        public const int ValidDaysMax = 3650;
    }

    public static class Discount
    {
        public const int PercentageMin = 1;
        public const int PercentageMax = 99;

        public const string PercentageRequired = "Процентът е задължителен";
        public const string PercentageRangeError = "Процентът трябва да бъде между {1} и {2}";
        public const string StartDateRequired = "Началната дата е задължителна";
        public const string EndDateRequired = "Крайната дата е задължителна";
    }
}
