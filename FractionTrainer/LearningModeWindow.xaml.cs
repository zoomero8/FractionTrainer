using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FractionTrainer
{
    public partial class LearningModeWindow : Window
    {
        // --- Поля класса ---
        private readonly Random random = new Random();
        private int targetNumerator;
        private int targetDenominator;
        private int currentUserDenominator = 1;

        // --- Конструктор ---
        public LearningModeWindow()
        {
            InitializeComponent();
            GenerateNewLevel();
        }
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonContent = CheckButton.Content.ToString();

            // --- Логика состояний кнопки ---
            if (buttonContent == "Продолжить")
            {
                GenerateNewLevel(); // Переходим к следующему уровню
                return;
            }
            else if (buttonContent == "Заново")
            {
                FractionDisplay.ResetUserSelectionAndDraw();
                ResetButtonAndFeedbackState();
                return;
            }

            // --- Логика для состояния "Проверить" ---
            if (FractionDisplay == null) return;
            int userSelectedNumerator = FractionDisplay.UserSelectedSectorsCount;
            int userSelectedDenominator = FractionDisplay.Denominator;

            if (userSelectedDenominator <= 1 && userSelectedNumerator == 0)
            {
                CustomMessageBoxWindow.Show("Сначала соберите дробь, используя кнопки '+/- доля' и кликая по секторам.", "Подсказка", this);
                return;
            }

            double targetValue = (double)targetNumerator / targetDenominator;
            double userValue = (userSelectedDenominator == 0) ? 0 : (double)userSelectedNumerator / userSelectedDenominator;

            if (Math.Abs(targetValue - userValue) < 0.0001)
            {
                // Правильный ответ!
                ShowSuccessFeedback();
            }
            else
            {
                // Неправильный ответ!
                ShowErrorFeedback();
            }
        }

        private void DecreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState(); // Сбрасываем обратную связь, если пользователь меняет знаменатель
            currentUserDenominator--;
            UpdateShapeDenominator();
        }

        private void IncreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState(); // Сбрасываем обратную связь, если пользователь меняет знаменатель
            currentUserDenominator++;
            UpdateShapeDenominator();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = this.Owner ?? Application.Current.MainWindow;
            if (ownerWindow != null && ownerWindow != this) { if (!ownerWindow.IsVisible) ownerWindow.Show(); ownerWindow.Focus(); }
            this.Close();
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
            FeedbackText.Text = "✓"; // Только галочка
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Зеленый
            FeedbackText.Visibility = Visibility.Visible;

            CheckButton.Content = "Продолжить";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Насыщенный зеленый
        }

        private void ShowErrorFeedback()
        {
            FeedbackText.Text = "✗"; // Только крестик
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Красный
            FeedbackText.Visibility = Visibility.Visible;

            CheckButton.Content = "Заново";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Красный цвет
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";

            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");

            CheckButton.IsEnabled = true;
            DecreaseDenominatorButton.IsEnabled = true;
            IncreaseDenominatorButton.IsEnabled = true;
        }
    }
}
