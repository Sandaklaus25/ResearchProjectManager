using Microsoft.AspNetCore.Http;

namespace ResearchProjectManager.Helper
{
    public static class CookieHelper
    {
        public static void SetActiveCourseCookie(HttpResponse response, IDictionary<object, object?> items, string userId, int courseId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                response.Cookies.Append($"ActiveCourseId_{userId}", courseId.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
                items["ActiveCourseId"] = courseId.ToString();
            }
        }

        public static void ClearActiveCourseCookie(HttpResponse response, IDictionary<object, object?> items, string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                response.Cookies.Delete($"ActiveCourseId_{userId}");
                items.Remove("ActiveCourseId");
            }
        }
    }
}