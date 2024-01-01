namespace Tippser.Core.Enums
{
    public static class Endpoint
    {
        public static string Convert(ApiEndpoint endpoint)
        {
            return $"{endpoint.ToString().Replace("_", "/")}";
        }

        public enum ApiEndpoint
        {
            None,
            SetCulture,
            Common_SetCulture,
            Account_Create,
            Account_SignOut,
            Account_SignIn,
            Account_ForgotPassword,
            Bets_Get,
            Competitions_GetStandings

        }
    }
}
