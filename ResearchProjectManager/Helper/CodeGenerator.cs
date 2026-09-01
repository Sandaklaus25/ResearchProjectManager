namespace ResearchProjectManager.Helpers
{
    public static class CodeGenerator
    {
        public static string GenerateSpecialCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }
    }
}