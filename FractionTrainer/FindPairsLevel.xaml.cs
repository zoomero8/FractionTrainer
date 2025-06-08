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
    public partial class FindPairsLevel : UserControl, ILevelControl
    {
        public event EventHandler<bool> LevelCompleted;

        private readonly Random random = new Random();
        private List<FractionOption> currentOptions;
        private readonly List<ToggleButton> optionToggleButtons;
        private readonly List<FractionShapeVisualizer> optionShapes;

        // --- ИЗМЕНЕНИЕ: Упрощенные цвета для выделения ---
        private readonly SolidColorBrush defaultBg = Brushes.White;
        private readonly SolidColorBrush defaultBorder = new SolidColorBrush(Color.FromRgb(221, 221, 221));

        // Цвета для выбранной ПАРЫ (только синий)
        private readonly SolidColorBrush pairBg = new SolidColorBrush(Color.FromRgb(224, 240, 255)); // Светло-синий
        private readonly SolidColorBrush pairBorder = new SolidColorBrush(Color.FromRgb(0, 122, 255));   // Синий

        // Цвета для ОДИНОЧНОГО выбора
        private readonly SolidColorBrush singleSelectedBg = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // Светло-серый
        private readonly SolidColorBrush singleSelectedBorder = Brushes.Gray;


        public FindPairsLevel()
        {
            InitializeComponent();

            optionToggleButtons = new List<ToggleButton> { OptionButton1, OptionButton2, OptionButton3, OptionButton4, OptionButton5, OptionButton6 };
            optionShapes = new List<FractionShapeVisualizer> { OptionShape1, OptionShape2, OptionShape3, OptionShape4, OptionShape5, OptionShape6 };

            GenerateLevel();
        }

        public void GenerateLevel()
        {
            ResetButtonAndFeedbackState();
            currentOptions = new List<FractionOption>();
            var availableShapes = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>().ToList();
            var usedFractionValues = new HashSet<double>();

            int pairsToGenerate = random.Next(1, 4);

            // Генерация пар
            for (int i = 0; i < pairsToGenerate; i++)
            {
                double pairValue;
                int baseNum, baseDen;
                int attempts = 0;
                do
                {
                    baseDen = random.Next(2, 6);
                    baseNum = random.Next(1, baseDen);
                    pairValue = (double)baseNum / baseDen;
                    attempts++;
                } while (usedFractionValues.Contains(pairValue) && attempts < 20);

                if (attempts >= 20) continue;

                var option1 = CreateCorrectFractionOption(baseNum, baseDen, availableShapes, currentOptions);
                if (option1 == null) continue;

                var option2 = CreateDifferentVisualOption(baseNum, baseDen, availableShapes, option1, currentOptions);

                if (option2 != null)
                {
                    usedFractionValues.Add(pairValue);
                    currentOptions.Add(option1);
                    currentOptions.Add(option2);
                }
            }

            // Генерация "одиночных" дробей (дистракторов)
            while (currentOptions.Count < 6)
            {
                FractionOption distractor = null;
                int attempts = 0;
                do
                {
                    int distractorDen = random.Next(2, 9);
                    int distractorNum = random.Next(1, distractorDen);
                    double distractorValue = (double)distractorNum / distractorDen;

                    if (!usedFractionValues.Contains(distractorValue))
                    {
                        distractor = CreateCorrectFractionOption(distractorNum, distractorDen, availableShapes, currentOptions);
                        if (distractor != null)
                        {
                            usedFractionValues.Add(distractorValue);
                        }
                    }
                    attempts++;
                } while (distractor == null && attempts < 50);

                if (distractor != null) currentOptions.Add(distractor);
                else break;
            }

            // Перемешивание и отображение
            currentOptions = currentOptions.OrderBy(x => random.Next()).ToList();
            for (int i = 0; i < optionToggleButtons.Count; i++)
            {
                if (i < currentOptions.Count)
                {
                    var option = currentOptions[i];
                    optionShapes[i].CurrentShapeType = option.Shape;
                    optionShapes[i].Denominator = option.DisplayedDenominator;
                    optionShapes[i].TargetNumerator = option.DisplayedNumerator;
                    optionShapes[i].ResetUserSelectionAndDraw();

                    optionToggleButtons[i].IsChecked = false;
                    optionToggleButtons[i].Visibility = Visibility.Visible;
                    optionToggleButtons[i].Tag = option;
                }
                else
                {
                    optionToggleButtons[i].Visibility = Visibility.Collapsed;
                }
            }
            // Первоначальная установка цветов
            UpdateSelectionColors();
        }

        private void OptionButton_StateChanged(object sender, RoutedEventArgs e)
        {
            UpdateSelectionColors();
            if (CheckButton.Content.ToString() != "Проверить")
            {
                ResetButtonAndFeedbackState();
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckButton.Content.ToString() == "Заново")
            {
                LevelCompleted?.Invoke(this, false);
                return;
            }

            var selectedOptions = optionToggleButtons
                .Where(btn => btn.IsChecked == true && btn.Visibility == Visibility.Visible)
                .Select(btn => btn.Tag as FractionOption)
                .ToList();

            var allPairedOptionsInLevel = currentOptions
                .GroupBy(opt => opt.Value)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .ToList();

            bool isCorrect = allPairedOptionsInLevel.Any() &&
                             allPairedOptionsInLevel.Count == selectedOptions.Count &&
                             selectedOptions.All(opt => allPairedOptionsInLevel.Contains(opt));

            if (isCorrect)
            {
                ShowSuccessFeedback();
                await Task.Delay(1200);
                LevelCompleted?.Invoke(this, true);
            }
            else
            {
                ShowErrorFeedback();
            }
        }

        // --- ИЗМЕНЕНИЕ: Упрощенная логика обновления цветов ---
        private void UpdateSelectionColors()
        {
            var selectedOptions = optionToggleButtons
                .Where(b => b.IsChecked == true && b.Visibility == Visibility.Visible)
                .Select(b => b.Tag as FractionOption)
                .ToList();

            // 1. Находим значения дробей, которые образуют пару (выбрано 2 или больше с одинаковым значением)
            var pairedValues = selectedOptions
                .GroupBy(opt => opt.Value)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            // 2. Применяем стили ко всем кнопкам
            foreach (var btn in optionToggleButtons)
            {
                if (btn.IsChecked == true && btn.Tag is FractionOption option)
                {
                    // 3. Если значение кнопки есть в списке пар, красим в синий
                    if (pairedValues.Contains(option.Value))
                    {
                        ApplyStyleToButton(btn, pairBg, pairBorder, 2);
                    }
                    else // 4. Иначе это одиночный выбор, красим в серый
                    {
                        ApplyStyleToButton(btn, singleSelectedBg, singleSelectedBorder, 2);
                    }
                }
                else // 5. Кнопка не выбрана, сбрасываем стиль
                {
                    ApplyStyleToButton(btn, defaultBg, defaultBorder, 1);
                }
            }
        }

        private void ApplyStyleToButton(ToggleButton btn, Brush background, Brush borderBrush, int thickness)
        {
            btn.Background = background;
            btn.BorderBrush = borderBrush;
            btn.BorderThickness = new Thickness(thickness);
        }

        #region Helper Methods (Generation & Feedback)
        private FractionOption CreateDifferentVisualOption(int baseNum, int baseDen, List<ShapeType> availableShapes, FractionOption firstOption, List<FractionOption> existingOptions)
        {
            int attempts = 0;
            while (attempts < 20)
            {
                var potentialOption = CreateCorrectFractionOption(baseNum, baseDen, availableShapes, existingOptions.Concat(new[] { firstOption }).ToList());
                if (potentialOption != null && (potentialOption.DisplayedNumerator != firstOption.DisplayedNumerator || potentialOption.DisplayedDenominator != firstOption.DisplayedDenominator || potentialOption.Shape != firstOption.Shape))
                {
                    return potentialOption;
                }
                attempts++;
            }
            return null;
        }

        private FractionOption CreateCorrectFractionOption(int targetNum, int targetDen, List<ShapeType> availableShapes, List<FractionOption> existingOptions)
        {
            int attempts = 0;
            while (attempts < 20)
            {
                ShapeType shape = availableShapes[random.Next(availableShapes.Count)];
                int optionNum = 0, optionDen = 0;
                bool possible = false;
                switch (shape)
                {
                    case ShapeType.Circle:
                        int k = random.Next(1, 4);
                        optionNum = targetNum * k;
                        optionDen = targetDen * k;
                        if (optionDen > 16) { k = 1; optionNum = targetNum * k; optionDen = targetDen * k; }
                        possible = true;
                        break;
                    case ShapeType.Triangle:
                        if ((targetNum * 3) % targetDen == 0) { optionNum = (targetNum * 3) / targetDen; optionDen = 3; possible = (optionNum >= 1 && optionNum < optionDen); }
                        break;
                    case ShapeType.Diamond:
                        if ((targetNum * 4) % targetDen == 0) { optionNum = (targetNum * 4) / targetDen; optionDen = 4; possible = (optionNum >= 1 && optionNum < optionDen); }
                        break;
                    case ShapeType.Octagon:
                        if ((targetNum * 8) % targetDen == 0) { optionNum = (targetNum * 8) / targetDen; optionDen = 8; possible = (optionNum >= 1 && optionNum < optionDen); }
                        break;
                }
                if (possible && !existingOptions.Any(o => o.Shape == shape && o.DisplayedNumerator == optionNum && o.DisplayedDenominator == optionDen))
                {
                    return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = true };
                }
                attempts++;
            }
            return null;
        }

        private void ResetButtonAndFeedbackState()
        {
            FeedbackBackground.Background = Brushes.Transparent;
            FeedbackText.Visibility = Visibility.Collapsed;
            CheckButton.Content = "Проверить";
            CheckButton.IsEnabled = true;

            if (Application.Current.TryFindResource("ModernButton") is Style modernButtonStyle &&
                modernButtonStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == Control.BackgroundProperty) is Setter backgroundSetter)
            {
                CheckButton.Background = (Brush)backgroundSetter.Value;
            }
            else
            {
                CheckButton.Background = new SolidColorBrush(Color.FromRgb(0, 122, 255));
            }
        }

        private void ShowSuccessFeedback()
        {
            FeedbackBackground.Background = new SolidColorBrush(Color.FromRgb(224, 251, 226));
            FeedbackText.Text = "✓";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Отлично!";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            CheckButton.IsEnabled = false;
        }

        private void ShowErrorFeedback()
        {
            FeedbackBackground.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
            FeedbackText.Text = "✗";
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            FeedbackText.Visibility = Visibility.Visible;
            CheckButton.Content = "Заново";
            CheckButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
        }
        #endregion
    }
}