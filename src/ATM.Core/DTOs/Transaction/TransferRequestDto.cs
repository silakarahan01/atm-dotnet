namespace ATM.Core.DTOs.Transaction;

public class TransferRequestDto
{
    public string TargetAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
