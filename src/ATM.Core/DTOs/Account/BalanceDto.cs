namespace ATM.Core.DTOs.Account;

public class BalanceDto
{
    public decimal Balance { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
}
