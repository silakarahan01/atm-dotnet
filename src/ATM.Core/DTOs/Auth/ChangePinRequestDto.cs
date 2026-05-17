namespace ATM.Core.DTOs.Auth;

public class ChangePinRequestDto
{
    public string CurrentPin { get; set; } = string.Empty;
    public string NewPin { get; set; } = string.Empty;
}
