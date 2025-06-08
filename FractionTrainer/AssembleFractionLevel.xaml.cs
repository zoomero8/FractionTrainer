using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FractionTrainer
{
    /// <summary>
    /// UserControl для уровня типа "Соберите дробь".
    /// Реализует ILevelControl для взаимодействия с KnowledgeCheckWindow.
    /// </summary>
    public partial class AssembleFractionLevel : UserControl, ILevelControl
    {
        // --- Событие ---
        public event EventHandler<bool> LevelCompleted;

        // --- Поля класса ---
        private readonly Random random = new Random();
        private int targetNumerator;
        private int targetDenominator;
        private int currentUserDenominator = 1;

        // --- Конструктор ---
        public AssembleFractionLevel()
        {
            InitializeComponent();
            GenerateNewLevel();
        }

        // --- Обработчики событий ---

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonContent = CheckButton.Content.ToString();

            // ИЗМЕНЕНИЕ 1: Теперь при нажатии на "Заново" мы сообщаем о провале
            if (buttonContent == "Заново")
            {
                // Сообщаем родительскому окну, что попытка была неудачной.
                // Родительское окно само снимет жизнь и перезагрузит уровень.
                LevelCompleted?.Invoke(this, false);
                return;
            }

            if (FractionDisplay == null) return;
            int userSelectedNumerator = FractionDisplay.UserSelectedSectorsCount;
            int userSelectedDenominator = FractionDisplay.Denominator;

            if (userSelectedDenominator <= 1 && userSelectedNumerator == 0)
            {
                return;
            }

            double targetValue = (double)targetNumerator / targetDenominator;
            double userValue = (userSelectedDenominator == 0) ? 0 : (double)userSelectedNumerator / userSelectedDenominator;
            bool isCorrect = Math.Abs(targetValue - userValue) < 0.0001;

            if (isCorrect)
            {
                // Логика для правильного ответа остается без изменений
                ShowSuccessFeedback();
                Dispatcher.Invoke(async () => {
                    await System.Threading.Tasks.Task.Delay(1000);
                    LevelCompleted?.Invoke(this, true);
                });
            }
            else
            {
                // ИЗМЕНЕНИЕ 2: При ошибке мы просто показываем фидбек и ЖДЕМ.
                // Мы НЕ вызываем LevelCompleted. Мы ждем, пока пользователь нажмет "Заново".
                ShowErrorFeedback();
            }
        }

        private void DecreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState();
            currentUserDenominator--;
            UpdateShapeDenominator();
        }

        private void IncreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState();
            currentUserDenominator++;
            UpdateShapeDenominator();
        }

        // --- Вспомогательные методы ---

        private void GenerateNewLevel()
        {
            targetDenominator = random.Next(2, 9);
            targetNumerator = random.Next(1, targetDenominator);
            NumeratorTextBlock.Text = targetNumerator.ToString();
            DenominatorTextBlock.Text = targetDenominator.ToString();

            currentUserDenominator = 1;

            if (FractionDisplay != null)
            {
                FractionDisplay.CurrentShapeType = ShapeType.Circle;
                UpdateShapeDenominator();
            }

            ResetButtonAndFeedbackState();
        }

        private void UpdateShapeDenominator()
        {
            if (currentUserDenominator < 1) currentUserDenominator = 1;
            if (currentUserDenominator > 16) currentUserDenominator = 16;
            if (FractionDisplay != null) { FractionDisplay.Denominator = currentUserDenominator; }
        }

        private void ShowSuccessFeedback()
        {
            FeedbackText.Text = "✓";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Отлично!"; // Просто меняем текст, "Продолжить" управляется извне
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            CheckButton.IsEnabled = false; // Блокируем кнопку после ответа
            DecreaseDenominatorButton.IsEnabled = false;
            IncreaseDenominatorButton.IsEnabled = false;
        }

        private void ShowErrorFeedback()
        {
            // --- Получаем цвета из текущей темы с "запасным" вариантом ---

            // Пытаемся найти кисть для фона. Если не нашли, используем светло-розовый.
            var errorBackground = (Brush)Application.Current.TryFindResource("ErrorBackgroundBrush")
                                  ?? new SolidColorBrush(Color.FromRgb(255, 235, 238));

            // Пытаемся найти кисть для текста и кнопки. Если не нашли, используем красный.
            var errorForeground = (Brush)Application.Current.TryFindResource("ErrorBrush")
                                  ?? new SolidColorBrush(Color.FromRgb(220, 53, 69));

            // --- Применяем цвета ---
            FeedbackText.Foreground = errorForeground;
            CheckButton.Background = errorForeground;

            // Остальная логика остается без изменений
            FeedbackText.Text = "✗";
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Заново";
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";
            CheckButton.IsEnabled = true; // Убедимся, что кнопка активна

            // --- ИЗМЕНЕННАЯ И ИСПРАВЛЕННАЯ ЛОГИКА ---
            // Удаляем старый код, который вызывал ошибку.
            // Вместо него напрямую устанавливаем ссылка на ресурсы из нашей темы.
            // Это C#-эквивалент записи: Background="{DynamicResource ButtonAccentBrush}"
            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");
        }
    }
}
