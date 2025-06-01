using System;
using System.Windows;

namespace FractionTrainer // Убедитесь, что пространство имен совпадает
{
    public partial class LearningModeWindow : Window
    {
        private Random random = new Random();

        private int sectorsToSelect;
        private int totalSectorsInShape; // Переименовано для ясности
        private int baseNumeratorToDisplay;
        private int baseDenominatorToDisplay;

        public LearningModeWindow()
        {
            InitializeComponent(); // Убедитесь, что эта строка не вызывает ошибок
            GenerateNewLevel();
        }

        // В классе LearningModeWindow
        // В файле LearningModeWindow.xaml.cs

        private void GenerateNewLevel()
        {
            System.Diagnostics.Debug.WriteLine("--- GenerateNewLevel: Начало ---");

            ShapeType selectedShape;
            int shapeChoice = random.Next(0, 4); // 0: Circle, 1: Triangle, 2: Octagon, 3: Diamond
            System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] shapeChoice (0-C, 1-T, 2-O, 3-D): {shapeChoice}");

            switch (shapeChoice)
            {
                case 0: selectedShape = ShapeType.Circle; break;
                case 1: selectedShape = ShapeType.Triangle; break;
                case 2: selectedShape = ShapeType.Octagon; break;
                default: // case 3
                    selectedShape = ShapeType.Diamond;
                    break;
            }
            System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] selectedShape: {selectedShape}");

            if (FractionDisplay == null)
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] FractionDisplay is NULL. Выход.");
                return;
            }
            FractionDisplay.CurrentShapeType = selectedShape;

            if (selectedShape == ShapeType.Triangle)
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] Логика для Треугольника");
                totalSectorsInShape = 3;
                sectorsToSelect = random.Next(1, 3); // 1 или 2
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Triangle: sectorsToSelect = {sectorsToSelect}");

                int multiplier = random.Next(2, 5); // Множитель от 2 до 4
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Triangle: multiplier = {multiplier}");

                baseNumeratorToDisplay = sectorsToSelect * multiplier;
                baseDenominatorToDisplay = totalSectorsInShape * multiplier;
            }
            else if (selectedShape == ShapeType.Octagon)
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] Логика для Восьмиугольника");
                totalSectorsInShape = 8;
                int trueNumeratorForOctagon = random.Next(1, 8); // от 1 до 7
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Octagon: trueNumeratorForOctagon = {trueNumeratorForOctagon}");
                sectorsToSelect = trueNumeratorForOctagon;

                int commonDivisor = GCD(trueNumeratorForOctagon, totalSectorsInShape);
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Octagon: commonDivisor = {commonDivisor}");

                baseNumeratorToDisplay = trueNumeratorForOctagon / commonDivisor;
                baseDenominatorToDisplay = totalSectorsInShape / commonDivisor;
            }
            else if (selectedShape == ShapeType.Diamond)
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] Логика для Алмаза");
                totalSectorsInShape = 4;
                baseNumeratorToDisplay = random.Next(1, 4); // 1, 2, или 3
                baseDenominatorToDisplay = 4;
                sectorsToSelect = baseNumeratorToDisplay;
            }
            else // ShapeType.Circle
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] Логика для Круга");
                int baseDen = random.Next(2, 7);
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Circle: baseDen = {baseDen}");
                int baseNum = random.Next(1, baseDen);
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Circle: baseNum = {baseNum}");

                baseNumeratorToDisplay = baseNum;
                baseDenominatorToDisplay = baseDen;

                int multiplier = random.Next(1, 5);
                System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Circle: multiplier = {multiplier}");

                totalSectorsInShape = baseDen * multiplier;
                sectorsToSelect = baseNum * multiplier;
            }

            System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Итоговые значения перед отображением: baseNumeratorToDisplay = {baseNumeratorToDisplay}, baseDenominatorToDisplay = {baseDenominatorToDisplay}");
            System.Diagnostics.Debug.WriteLine($"[GenerateNewLevel] Итоговые значения для контрола: sectorsToSelect = {sectorsToSelect}, totalSectorsInShape = {totalSectorsInShape}");


            if (NumeratorTextBlock != null && DenominatorTextBlock != null)
            {
                NumeratorTextBlock.Text = baseNumeratorToDisplay.ToString();
                DenominatorTextBlock.Text = baseDenominatorToDisplay.ToString();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] ОШИБКА: NumeratorTextBlock или DenominatorTextBlock не найдены!");
            }

            if (FractionDisplay != null)
            {
                FractionDisplay.TargetNumerator = sectorsToSelect;
                FractionDisplay.Denominator = totalSectorsInShape;
                FractionDisplay.ResetUserSelectionAndDraw();
            }
            System.Diagnostics.Debug.WriteLine("--- GenerateNewLevel: Конец ---");
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] Перед вызовом FractionDisplay.UserSelectedSectorsCount.");
            int userSelection = -1;

            if (FractionDisplay == null)
            {
                System.Diagnostics.Debug.WriteLine("[CheckButton_Click] FractionDisplay IS NULL!");
                CustomMessageBoxWindow.Show("Ошибка: компонент отображения дроби не инициализирован.", "Критическая ошибка", this);
                return;
            }

            try
            {
                userSelection = FractionDisplay.UserSelectedSectorsCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] ИСКЛЮЧЕНИЕ при вызове UserSelectedSectorsCount: {ex.ToString()}");
                CustomMessageBoxWindow.Show($"Произошла ошибка при получении выбора: {ex.Message}", "Ошибка", this);
                return;
            }
            System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] СРАЗУ ПОСЛЕ вызова. userSelection = {userSelection}");

            if (userSelection == sectorsToSelect)
            {
                // Используем наше кастомное окно
                CustomMessageBoxWindow.Show("Правильно! Следующий уровень.", "Результат", this);
                GenerateNewLevel();
            }
            else
            {
                // Используем наше кастомное окно
                CustomMessageBoxWindow.Show($"Неправильно. Вы выбрали {userSelection} из {totalSectorsInShape}. Попробуйте еще раз! (Нужно было собрать дробь {baseNumeratorToDisplay}/{baseDenominatorToDisplay})", "Результат", this);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var ownerWindow = this.Owner ?? Application.Current.MainWindow;
            if (ownerWindow != null && ownerWindow != this) // Не показываем сами себя, если мы и есть MainWindow
            {
                if (!ownerWindow.IsVisible) ownerWindow.Show();
                ownerWindow.Focus(); // Передаем фокус
            }
            this.Close();
        }
        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Math.Abs(a); // Возвращаем абсолютное значение на случай отрицательных чисел, хотя здесь они не ожидаются
        }
    }
}