using System;
using System.Windows;

namespace FractionTrainer
{
    public partial class LearningModeWindow : Window
    {
        private Random random = new Random();

        // Эти поля теперь будут хранить информацию о *визуальном представлении* на круге
        // и количестве секторов, которые реально нужно выбрать
        private int sectorsToSelect; // Сколько секторов пользователь должен выбрать на круге
        private int totalSectorsInCircle; // Общее количество секторов на круге

        // Для отображения базовой дроби
        private int baseNumeratorToDisplay;
        private int baseDenominatorToDisplay;


        public LearningModeWindow()
        {
            InitializeComponent();
            GenerateNewLevel();
        }

        private void GenerateNewLevel()
        {
            // 1. Генерируем простую базовую дробь (числитель и знаменатель)
            int baseDen = random.Next(2, 5); // Базовый знаменатель, например, от 2 до 4 (чтобы не слишком сложно)
            int baseNum = random.Next(1, baseDen); // Базовый числитель, меньше знаменателя

            // Сохраняем для отображения
            baseNumeratorToDisplay = baseNum;
            baseDenominatorToDisplay = baseDen;

            // 2. Выбираем множитель для усложнения (чтобы круг имел больше долей)
            // Множитель k=1 означает, что круг будет соответствовать базовой дроби (как раньше)
            // k=2, 3 и т.д. усложняют задачу
            int multiplier = random.Next(1, 4); // Множитель от 1 до 3

            // 3. Рассчитываем параметры для круга
            totalSectorsInCircle = baseDen * multiplier;
            sectorsToSelect = baseNum * multiplier;

            // Обновляем текстовое поле, чтобы показать базовую дробь
            TargetFractionTextBlock.Text = $"{baseNumeratorToDisplay}/{baseDenominatorToDisplay}";

            // Настраиваем FractionCircleControl
            // FractionDisplay.TargetNumerator теперь не так важен, если контрол не использует его для подсказок
            // Главное - правильно установить Denominator для отрисовки круга
            FractionDisplay.TargetNumerator = sectorsToSelect; // Можно оставить для консистентности или если понадобится
            FractionDisplay.Denominator = totalSectorsInCircle;
            FractionDisplay.ResetUserSelectionAndDraw(); // Сбрасываем предыдущий выбор
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] Перед вызовом FractionDisplay.UserSelectedSectorsCount.");
            int userSelection = -1;
            try
            {
                userSelection = FractionDisplay.UserSelectedSectorsCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] ИСКЛЮЧЕНИЕ при вызове UserSelectedSectorsCount: {ex.ToString()}");
            }
            System.Diagnostics.Debug.WriteLine($"[CheckButton_Click] СРАЗУ ПОСЛЕ вызова. userSelection = {userSelection}");

            // Теперь сравниваем выбор пользователя с рассчитанным количеством секторов 'sectorsToSelect'
            if (userSelection == sectorsToSelect)
            {
                MessageBox.Show("Правильно! Следующий уровень.", "Результат");
                GenerateNewLevel();
            }
            else
            {
                // В сообщении об ошибке используем totalSectorsInCircle
                MessageBox.Show($"Неправильно. Вы выбрали {userSelection} из {totalSectorsInCircle}. Попробуйте еще раз! (Нужно было собрать дробь {baseNumeratorToDisplay}/{baseDenominatorToDisplay})", "Результат");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Owner != null)
            {
                this.Owner.Show();
            }
            this.Close();
        }
    }
}