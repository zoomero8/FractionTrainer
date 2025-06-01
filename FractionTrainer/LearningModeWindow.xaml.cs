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

        private void GenerateNewLevel()
        {
            ShapeType selectedShape;
            int shapeChoice = random.Next(0, 3); // 0: Circle, 1: Triangle, 2: Octagon

            switch (shapeChoice)
            {
                case 0:
                    selectedShape = ShapeType.Circle;
                    break;
                case 1:
                    selectedShape = ShapeType.Triangle;
                    break;
                default: // case 2
                    selectedShape = ShapeType.Octagon;
                    break;
            }

            if (FractionDisplay == null)
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] FractionDisplay is NULL before setting shape type!");
                return;
            }
            FractionDisplay.CurrentShapeType = selectedShape;

            if (selectedShape == ShapeType.Triangle)
            {
                totalSectorsInShape = 3;
                // Базовая задача: выбрать 1 или 2 сектора (избегаем 3/3)
                sectorsToSelect = random.Next(1, 3); // Генерирует 1 или 2

                // Умножаем базовую дробь (sectorsToSelect / totalSectorsInShape) для усложнения
                int multiplier = random.Next(2, 5); // Множитель от 2 до 4
                baseNumeratorToDisplay = sectorsToSelect * multiplier;
                baseDenominatorToDisplay = totalSectorsInShape * multiplier;
                // Пример: если sectorsToSelect=1, показываем 2/6, 3/9, или 4/12. Пользователь должен сократить до 1/3.
            }
            else if (selectedShape == ShapeType.Octagon)
            {
                totalSectorsInShape = 8;
                // Генерируем "истинный" числитель для 8 секторов (избегаем 8/8)
                int trueNumeratorForOctagon = random.Next(1, 8); // Генерирует от 1 до 7

                sectorsToSelect = trueNumeratorForOctagon;

                // Сокращаем дробь (trueNumeratorForOctagon / totalSectorsInShape) для отображения
                int commonDivisor = GCD(trueNumeratorForOctagon, totalSectorsInShape);
                baseNumeratorToDisplay = trueNumeratorForOctagon / commonDivisor;
                baseDenominatorToDisplay = totalSectorsInShape / commonDivisor;
                // Пример: если trueNumeratorForOctagon=6, то дробь 6/8, commonDivisor=2, отображаем 3/4.
                // Пользователь видит 3/4 и должен выбрать 6 из 8 секторов.
            }
            else // ShapeType.Circle
            {
                // Логика для круга: базовый знаменатель 2-6, множитель 1-4
                // Гарантируем, что базовая дробь не N/N
                int baseDen = random.Next(2, 7); // Базовый знаменатель от 2 до 6
                int baseNum = random.Next(1, baseDen); // Базовый числитель от 1 до baseDen-1

                baseNumeratorToDisplay = baseNum;
                baseDenominatorToDisplay = baseDen;

                int multiplier = random.Next(1, 5); // Множитель от 1 до 4

                totalSectorsInShape = baseDen * multiplier;
                sectorsToSelect = baseNum * multiplier;
                // Эта логика уже гарантирует, что sectorsToSelect < totalSectorsInShape,
                // так как baseNum < baseDen.
            }

            if (NumeratorTextBlock != null && DenominatorTextBlock != null) // Проверяем оба TextBlock'a
            {
                NumeratorTextBlock.Text = baseNumeratorToDisplay.ToString();
                DenominatorTextBlock.Text = baseDenominatorToDisplay.ToString();
            }
            else
            {
                // Если элементы не найдены (что было бы странно, если XAML корректен),
                // можно вывести отладочное сообщение
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] NumeratorTextBlock or DenominatorTextBlock is NULL!");
            }

            if (FractionDisplay != null)
            {
                FractionDisplay.TargetNumerator = sectorsToSelect; // Это "истинное" количество секторов для выбора
                FractionDisplay.Denominator = totalSectorsInShape; // Это количество секторов на фигуре
                FractionDisplay.ResetUserSelectionAndDraw();
            }
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