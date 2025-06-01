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
                // Попытка показать Owner, если он существует и не является текущим окном,
                // и если он не видим.
                if (this.Owner != null && this.Owner != Application.Current.MainWindow && !this.Owner.IsVisible)
                {
                    this.Owner.Show();
                }
                else if (Application.Current.MainWindow != null && Application.Current.MainWindow != learningWindow && !Application.Current.MainWindow.IsVisible)
                {
                    // Если Owner не был установлен или это главное окно, пытаемся показать Application.Current.MainWindow
                    Application.Current.MainWindow.Show();
                }
                // Если MainWindow было скрыто (this.Hide()), то оно this
                else if (this.IsVisible == false)
                {
                    this.Show();
                }
            };

            learningWindow.Show();
            this.Hide();
        }

        private void TestModeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Режим проверки знаний еще не реализован.", "Информация");
        }
    }
}