using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // Для ToggleButton
using System.Windows.Media;

namespace FractionTrainer
{
    public class FractionOption
    {
        public ShapeType Shape { get; set; }
        public int DisplayedNumerator { get; set; }   // Сколько секторов закрашено на фигуре варианта
        public int DisplayedDenominator { get; set; } // На сколько секторов разделена фигура варианта
        public bool IsCorrect { get; set; }           // Является ли этот вариант правильным ответом на целевую дробь

        // Реальное значение дроби этого варианта (для сравнения)
        public double Value => (DisplayedDenominator == 0) ? double.NaN : (double)DisplayedNumerator / DisplayedDenominator;
    }

    public partial class MultipleChoiceFractionWindow : Window
    {
        private Random random = new Random();

        // Целевая дробь уровня
        private int targetNumeratorValue;
        private int targetDenominatorValue;
        private double targetFractionValue;

        // Список текущих вариантов ответа
        private List<FractionOption> currentOptions;
        private List<ToggleButton> optionToggleButtons;
        private List<FractionShapeVisualizer> optionShapes;


        public MultipleChoiceFractionWindow()
        {
            InitializeComponent();

            // Собираем кнопки и визуализаторы в списки для удобного доступа
            optionToggleButtons = new List<ToggleButton> { OptionButton1, OptionButton2, OptionButton3, OptionButton4 };
            optionShapes = new List<FractionShapeVisualizer> { OptionShape1, OptionShape2, OptionShape3, OptionShape4 };

            // Связываем Tag кнопки с ее индексом для легкой идентификации (если понадобится)
            for (int i = 0; i < optionToggleButtons.Count; i++)
            {
                optionToggleButtons[i].Tag = i;
            }

            GenerateMultipleChoiceLevel();
        }

        private void GenerateMultipleChoiceLevel()
        {
            System.Diagnostics.Debug.WriteLine("--- GenerateMultipleChoiceLevel: Начало ---");

            // 1. Генерируем целевую дробь (избегаем N/N)
            // Позволим знаменателю быть немного больше для разнообразия целевых дробей
            targetDenominatorValue = random.Next(2, 9); // Целевой знаменатель от 2 до 8
            targetNumeratorValue = random.Next(1, targetDenominatorValue); // Целевой числитель < знаменателя
            targetFractionValue = (double)targetNumeratorValue / targetDenominatorValue;

            TargetNumeratorTextBlock.Text = targetNumeratorValue.ToString();
            TargetDenominatorTextBlock.Text = targetDenominatorValue.ToString();
            System.Diagnostics.Debug.WriteLine($"[GMCL] Целевая дробь: {targetNumeratorValue}/{targetDenominatorValue} (Значение: {targetFractionValue})");

            currentOptions = new List<FractionOption>();
            List<ShapeType> availableShapes = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>().ToList();

            // 2. Определяем, сколько будет правильных ответов (например, 1 или 2)
            int numberOfCorrectAnswersToGenerate = random.Next(1, 3); // Будет 1 или 2 правильных ответа
            System.Diagnostics.Debug.WriteLine($"[GMCL] Количество правильных ответов для генерации: {numberOfCorrectAnswersToGenerate}");

            // 3. Генерируем правильные варианты
            for (int i = 0; i < numberOfCorrectAnswersToGenerate; i++)
            {
                FractionOption correctOption = null;
                int attempts = 0;
                while (correctOption == null && attempts < 10) // Пытаемся сгенерировать, чтобы избежать дубликатов или невозможных вариантов
                {
                    correctOption = CreateCorrectFractionOption(targetNumeratorValue, targetDenominatorValue, availableShapes);
                    // Проверка на дубликат с уже существующими опциями (по визуальному представлению)
                    if (correctOption != null && currentOptions.Any(o => o.Shape == correctOption.Shape && o.DisplayedNumerator == correctOption.DisplayedNumerator && o.DisplayedDenominator == correctOption.DisplayedDenominator))
                    {
                        correctOption = null; // Этот вариант уже есть, пробуем снова
                    }
                    attempts++;
                }
                if (correctOption != null)
                {
                    currentOptions.Add(correctOption);
                    System.Diagnostics.Debug.WriteLine($"[GMCL] Сгенерирован правильный вариант: {correctOption.DisplayedNumerator}/{correctOption.DisplayedDenominator} ({correctOption.Shape}), IsCorrect: {correctOption.IsCorrect}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GMCL] Не удалось сгенерировать уникальный правильный вариант {i + 1}");
                    // Если не удалось, можно попробовать создать самый простой правильный вариант
                    // или уменьшить numberOfCorrectAnswersToGenerate
                }
            }

            // Если после попыток правильных вариантов меньше, чем планировалось, добираем простым способом
            while (currentOptions.Count(o => o.IsCorrect) < numberOfCorrectAnswersToGenerate && currentOptions.Count < 4)
            {
                var simpleCorrect = new FractionOption
                {
                    Shape = ShapeType.Circle, // Самый гибкий вариант
                    DisplayedNumerator = targetNumeratorValue,
                    DisplayedDenominator = targetDenominatorValue,
                    IsCorrect = true
                };
                if (!currentOptions.Any(o => o.Shape == simpleCorrect.Shape && o.DisplayedNumerator == simpleCorrect.DisplayedNumerator && o.DisplayedDenominator == simpleCorrect.DisplayedDenominator))
                {
                    currentOptions.Add(simpleCorrect);
                    System.Diagnostics.Debug.WriteLine($"[GMCL] Добавлен простой правильный вариант: {simpleCorrect.DisplayedNumerator}/{simpleCorrect.DisplayedDenominator} (Circle)");
                }
                else break; // Если даже такой уже есть, выходим
            }


            // 4. Генерируем неправильные варианты (дистракторы) до общего количества 4
            int distractorsNeeded = 4 - currentOptions.Count;
            System.Diagnostics.Debug.WriteLine($"[GMCL] Количество дистракторов для генерации: {distractorsNeeded}");

            for (int i = 0; i < distractorsNeeded; i++)
            {
                FractionOption distractorOption = null;
                int attempts = 0;
                while (distractorOption == null && attempts < 10)
                {
                    distractorOption = CreateDistractorFractionOption(targetNumeratorValue, targetDenominatorValue, availableShapes);
                    // Проверка на дубликат и на случайное совпадение с правильным ответом
                    if (distractorOption != null &&
                        (currentOptions.Any(o => o.Shape == distractorOption.Shape && o.DisplayedNumerator == distractorOption.DisplayedNumerator && o.DisplayedDenominator == distractorOption.DisplayedDenominator) ||
                         Math.Abs(distractorOption.Value - targetFractionValue) < 0.0001)) // Проверка, не равен ли дистрактор целевой дроби
                    {
                        distractorOption = null;
                    }
                    attempts++;
                }
                if (distractorOption != null)
                {
                    currentOptions.Add(distractorOption);
                    System.Diagnostics.Debug.WriteLine($"[GMCL] Сгенерирован дистрактор: {distractorOption.DisplayedNumerator}/{distractorOption.DisplayedDenominator} ({distractorOption.Shape}), IsCorrect: {distractorOption.IsCorrect}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GMCL] Не удалось сгенерировать уникальный дистрактор {i + 1}");
                    // Можно добавить простой запасной дистрактор
                    var backupDistractor = new FractionOption
                    {
                        Shape = ShapeType.Circle,
                        DisplayedDenominator = targetDenominatorValue + 1, // чтобы отличался
                        DisplayedNumerator = targetNumeratorValue,
                        IsCorrect = false
                    };
                    // Добавить проверку на дубликат перед добавлением backupDistractor
                    if (!currentOptions.Any(o => o.Shape == backupDistractor.Shape && o.DisplayedNumerator == backupDistractor.DisplayedNumerator && o.DisplayedDenominator == backupDistractor.DisplayedDenominator))
                    {
                        currentOptions.Add(backupDistractor);
                    }
                    else if (currentOptions.Count < 4)
                    { // Если и такой есть, но места еще есть, делаем другой
                        backupDistractor.DisplayedNumerator = Math.Max(0, targetNumeratorValue - 1);
                        if (!currentOptions.Any(o => o.Shape == backupDistractor.Shape && o.DisplayedNumerator == backupDistractor.DisplayedNumerator && o.DisplayedDenominator == backupDistractor.DisplayedDenominator))
                        {
                            currentOptions.Add(backupDistractor);
                        }
                    }
                }
            }

            // Если вариантов всё ещё меньше 4 (из-за проблем с генерацией уникальных), добиваем простыми дистракторами
            int emergencyDistractorCounter = 1;
            while (currentOptions.Count < 4)
            {
                var emergencyOption = new FractionOption
                {
                    Shape = (ShapeType)random.Next(0, availableShapes.Count),
                    DisplayedDenominator = targetDenominatorValue + emergencyDistractorCounter, // Делаем отличный знаменатель
                    DisplayedNumerator = Math.Max(1, targetNumeratorValue + emergencyDistractorCounter % 2), // И числитель
                    IsCorrect = false
                };
                // Упрощенная проверка на дубликат
                if (!currentOptions.Any(o => o.DisplayedNumerator == emergencyOption.DisplayedNumerator && o.DisplayedDenominator == emergencyOption.DisplayedDenominator))
                {
                    currentOptions.Add(emergencyOption);
                }
                emergencyDistractorCounter++;
                if (emergencyDistractorCounter > 10) break; // Предохранитель от бесконечного цикла
            }


            // 5. Перемешиваем варианты
            currentOptions = currentOptions.OrderBy(x => random.Next()).ToList();
            System.Diagnostics.Debug.WriteLine($"[GMCL] Варианты после перемешивания:");
            foreach (var opt in currentOptions) { System.Diagnostics.Debug.WriteLine($"  {opt.DisplayedNumerator}/{opt.DisplayedDenominator} ({opt.Shape}), Correct: {opt.IsCorrect}"); }


            // 6. Отображаем варианты на UI
            for (int i = 0; i < optionShapes.Count; i++)
            {
                if (i < currentOptions.Count)
                {
                    optionShapes[i].CurrentShapeType = currentOptions[i].Shape;
                    optionShapes[i].Denominator = currentOptions[i].DisplayedDenominator;
                    optionShapes[i].TargetNumerator = currentOptions[i].DisplayedNumerator;
                    optionShapes[i].ResetUserSelectionAndDraw();

                    optionToggleButtons[i].IsChecked = false;
                    optionToggleButtons[i].Visibility = Visibility.Visible;
                    // Сохраняем сам объект FractionOption в Tag кнопки для легкого доступа при проверке
                    optionToggleButtons[i].Tag = currentOptions[i];
                }
                else
                {
                    optionToggleButtons[i].Visibility = Visibility.Collapsed;
                }
            }
            System.Diagnostics.Debug.WriteLine("--- GenerateMultipleChoiceLevel: Конец ---");
            ResetButtonAndFeedbackState();
        }
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
                    if (optionDen > 16)
                    {
                        k = 1;
                        optionNum = targetNum * k;
                        optionDen = targetDen * k;
                    }
                    break;
                case ShapeType.Triangle:
                    if ((targetNum * 3) % targetDen == 0)
                    {
                        optionNum = (targetNum * 3) / targetDen;
                        optionDen = 3;
                        if (optionNum < 1 || optionNum > optionDen) return null;
                    }
                    else return null;
                    break;
                case ShapeType.Diamond:
                    if ((targetNum * 4) % targetDen == 0)
                    {
                        optionNum = (targetNum * 4) / targetDen;
                        optionDen = 4;
                        if (optionNum < 1 || optionNum > optionDen) return null;
                    }
                    else return null;
                    break;
                case ShapeType.Octagon:
                    if ((targetNum * 8) % targetDen == 0)
                    {
                        optionNum = (targetNum * 8) / targetDen;
                        optionDen = 8;
                        if (optionNum < 1 || optionNum > optionDen) return null;
                    }
                    else return null;
                    break;
                default:
                    return null;
            }
  
            if (optionNum == 0 && optionDen > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[CreateCorrectFractionOption] Сгенерирован корректный вариант с нулевым числителем для {shape}: {optionNum}/{optionDen} из цели {targetNum}/{targetDen}. Этого не должно быть.");
                return null;
            }

            return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = true };
        }

        // Вспомогательный метод для создания НЕПРАВИЛЬНОГО варианта (дистрактора)
        private FractionOption CreateDistractorFractionOption(int targetNum, int targetDen, List<ShapeType> availableShapes)
        {
            ShapeType shape = availableShapes[random.Next(availableShapes.Count)];
            int optionNum;
            int optionDen;
            double targetValue = (double)targetNum / targetDen;

            // Определяем знаменатель в зависимости от фигуры
            switch (shape)
            {
                case ShapeType.Circle:
                    optionDen = random.Next(Math.Max(2, targetDen - 2), targetDen + 3);
                    optionDen = Math.Max(2, optionDen);
                    optionDen = Math.Min(16, optionDen);
                    break;
                case ShapeType.Triangle:
                    optionDen = 3;
                    break;
                case ShapeType.Diamond:
                    optionDen = 4;
                    break;
                case ShapeType.Octagon:
                    optionDen = 8;
                    break;
                default:
                    return null;
            }

            // Генерируем числитель (optionNum) так, чтобы он был от 1 до optionDen-1,
            // и чтобы итоговая дробь не совпадала с целевой.
            int attempts = 0;
            if (optionDen < 2 && (shape == ShapeType.Triangle || shape == ShapeType.Diamond || shape == ShapeType.Octagon))
            {
                
                System.Diagnostics.Debug.WriteLine($"[CreateDistractor] Знаменатель {optionDen} слишком мал для генерации числителя < знаменателя и > 0.");
                return null; // Не можем сгенерировать такой дистрактор
            }


            do
            {
                // ИЗМЕНЕНИЕ ЗДЕСЬ: optionNum генерируется от 1 до optionDen - 1 (не включая optionDen)
                // Это гарантирует, что числитель будет меньше знаменателя и больше 0.
                // random.Next(minValue, maxValue) - maxValue не включается.
                // Поэтому, если optionDen = 2, random.Next(1, 2) вернет только 1.
                // Если optionDen = 3, random.Next(1, 3) вернет 1 или 2.
                if (optionDen <= 1) // Если знаменатель 1 или меньше, невозможно создать дробь num < den и num >= 1
                {
                    // Попытка сгенерировать другой знаменатель для круга, или вернуть null
                    if (shape == ShapeType.Circle)
                    {
                        optionDen = random.Next(2, 8); // Пробуем еще раз для круга
                        if (optionDen <= 1) return null; // Если опять не вышло
                    }
                    else
                    {
                        return null; // Для других фигур с фиксированным знаменателем, если он стал <=1, это ошибка логики выше.
                    }
                }
                optionNum = random.Next(1, optionDen);

                attempts++;
                if (attempts > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"[CreateDistractor] Не удалось подобрать уникальный неверный числитель за {attempts} попыток для {shape} {optionNum}/{optionDen}, цель {targetValue}");
                    return null;
                }
            } while (optionDen != 0 && Math.Abs((double)optionNum / optionDen - targetValue) < 0.0001);


            return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = false };
        }


        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonContent = CheckButton.Content.ToString();

            if (buttonContent == "Продолжить")
            {
                GenerateMultipleChoiceLevel();
                return;
            }
            else if (buttonContent == "Заново")
            {
                // Сбрасываем выбор пользователя (ToggleButtons), но не генерируем новый уровень
                foreach (var btn in optionToggleButtons)
                {
                    btn.IsChecked = false;
                }
                ResetButtonAndFeedbackState(); // Возвращаем кнопку и панель в исходное состояние
                return;
            }

            // --- Логика для состояния "Проверить" ---
            int totalCorrectOptionsInLevel = currentOptions.Count(opt => opt.IsCorrect);
            int userMadeCorrectSelections = 0;
            int userMadeIncorrectSelections = 0;

            for (int i = 0; i < optionToggleButtons.Count; i++)
            {
                if (i < currentOptions.Count && optionToggleButtons[i].Visibility == Visibility.Visible)
                {
                    bool isSelected = optionToggleButtons[i].IsChecked == true;
                    // Получаем данные опции из Tag, который мы установили при генерации
                    if (optionToggleButtons[i].Tag is FractionOption optionData)
                    {
                        if (isSelected)
                        {
                            if (optionData.IsCorrect) userMadeCorrectSelections++;
                            else userMadeIncorrectSelections++;
                        }
                    }
                }
            }

            if (userMadeCorrectSelections == totalCorrectOptionsInLevel && userMadeIncorrectSelections == 0 && totalCorrectOptionsInLevel > 0)
            {
                ShowSuccessFeedback();
            }
            else
            {
                ShowErrorFeedback();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = this.Owner ?? Application.Current.MainWindow;
            if (ownerWindow != null && ownerWindow != this) { if (!ownerWindow.IsVisible) ownerWindow.Show(); ownerWindow.Focus(); }
            this.Close();
        }


        private void ShowSuccessFeedback()
        {
            FeedbackText.Text = "✓";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Продолжить";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
        }

        private void ShowErrorFeedback()
        {
            FeedbackText.Text = "✗";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Заново";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";
            CheckButton.IsEnabled = true; // Убедимся, что кнопка активна

            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");
        }
    }
}