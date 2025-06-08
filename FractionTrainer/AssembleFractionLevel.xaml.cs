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

            // Если кнопка в режиме "Заново", сбрасываем попытку.
            // "Продолжить" здесь не используется, т.к. переход на след. уровень управляется извне.
            if (buttonContent == "Заново")
            {
                FractionDisplay.ResetUserSelectionAndDraw();
                ResetButtonAndFeedbackState();
                return;
            }

            if (FractionDisplay == null) return;
            int userSelectedNumerator = FractionDisplay.UserSelectedSectorsCount;
            int userSelectedDenominator = FractionDisplay.Denominator;

            if (userSelectedDenominator <= 1 && userSelectedNumerator == 0)
            {
                // В режиме проверки не используем кастомные окна, можно просто ничего не делать или встряхнуть кнопку
                return;
            }

            double targetValue = (double)targetNumerator / targetDenominator;
            double userValue = (userSelectedDenominator == 0) ? 0 : (double)userSelectedNumerator / userSelectedDenominator;
            bool isCorrect = Math.Abs(targetValue - userValue) < 0.0001;

            if (isCorrect)
            {
                ShowSuccessFeedback(); // Показываем зеленую панель
                // Вызываем событие, сообщая, что уровень пройден ПРАВИЛЬНО
                // Задержка, чтобы пользователь увидел результат перед переходом.
                Dispatcher.Invoke(async () => {
                    await System.Threading.Tasks.Task.Delay(1000); // 1 секунда
                    LevelCompleted?.Invoke(this, true);
                });
            }
            else
            {
                ShowErrorFeedback(); // Показываем красную панель
                // Вызываем событие, сообщая, что уровень пройден НЕПРАВИЛЬНО
                LevelCompleted?.Invoke(this, false);
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
            FeedbackBackground.Background = new SolidColorBrush(Color.FromRgb(224, 251, 226));
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
            FeedbackBackground.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
            FeedbackText.Text = "✗";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Ошибка";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            CheckButton.IsEnabled = false; // Блокируем кнопку после ответа
            DecreaseDenominatorButton.IsEnabled = false;
            IncreaseDenominatorButton.IsEnabled = false;
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackBackground.Background = Brushes.Transparent;
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";

            if (Application.Current.TryFindResource("ModernButton") is Style modernButtonStyle)
            {
                var backgroundSetter = modernButtonStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Button.BackgroundProperty);
                if (backgroundSetter != null) CheckButton.Background = (Brush)backgroundSetter.Value;
            }
            else
            {
                CheckButton.Background = new SolidColorBrush(Color.FromRgb(0, 122, 255));
            }

            CheckButton.IsEnabled = true;
            DecreaseDenominatorButton.IsEnabled = true;
            IncreaseDenominatorButton.IsEnabled = true;
        }
    }
}
