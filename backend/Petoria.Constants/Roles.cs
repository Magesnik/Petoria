namespace Petoria.Constants;

/// <summary>
/// Дефинира потребителските роли в системата.
/// Използва се за контрол на достъпа до различни функционалности.
/// </summary>
public static class Roles
{
    /// <summary>Обикновен потребител — може да резервира и пише ревюта</summary>
    public const string User = "User";

    /// <summary>Администратор — може да създава и управлява хотели</summary>
    public const string Admin = "Admin";

    /// <summary>Супер администратор — пълен достъп: управление на потребители, хотели, поддръжка</summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>Модератор на хотел — може да управлява конкретен хотел (стаи, наличност, съобщения)</summary>
    public const string HotelModerator = "HotelModerator";
}
