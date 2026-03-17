namespace Petoria.Core.Models.Auth;

/// <summary>Заявка: Google OAuth токен за логин</summary>
public class GoogleLoginModel
{
    public string GoogleToken { get; set; } = string.Empty;
}
