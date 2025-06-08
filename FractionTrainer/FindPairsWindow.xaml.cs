using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FractionTrainer
{
    public partial class FindPairsWindow : Window
    {
        // --- Поля класса ---
        private readonly Random random = new Random();
        private List<FractionOption> currentOptions;
        private readonly List<ToggleButton> optionToggleButtons;
        private readonly List<FractionShapeVisualizer> optionShapes;

        // --- Конструктор ---
        public FindPairsWindow()
        {
            InitializeComponent();
            // Собираем UI элементы в списки для удобного доступа
            optionToggleButtons = new List<ToggleButton> { OptionButton1, OptionButton2, OptionButton3, OptionButton4, OptionButton5, OptionButton6 };
            optionShapes = new List<FractionShapeVisualizer> { OptionShape1, OptionShape2, OptionShape3, OptionShape4, OptionShape5, OptionShape6 };

            GeneratePairsLevel();
        }

        // --- Основная логика ---

        private void GeneratePairsLevel()
        {
            ResetButtonAndFeedbackState();
            currentOptions = new List<FractionOption>();
            var availableShapes = Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>().ToList();
            var usedFractionValues = new HashSet<double>();

            // 1. Определяем количество пар (1, 2 или 3)
            int pairsToGenerate = random.Next(1, 4);

            // 2. Генерируем пары
            for (int i = 0; i < pairsToGenerate; i++)
            {
                // Генерируем уникальное значение для новой пары
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
                usedFractionValues.Add(pairValue);

                // --- ИЗМЕНЕННАЯ ЛОГИКА ГЕНЕРАЦИИ ПАРЫ ---

                // Сначала генерируем первый элемент пары
                FractionOption option1 = null;
                int attempts1 = 0;
                while (option1 == null && attempts1 < 20)
                {
                    option1 = CreateCorrectFractionOption(baseNum, baseDen, availableShapes, currentOptions);
                    attempts1++;
                }

                if (option1 == null)
                {
                    usedFractionValues.Remove(pairValue); // Освобождаем значение, т.к. пара не создана
                    continue; // Не удалось создать первую часть, пропускаем пару
                }

                // Затем генерируем второй элемент, убеждаясь, что у него ДРУГОЕ кол-во закрашенных секторов
                FractionOption option2 = null;
                int attempts2 = 0;
                while (option2 == null && attempts2 < 20)
                {
                    var potentialOption2 = CreateCorrectFractionOption(baseNum, baseDen, availableShapes, currentOptions.Concat(new[] { option1 }).ToList());

                    // Проверяем, что вариант создан И что кол-во закрашенных секторов не совпадает
                    if (potentialOption2 != null && potentialOption2.DisplayedNumerator != option1.DisplayedNumerator)
                    {
                        option2 = potentialOption2;
                    }
                    attempts2++;
                }

                if (option2 == null)
                {
                    // Если не удалось найти подходящую вторую часть, то и первая нам не нужна.
                    // Убираем ее из usedFractionValues, чтобы не было "одиночек" от пар.
                    usedFractionValues.Remove(pairValue);
                }
                else
                {
                    // Если все успешно, добавляем обе части в список
                    currentOptions.Add(option1);
                    currentOptions.Add(option2);
                }
            }

            // 3. Добавляем дистракторы (уникальные "одиночные" дроби)
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
                } while (distractor == null && attempts < 20);

                if (distractor != null)
                {
                    currentOptions.Add(distractor);
                }
                else
                {
                    break; // Не удалось создать уникальный дистрактор, выходим
                }
            }

            // 4. Перемешиваем и отображаем
            currentOptions = currentOptions.OrderBy(x => random.Next()).ToList();

            for (int i = 0; i < optionShapes.Count; i++)
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
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckButton.Content.ToString() == "Продолжить" || CheckButton.Content.ToString() == "Заново")
            {
                GeneratePairsLevel();
                return;
            }

            var selectedOptions = optionToggleButtons
                .Where(btn => btn.IsChecked == true && btn.Tag is FractionOption)
                .Select(btn => btn.Tag as FractionOption)
                .ToList();

            var allPairedOptionsInLevel = currentOptions
                .GroupBy(opt => opt.Value)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .ToList();

            bool isCorrect = allPairedOptionsInLevel.Count == selectedOptions.Count &&
                             selectedOptions.All(opt => allPairedOptionsInLevel.Contains(opt));

            if (isCorrect && selectedOptions.Any())
            {
                ShowSuccessFeedback();
            }
            else
            {
                ShowErrorFeedback();
            }
        }

        // --- Вспомогательные методы (обратная связь, навигация, генерация) ---

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

            // --- ИЗМЕНЕННАЯ И ИСПРАВЛЕННАЯ ЛОГИКА ---
            // Удаляем старый код, который вызывал ошибку.
            // Вместо него напрямую устанавливаем ссылка на ресурсы из нашей темы.
            // Это C#-эквивалент записи: Background="{DynamicResource ButtonAccentBrush}"
            CheckButton.SetResourceReference(Button.BackgroundProperty, "ButtonAccentBrush");
            CheckButton.SetResourceReference(Button.ForegroundProperty, "ButtonTextBrush");
        }

        private FractionOption CreateCorrectFractionOption(int targetNum, int targetDen, List<ShapeType> availableShapes, List<FractionOption> existingOptions)
        {
            int attempts = 0;
            while (attempts < 20)
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
                        if ((targetNum * 3) % targetDen == 0) { optionNum = (targetNum * 3) / targetDen; optionDen = 3; if (optionNum < 1 || optionNum > optionDen) continue; } else continue;
                        break;
                    case ShapeType.Diamond:
                        if ((targetNum * 4) % targetDen == 0) { optionNum = (targetNum * 4) / targetDen; optionDen = 4; if (optionNum < 1 || optionNum > optionDen) continue; } else continue;
                        break;
                    case ShapeType.Octagon:
                        if ((targetNum * 8) % targetDen == 0) { optionNum = (targetNum * 8) / targetDen; optionDen = 8; if (optionNum < 1 || optionNum > optionDen) continue; } else continue;
                        break;
                    default: continue;
                }

                if (existingOptions == null || !existingOptions.Any(o => o.Shape == shape && o.DisplayedNumerator == optionNum && o.DisplayedDenominator == optionDen))
                {
                    return new FractionOption { Shape = shape, DisplayedNumerator = optionNum, DisplayedDenominator = optionDen, IsCorrect = true };
                }
                attempts++;
            }
            return null;
        }

        private static int GCD(int a, int b)
        {
            while (b != 0) { int temp = b; b = a % b; a = temp; }
            return Math.Abs(a);
        }
    }
}
