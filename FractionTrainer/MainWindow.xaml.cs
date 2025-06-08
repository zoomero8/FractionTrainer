using System.Windows;

namespace FractionTrainer
{
    public partial class MainWindow : Window
    {
        // Поле _currentTheme больше не нужно, его роль выполняет IsChecked у ToggleButton

        public MainWindow()
        {
            InitializeComponent();
            // Подписываемся на событие загрузки, чтобы установить правильное состояние кнопки
            Loaded += MainWindow_Loaded;
        }

        // При загрузке окна синхронизируем состояние кнопки с состоянием темы
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем переключатель в положение "включен", если текущая тема - темная
            ThemeToggleButton.IsChecked = (ThemeManager.CurrentTheme == Theme.Dark);
        }

        // --- НОВАЯ, УПРОЩЕННАЯ ЛОГИКА СМЕНЫ ТЕМЫ ---

        // Срабатывает, когда переключатель ВКЛЮЧАЕТСЯ (переход на темную тему)
        // Срабатывает, когда переключатель ВКЛЮЧАЕТСЯ (переход на темную тему)
        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.CurrentTheme == Theme.Dark) return;

            ThemeManager.ApplyTheme(Theme.Dark);

            // БЫЛО НЕПРАВИЛЬНО: App.CurrentTheme = Theme.Dark;
            // ИСПРАВЛЕННЫЙ ВАРИАНТ:
            App.SettingsManager.Settings.CurrentTheme = Theme.Dark; // <-- ИЗМЕНЕНИЕ ЗДЕСЬ
            App.SettingsManager.Save();
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.CurrentTheme == Theme.Light) return;

            ThemeManager.ApplyTheme(Theme.Light);

            // БЫЛО НЕПРАВИЛЬНО: App.CurrentTheme = Theme.Light;
            // ИСПРАВЛЕННЫЙ ВАРИАНТ:
            App.SettingsManager.Settings.CurrentTheme = Theme.Light; // <-- ИЗМЕНЕНИЕ ЗДЕСЬ
            App.SettingsManager.Save();
        }

        // --- Методы открытия окон остаются без изменений ---
        private void OpenWindow<T>(T window) where T : Window
        {
            window.Owner = this;
            window.Closed += (s, args) => this.Show();
            this.Hide();
            window.Show();
        }

        private void LearningModeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new LearningModeWindow());
        }

        private void MultipleChoiceModeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new MultipleChoiceFractionWindow());
        }

        private void FindPairsModeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new FindPairsWindow());
        }

        private void TestModeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new KnowledgeCheckWindow());
        }
    }
}