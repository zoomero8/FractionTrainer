using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FractionTrainer
{

    public partial class MultipleChoiceLevel : UserControl, ILevelControl
    {
        // --- Событие ---
        public event EventHandler<bool> LevelCompleted;

        // --- Поля класса ---
        private readonly Random random = new Random();
        private int targetNumeratorValue;
        private int targetDenominatorValue;
        private double targetFractionValue;
        private List<FractionOption> currentOptions;
        private readonly List<ToggleButton> optionToggleButtons;
        private readonly List<FractionShapeVisualizer> optionShapes;

        public MultipleChoiceLevel()
        {
            InitializeComponent();
            optionToggleButtons = new List<ToggleButton> { OptionButton1, OptionButton2, OptionButton3, OptionButton4 };
            optionShapes = new List<FractionShapeVisualizer> { OptionShape1, OptionShape2, OptionShape3, OptionShape4 };

            // Добавляем обработчик, чтобы сбрасывать состояние кнопки при изменении выбора
            foreach (var btn in optionToggleButtons)
            {
                btn.Click += (s, e) => ResetFeedbackIfAnswered();
            }

            GenerateLevel();
        }


        public void GenerateLevel()
        {
            ResetButtonAndFeedbackState();

            // 1. Генерируем целевую дробь
            targetDenominatorValue = random.Next(2, 9);
            targetNumeratorValue = random.Next(1, targetDenominatorValue);
            targetFractionValue = (double)targetNumeratorValue / targetDenominatorValue;

            TargetNumeratorTextBlock.Text = targetNumeratorValue.ToString();
            TargetDenominatorTextBlock.Text = targetDenominatorValue.ToString();

            currentOptions = new List<FractionOption>();
            List<ShapeType> availableShapes = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>().ToList();

            // 2. Определяем, сколько будет правильных ответов (1 или 2)
            int numberOfCorrectAnswersToGenerate = random.Next(1, 3);

            // 3. Генерируем правильные варианты
            for (int i = 0; i < numberOfCorrectAnswersToGenerate; i++)
            {
                FractionOption correctOption = null;
                int attempts = 0;
                while (correctOption == null && attempts < 20)
                {
                    correctOption = CreateCorrectFractionOption(targetNumeratorValue, targetDenominatorValue, availableShapes);
                    // Проверка на дубликат по визуальному представлению
                    if (correctOption != null && currentOptions.Any(o => o.Shape == correctOption.Shape && o.DisplayedNumerator == correctOption.DisplayedNumerator && o.DisplayedDenominator == correctOption.DisplayedDenominator))
                    {
                        correctOption = null;
                    }
                    attempts++;
                }
                if (correctOption != null)
                {
                    currentOptions.Add(correctOption);
                }
            }

            // 4. Генерируем неправильные варианты (дистракторы) до общего количества 4
            while (currentOptions.Count < 4)
            {
                FractionOption distractorOption = null;
                int attempts = 0;
                while (distractorOption == null && attempts < 20)
                {
                    distractorOption = CreateDistractorFractionOption(targetNumeratorValue, targetDenominatorValue, availableShapes);
                    // Проверка на дубликат и на случайное совпадение с правильным ответом
                    if (distractorOption != null &&
                        (currentOptions.Any(o => o.Shape == distractorOption.Shape && o.DisplayedNumerator == distractorOption.DisplayedNumerator && o.DisplayedDenominator == distractorOption.DisplayedDenominator) ||
                         Math.Abs(distractorOption.Value - targetFractionValue) < 0.0001))
                    {
                        distractorOption = null;
                    }
                    attempts++;
                }

                if (distractorOption != null)
                {
                    currentOptions.Add(distractorOption);
                }
                else
                {
                    var emergencyOption = CreateEmergencyDistractor(availableShapes);
                    if (!currentOptions.Any(o => o.DisplayedNumerator == emergencyOption.DisplayedNumerator && o.DisplayedDenominator == emergencyOption.DisplayedDenominator))
                    {
                        currentOptions.Add(emergencyOption);
                    }
                    else
                    {
                        // Если даже аварийный не прошел, выходим из цикла, чтобы избежать зависания
                        break;
                    }
                }
            }

            // 5. Перемешиваем варианты
            currentOptions = currentOptions.OrderBy(x => random.Next()).ToList();

            // 6. Отображаем варианты на UI
            for (int i = 0; i < optionShapes.Count; i++)
            {
                if (i < currentOptions.Count)
                {
                    var currentOpt = currentOptions[i];
                    optionShapes[i].CurrentShapeType = currentOpt.Shape;
                    optionShapes[i].Denominator = currentOpt.DisplayedDenominator;
                    optionShapes[i].TargetNumerator = currentOpt.DisplayedNumerator;
                    optionShapes[i].ResetUserSelectionAndDraw();

                    optionToggleButtons[i].IsChecked = false;
                    optionToggleButtons[i].Visibility = Visibility.Visible;
                    // Сохраняем сам объект FractionOption в Tag для легкого доступа при проверке
                    optionToggleButtons[i].Tag = currentOpt;
                }
                else
                {
                    optionToggleButtons[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        // --- Обработчики событий ---

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonContent = CheckButton.Content.ToString();

            if (buttonContent == "Заново")
            {
                LevelCompleted?.Invoke(this, false); // Сообщаем о неудаче
                return;
            }

            // Логика проверки
            var selectedOptions = optionToggleButtons
                .Where(btn => btn.IsChecked == true && btn.Visibility == Visibility.Visible)
                .Select(btn => btn.Tag as FractionOption)
                .ToList();

            // Если ничего не выбрано, это ошибка
            if (!selectedOptions.Any())
            {
                ShowErrorFeedback();
                return;
            }

            int totalCorrectOptionsInLevel = currentOptions.Count(opt => opt.IsCorrect);
            int userMadeCorrectSelections = selectedOptions.Count(opt => opt.IsCorrect);
            int userMadeIncorrectSelections = selectedOptions.Count - userMadeCorrectSelections;

            bool isCorrect = userMadeCorrectSelections == totalCorrectOptionsInLevel && userMadeIncorrectSelections == 0;

            if (isCorrect)
            {
                ShowSuccessFeedback();
                await Task.Delay(1200); // Даем пользователю время увидеть результат
                LevelCompleted?.Invoke(this, true); // Сообщаем об успехе
            }
            else
            {
                ShowErrorFeedback();
            }
        }

        private void ResetFeedbackIfAnswered()
        {
            string buttonContent = CheckButton.Content.ToString();
            if (buttonContent == "Заново")
            {
                // Сбрасываем выбор пользователя, но не генерируем новый уровень
                foreach (var btn in optionToggleButtons)
                {
                    btn.IsChecked = false;
                }
                ResetButtonAndFeedbackState(); // Возвращаем кнопку и панель в исходное состояние
            }
        }

        // --- Методы генерации вариантов (перенесены из Window) ---

        private FractionOption CreateCorrectFractionOption(int targetNum, int targetDen, List<ShapeType> availableShapes)
        {
            ShapeType shape = availableShapes[random.Next(availableShapes.Count)];
            int optionNum, optionDen;

            switch (shape)
            {
                case ShapeType.Circle:
                    int k = random.Next(1, 4);
                    optionNum = targetNum * k;
                    optionDen = targetDen * k;
                    if (optionDen > 16) { k = 1; optionNum = targetNum * k; optionDen = targetDen * k; }
                    break;
                case ShapeType.Triangle:
                    if ((targetNum * 3) % targetDen == 0) { optionNum = (targetNum * 3) / targetDen; optionDen = 3; if (optionNum < 1 || optionNum > optionDen) return null; } else return null;
                    break;
                case ShapeType.Diamond:
                    if ((targetNum * 4) % targetDen == 0) { optionNum = (targetNum * 4) / targetDen; optionDen = 4; if (optionNum < 1 || optionNum > optionDen) return null; } else return null;
                    break;
                case ShapeType.Octagon:
                    if ((targetNum * 8) % targetDen == 0) { optionNum = (targetNum * 8) / targetDen; optionDen = 8; if (optionNum < 1 || optionNum > optionDen) return null; } else return null;
                    break;
                default: return null;
            }

            if (optionNum == 0) return null; // Не должно быть правильных ответов с нулевым числителем

            return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = true };
        }

        private FractionOption CreateDistractorFractionOption(int targetNum, int targetDen, List<ShapeType> availableShapes)
        {
            ShapeType shape = availableShapes[random.Next(availableShapes.Count)];
            int optionNum, optionDen;
            double targetValue = (double)targetNum / targetDen;

            switch (shape)
            {
                case ShapeType.Circle: optionDen = random.Next(2, 17); break;
                case ShapeType.Triangle: optionDen = 3; break;
                case ShapeType.Diamond: optionDen = 4; break;
                case ShapeType.Octagon: optionDen = 8; break;
                default: return null;
            }

            int attempts = 0;
            do
            {
                optionNum = random.Next(1, optionDen);
                attempts++;
                if (attempts > 20) return null; // Не удалось подобрать, пробуем другую фигуру
            } while (Math.Abs((double)optionNum / optionDen - targetValue) < 0.0001);

            return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = false };
        }

        private FractionOption CreateEmergencyDistractor(List<ShapeType> availableShapes)
        {
            // Создает гарантированно неверный ответ, если другие методы не сработали
            int den = random.Next(9, 17); // Берем большой знаменатель
            int num = random.Next(1, den);
            return new FractionOption
            {
                Shape = availableShapes[random.Next(availableShapes.Count)],
                DisplayedDenominator = den,
                DisplayedNumerator = num,
                IsCorrect = false
            };
        }



        private void ShowSuccessFeedback()
        {
            FeedbackText.Text = "✓";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Отлично!";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            CheckButton.IsEnabled = false; // Блокируем кнопку после правильного ответа
        }

        private void ShowErrorFeedback()
        {
            // --- Получаем цвета из текущей темы ---
            var errorBackground = (Brush)Application.Current.TryFindResource("ErrorBackgroundBrush");
            var errorForeground = (Brush)Application.Current.TryFindResource("ErrorBrush");

            // --- Применяем цвета из темы ---
            FeedbackText.Foreground = errorForeground;
            CheckButton.Background = errorForeground; // Кнопка тоже становится красной

            // Остальная логика остается без изменений
            FeedbackText.Text = "✗";
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Заново";
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";
            CheckButton.IsEnabled = true;

            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");
        }
    }
}