// Файл: ThemeManager.cs
using System;
using System.Linq;
using System.Windows;

namespace FractionTrainer
{
    public enum Theme { Light, Dark }

    public static class ThemeManager
    {
        public static Theme CurrentTheme { get; private set; } = Theme.Light;

        public static void ApplyTheme(Theme theme)
        {
            // Запоминаем новую тему
            CurrentTheme = theme;

            var oldThemeDictionary = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (oldThemeDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldThemeDictionary);
            }

            string themeUri = (theme == Theme.Light) ? "Themes/Light.xaml" : "Themes/Dark.xaml";

            var newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Insert(0, newThemeDictionary);
        }
    }
}