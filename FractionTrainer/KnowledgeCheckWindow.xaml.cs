using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FractionTrainer
{
    public partial class KnowledgeCheckWindow : Window
    {
        private readonly GameStateManager _gameStateManager;

        public KnowledgeCheckWindow()
        {
            InitializeComponent();
            _gameStateManager = new GameStateManager();
            StartNewGame();
        }

        private void StartNewGame()
        {
            _gameStateManager.StartNewGame();
            LoadCurrentLevel();
        }

        private void LoadCurrentLevel()
        {
            UpdateTopPanel();

            if (_gameStateManager.IsGameOver)
            {
                ShowGameOver();
                return;
            }

            if (_gameStateManager.IsGameWon)
            {
                ShowGameWon();
                return;
            }

            TestLevelType levelType = _gameStateManager.GetCurrentLevelType();
            UserControl currentLevelControl;

            // Создаем экземпляры наших UserControl'ов
            switch (levelType)
            {
                case TestLevelType.AssembleFraction:
                    currentLevelControl = new AssembleFractionLevel();
                    break;
                case TestLevelType.MultipleChoice:
                    currentLevelControl = new MultipleChoiceLevel();
                    break;
                case TestLevelType.FindPairs:
                    currentLevelControl = new FindPairsLevel();
                    break;
                default:
                    currentLevelControl = new AssembleFractionLevel();
                    break;
            }

            // Подписываемся на событие завершения уровня
            if (currentLevelControl is ILevelControl level)
            {
                level.LevelCompleted += OnLevelCompleted;
            }

            // Отображаем UserControl в окне
            LevelContentPresenter.Content = currentLevelControl;
        }

        private void OnLevelCompleted(object sender, bool isCorrect)
        {
            // Отписываемся от события, чтобы избежать повторных вызовов
            if (sender is ILevelControl level)
            {
                level.LevelCompleted -= OnLevelCompleted;
            }

            if (isCorrect)
            {
                _gameStateManager.CorrectAnswer();
                // Загружаем следующий уровень после правильного ответа
                LoadCurrentLevel();
            }
            else
            {
                _gameStateManager.IncorrectAnswer();

                // Обновляем жизни
                UpdateTopPanel();
                if (_gameStateManager.IsGameOver)
                {
                    ShowGameOver();
                }
                else
                {
                    // Если игра не окончена, пользователь остается на том же уровне.
                    // Просто перезагружаем текущий уровень.
                    LoadCurrentLevel();
                }
            }
        }
        private void UpdateTopPanel()
        {
            LevelProgressTextBlock.Text = $"Уровень {_gameStateManager.CurrentLevel}/{15}";

            var heartBrush = (Brush)Application.Current.TryFindResource("HeartBrush");
            if (heartBrush == null)
            {
                // Запасной вариант, если ресурс не найден
                heartBrush = Brushes.Red;
            }

            LivesPanel.Children.Clear();
            for (int i = 0; i < _gameStateManager.Lives; i++)
            {
                var heartIcon = new TextBlock
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Text = "\uEB52", // Используем ЗАЛИТОЕ сердце
                    FontSize = 20,
                    Foreground = heartBrush, // Применяем кисть из темы
                    Margin = new Thickness(3, 0, 3, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                LivesPanel.Children.Add(heartIcon);
            }
        }

        private void ShowGameOver()
        {
            CustomMessageBoxWindow.Show("Жизни закончились! Начнем сначала?", "Игра окончена", this);
            StartNewGame(); // Перезапускаем игру
        }

        private void ShowGameWon()
        {
            CustomMessageBoxWindow.Show("Поздравляем! Вы прошли все уровни!", "Победа!", this);
            this.Close(); // Закрываем окно после победы
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = this.Owner ?? Application.Current.MainWindow;
            if (ownerWindow != null && ownerWindow != this)
            {
                if (!ownerWindow.IsVisible) ownerWindow.Show();
                ownerWindow.Focus();
            }
            this.Close();
        }
    }
}
