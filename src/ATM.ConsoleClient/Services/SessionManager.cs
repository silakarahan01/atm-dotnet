namespace ATM.ConsoleClient.Services;

public class SessionManager
{
    public string? Token { get; private set; }
    public string? CardholderName { get; private set; }
    public bool IsLoggedIn => Token != null;

    public void SetSession(string token, string name)
    {
        Token = token;
        CardholderName = name;
    }

    public void Clear()
    {
        Token = null;
        CardholderName = null;
    }
}
