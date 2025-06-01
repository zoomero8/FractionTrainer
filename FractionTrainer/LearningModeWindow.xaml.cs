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
            if (random.Next(0, 2) == 0)
            {
                selectedShape = ShapeType.Circle;
            }
            else
            {
                selectedShape = ShapeType.Triangle;
            }

            if (FractionDisplay == null) // Добавлена проверка на null
            {
                System.Diagnostics.Debug.WriteLine("[GenerateNewLevel] FractionDisplay is NULL before setting shape type!");
                return; // Выходим, если контрол не создан
            }
            FractionDisplay.CurrentShapeType = selectedShape;

            if (selectedShape == ShapeType.Triangle)
            {
                baseDenominatorToDisplay = 3; // Знаменатель для отображения всегда 3
                baseNumeratorToDisplay = random.Next(1, baseDenominatorToDisplay + 1); // Числитель 1, 2 или 3 (1/3, 2/3, 3/3)
                                                                                       // Если хотите только правильные дроби (меньше 1), то random.Next(1, baseDenominatorToDisplay)

                totalSectorsInShape = 3;
                sectorsToSelect = baseNumeratorToDisplay; // Сколько нужно выбрать секторов треугольника
            }
            else // ShapeType.Circle
            {
                int baseDen = random.Next(2, 5);
                int baseNum = random.Next(1, baseDen);

                baseNumeratorToDisplay = baseNum;
                baseDenominatorToDisplay = baseDen;

                int multiplier = random.Next(1, 4);

                totalSectorsInShape = baseDen * multiplier;
                sectorsToSelect = baseNum * multiplier;
            }

            if (TargetFractionTextBlock != null)
            {
                TargetFractionTextBlock.Text = $"{baseNumeratorToDisplay}/{baseDenominatorToDisplay}";
            }

            if (FractionDisplay != null)
            {
                FractionDisplay.TargetNumerator = sectorsToSelect;
                FractionDisplay.Denominator = totalSectorsInShape; // Для треугольника это будет 3
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
                MessageBox.Show("Ошибка: компонент отображения дроби не инициализирован.", "Критическая ошибка");
                return;
            }

            try
            {
                userSelection = FractionDisplay.UserSelectedSectorsCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] ИСКЛЮЧЕНИЕ при вызове UserSelectedSectorsCount: {ex.ToString()}");
                MessageBox.Show($"Произошла ошибка при получении выбора: {ex.Message}", "Ошибка");
                return;
            }
            System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] СРАЗУ ПОСЛЕ вызова. userSelection = {userSelection}");

            if (userSelection == sectorsToSelect)
            {
                MessageBox.Show("Правильно! Следующий уровень.", "Результат");
                GenerateNewLevel();
            }
            else
            {
                MessageBox.Show($"Неправильно. Вы выбрали {userSelection} из {totalSectorsInShape}. Попробуйте еще раз! (Нужно было собрать дробь {baseNumeratorToDisplay}/{baseDenominatorToDisplay})", "Результат");
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
    }
}