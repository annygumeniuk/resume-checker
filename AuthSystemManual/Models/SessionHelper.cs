namespace ResumeCheckSystem.Models
{
    public class SessionHelper
    {
        private const string UserIdKey = "UserIdValue";
        private const string UserEmailKey = "UserEmail";

        public static bool IsSessionActive(HttpContext httpContext)
        {
            return httpContext.Session.Keys.Contains(UserIdKey);
        }

        public static int? GetUserId(HttpContext httpContext) // RETURNS WRONG ID
        {
            if (httpContext.Session.Keys.Contains(UserIdKey) && int.TryParse(httpContext.Session.GetString(UserIdKey), out int userId))
            {
                return userId;
            }
            return null;
        }

        public static string GetUserEmail(HttpContext httpContext)
        {
            return httpContext.Session.GetString(UserEmailKey);
        }

        public static void SetUserSession(HttpContext httpContext, int userId, string userEmail)
        {
            httpContext.Session.SetString(UserIdKey, userId.ToString());
            httpContext.Session.SetString(UserEmailKey, userEmail);
        }

        public static void ClearUserSession(HttpContext httpContext)
        {
            httpContext.Session.Remove(UserIdKey);
            httpContext.Session.Remove(UserEmailKey);
        }
    }
}
