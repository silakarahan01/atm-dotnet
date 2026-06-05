namespace ATM.API.Contracts;

/// <summary>Para yatırma/çekme istek gövdesi. Hesap kimliği JWT'den alınır.</summary>
public sealed record AmountRequest(decimal Amount);

public sealed record TransferRequest(string TargetAccountNumber, decimal Amount);

public sealed record ChangePinRequest(string CurrentPin, string NewPin);
