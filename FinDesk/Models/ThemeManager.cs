using Avalonia;
using Avalonia.Styling;
using FinDesk.Services;

namespace FinDesk.Models
{
    public static class ThemeManager
    {
        private static readonly DatabaseService _dbService = new DatabaseService();

        public static void LoadTheme()
        {
            var theme = _dbService.GetSetting("theme");
            ApplyTheme(theme);
        }

        public static void ToggleTheme()
        {
            var currentTheme = _dbService.GetSetting("theme");
            var newTheme = currentTheme == "light" ? "dark" : "light";

            ApplyTheme(newTheme);
            _dbService.SaveSetting("theme", newTheme);
        }

        private static void ApplyTheme(string theme)
        {
            if (Application.Current == null) return;

            if (theme == "dark")
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            }
        }

        public static string GetCurrentTheme()
        {
            return _dbService.GetSetting("theme");
        }
    }
}