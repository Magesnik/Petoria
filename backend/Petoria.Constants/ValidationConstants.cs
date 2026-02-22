namespace Petoria.Constants;

public static class ValidationConstants
{
    public static class Hotel
    {
        public const int NameMaxLength = 200;
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
    }

    public static class Reservation
    {
        public const int StatusMaxLength = 50;
        public const int NotesMaxLength = 500;
        public const int RoomsMin = 1;
        public const int RoomsMax = 100;
    }

    public static class HotelReview
    {
        public const int CommentMaxLength = 2000;
        public const int RatingMin = 1;
        public const int RatingMax = 5;
    }

    public static class Comment
    {
        public const int ContentMaxLength = 2000;
        public const int ContentMinLength = 1;
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
    }

    public static class User
    {
        public const int FirstNameMaxLength = 100;
        public const int LastNameMaxLength = 100;
        public const int PasswordMinLength = 6;
    }

    public static class Availability
    {
        public const int AvailableCountMin = 0;
        public const int AvailableCountMax = 1000;
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
    }
}
