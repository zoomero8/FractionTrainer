using System.Windows;

namespace FractionTrainer
{
    public partial class App : Application
    {
        public static SettingsService SettingsManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Теперь компилятор знает, что такое SettingsService
            SettingsManager = new SettingsService();

            // И он может получить доступ к свойству CurrentTheme
            Theme savedTheme = SettingsManager.Settings.CurrentTheme;
            ThemeManager.ApplyTheme(savedTheme);
        }
    }
}