using System.Windows;

namespace FractionTrainer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LearningModeButton_Click(object sender, RoutedEventArgs e)
        {
            LearningModeWindow learningWindow = new LearningModeWindow();
            learningWindow.Owner = this; // Устанавливаем MainWindow как владельца
            learningWindow.Show();
            learningWindow.Closed += (s, args) => {
                if (this.Owner == null || !this.Owner.IsVisible) // Проверяем, если Owner (MainWindow) не был показан кнопкой Назад
                {
                    this.Show();
                }
            };
            this.Hide();
        }

        private void TestModeButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет логика перехода к экрану/режиму проверки знаний
            MessageBox.Show("Переход в режим проверки знаний!"); // Временная заглушка

            // TODO: Создать и показать окно/страницу режима проверки знаний
            // TestModeWindow testWindow = new TestModeWindow();
            // testWindow.Show();
            // this.Close(); // или this.Hide();
        }
    }
}