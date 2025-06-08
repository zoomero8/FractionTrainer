using System.Windows;

namespace FractionTrainer
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            // Подписываемся на событие загрузки, чтобы установить правильное состояние кнопки
            Loaded += MainWindow_Loaded;
        }

        // При загрузке окна синхронизируем состояние кнопки с состоянием темы
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeToggleButton.IsChecked = (ThemeManager.CurrentTheme == Theme.Dark);
        }

        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.CurrentTheme == Theme.Dark) return;

            ThemeManager.ApplyTheme(Theme.Dark);

            App.SettingsManager.Settings.CurrentTheme = Theme.Dark; // <-- ИЗМЕНЕНИЕ ЗДЕСЬ
            App.SettingsManager.Save();
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.CurrentTheme == Theme.Light) return;

            ThemeManager.ApplyTheme(Theme.Light);

            App.SettingsManager.Settings.CurrentTheme = Theme.Light; // <-- ИЗМЕНЕНИЕ ЗДЕСЬ
            App.SettingsManager.Save();
        }

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