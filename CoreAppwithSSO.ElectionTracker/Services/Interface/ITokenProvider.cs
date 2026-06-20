namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface ITokenProvider
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
    }
}
