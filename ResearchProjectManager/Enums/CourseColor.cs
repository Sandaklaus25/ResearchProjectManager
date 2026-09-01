namespace ResearchProjectManager.Enums
{
    public enum CourseColor
    {
        Red = 1,
        Orange = 2,
        Purple = 3,
        Brown = 4,
        Blue = 5,
        Teal = 6,
        Green = 7,
        Slate = 8
    }

    public static class CourseTheme
    {
        public static readonly Dictionary<CourseColor, string> Colors = new()
        {
            { CourseColor.Red, "#D9534F" },
            { CourseColor.Orange, "#F0AD4E" },
            { CourseColor.Purple, "#8E44AD" },
            { CourseColor.Brown, "#6B3E26" },
            { CourseColor.Blue, "#3498DB" },
            { CourseColor.Teal, "#16A085" },
            { CourseColor.Green, "#2ECC71" }, 
            { CourseColor.Slate, "#34495E" }  
        };
    }
}