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

        // --- Обработчики событий ---

        /// <summary>
        /// Обрабатывает клик по основной кнопке, которая может быть в режиме "Проверить", "Продолжить" или "Заново".
        /// </summary>
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
                // Сбрасываем текущую попытку, не меняя задание
                FractionDisplay.ResetUserSelectionAndDraw(); // Очищаем выбранные сектора
                ResetButtonAndFeedbackState(); // Возвращаем кнопку и панель в исходное состояние
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

        /// <summary>
        /// Обрабатывает клик по кнопке "- доля", уменьшая количество секторов.
        /// </summary>
        private void DecreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState(); // Сбрасываем обратную связь, если пользователь меняет знаменатель
            currentUserDenominator--;
            UpdateShapeDenominator();
        }

        /// <summary>
        /// Обрабатывает клик по кнопке "+ доля", увеличивая количество секторов.
        /// </summary>
        private void IncreaseDenominatorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonAndFeedbackState(); // Сбрасываем обратную связь, если пользователь меняет знаменатель
            currentUserDenominator++;
            UpdateShapeDenominator();
        }

        /// <summary>
        /// Обрабатывает клик по кнопке "Назад" (стрелочке).
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = this.Owner ?? Application.Current.MainWindow;
            if (ownerWindow != null && ownerWindow != this) { if (!ownerWindow.IsVisible) ownerWindow.Show(); ownerWindow.Focus(); }
            this.Close();
        }

        // --- Вспомогательные методы ---

        /// <summary>
        /// Генерирует новый уровень: новую целевую дробь и сбрасывает фигуру.
        /// </summary>
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

        /// <summary>
        /// Обновляет знаменатель у фигуры и инициирует ее перерисовку.
        /// </summary>
        private void UpdateShapeDenominator()
        {
            if (currentUserDenominator < 1) currentUserDenominator = 1;
            if (currentUserDenominator > 16) currentUserDenominator = 16;
            if (FractionDisplay != null) { FractionDisplay.Denominator = currentUserDenominator; }
        }

        /// <summary>
        /// Показывает зеленую панель успеха и переключает кнопку в режим "Продолжить".
        /// </summary>
        private void ShowSuccessFeedback()
        {
            FeedbackText.Text = "✓"; // Только галочка
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Зеленый
            FeedbackText.Visibility = Visibility.Visible;

            CheckButton.Content = "Продолжить";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Насыщенный зеленый
        }

        /// <summary>
        /// Показывает красную панель ошибки и переключает кнопку в режим "Заново".
        /// </summary>
        private void ShowErrorFeedback()
        {
            FeedbackText.Text = "✗"; // Только крестик
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Красный
            FeedbackText.Visibility = Visibility.Visible;

            CheckButton.Content = "Заново";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Красный цвет
        }

        /// <summary>
        /// Сбрасывает панель и кнопку в исходное состояние "Проверить".
        /// </summary>
        private void ResetButtonAndFeedbackState()
        {
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";

            // --- ИЗМЕНЕНИЯ ЗДЕСЬ ---
            // Удаляем старую логику, которая вызывала ошибку.
            // Вместо нее используем SetResourceReference для установки стиля из темы.
            // Это C#-эквивалент записи: Background="{DynamicResource ButtonAccentBrush}"
            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");

            // Остальные строки оставляем как есть
            CheckButton.IsEnabled = true;
            DecreaseDenominatorButton.IsEnabled = true;
            IncreaseDenominatorButton.IsEnabled = true;
        }
    }
}
