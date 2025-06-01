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
            learningWindow.Owner = this;

            learningWindow.Closed += (s, args) =>
            {
                this.Show();
            };

            learningWindow.Show();
            this.Hide();
        }

        // НОВЫЙ ОБРАБОТЧИК
        private void MultipleChoiceModeButton_Click(object sender, RoutedEventArgs e)
        {
            MultipleChoiceFractionWindow multipleChoiceWindow = new MultipleChoiceFractionWindow();
            multipleChoiceWindow.Owner = this;

            multipleChoiceWindow.Closed += (s, args) =>
            {
                this.Show(); // Показываем MainWindow, когда окно "Выберите варианты" закрывается
            };

            multipleChoiceWindow.Show();
            this.Hide(); // Скрываем MainWindow
        }

        private void TestModeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Режим проверки знаний еще не реализован.", "Информация");
            // TODO: Реализовать переход к окну/режиму проверки знаний
        }
    }
}