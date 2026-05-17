namespace ATM.Core.DTOs.Auth;

public class LoginRequestDto
{
    public string CardNumber { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}
