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

        private void FindPairsModeButton_Click(object sender, RoutedEventArgs e)
        {
            FindPairsWindow findPairsWindow = new FindPairsWindow();
            findPairsWindow.Owner = this;

            findPairsWindow.Closed += (s, args) =>
            {
                this.Show(); // Показываем MainWindow, когда окно "Найдите пары" закрывается
            };

            findPairsWindow.Show();
            this.Hide(); // Скрываем MainWindow
        }

        private void TestModeButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр окна "Проверки знаний"
            KnowledgeCheckWindow knowledgeCheckWindow = new KnowledgeCheckWindow();
            knowledgeCheckWindow.Owner = this; // Устанавливаем текущее окно как родительское

            // Добавляем обработчик, который покажет главное меню,
            // когда окно проверки знаний закроется
            knowledgeCheckWindow.Closed += (s, args) =>
            {
                this.Show();
            };

            knowledgeCheckWindow.Show(); // Показываем окно проверки знаний
            this.Hide(); // Скрываем текущее окно (главное меню)
        }
    }
}