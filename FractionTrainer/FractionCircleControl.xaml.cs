using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Для MouseButtonEventArgs, хотя в последней версии Sector_Clicked его не использует напрямую
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractionTrainer
{
    public partial class FractionCircleControl : UserControl
    {
        // Новые цвета для современного дизайна
        private Brush UnselectedColor = new SolidColorBrush(Color.FromRgb(222, 226, 230)); // Светло-серый
        private Brush SelectedColor = new SolidColorBrush(Color.FromRgb(0, 122, 255));   // Синий, как у кнопок
        private Brush StrokeColor = new SolidColorBrush(Color.FromRgb(108, 117, 125)); // Темно-серый для обводки

        private List<int> selectedSectorIndexes = new List<int>();

        // Denominator Property
        public static readonly DependencyProperty DenominatorProperty =
            DependencyProperty.Register("Denominator", typeof(int), typeof(FractionCircleControl),
            new PropertyMetadata(1, OnFractionPropertiesChanged));

        public int Denominator
        {
            get { return (int)GetValue(DenominatorProperty); }
            set { SetValue(DenominatorProperty, value); }
        }

        // TargetNumerator Property
        public static readonly DependencyProperty TargetNumeratorProperty =
            DependencyProperty.Register("TargetNumerator", typeof(int), typeof(FractionCircleControl),
            new PropertyMetadata(0, OnFractionPropertiesChanged)); // Может вызывать DrawFraction, если нужно реагировать

        public int TargetNumerator
        {
            get { return (int)GetValue(TargetNumeratorProperty); }
            set { SetValue(TargetNumeratorProperty, value); }
        }

        // Свойство для получения количества выбранных пользователем секторов
        public int UserSelectedSectorsCount
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"[FractionCircleControl GETTER] Внутри UserSelectedSectorsCount. selectedSectorIndexes.Count = {selectedSectorIndexes.Count}");
                return selectedSectorIndexes.Count;
            }
        }

        public FractionCircleControl()
        {
            InitializeComponent();
        }

        private static void OnFractionPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FractionCircleControl control = d as FractionCircleControl;
            if (control != null)
            {
                // Если меняется знаменатель, сбрасываем выбор пользователя,
                // так как количество секторов изменилось и предыдущий выбор некорректен.
                if (e.Property == DenominatorProperty)
                {
                    control.selectedSectorIndexes.Clear();
                }
                control.DrawFraction();
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawFraction();
        }

        // Публичный метод для сброса выбора пользователя и перерисовки
        public void ResetUserSelectionAndDraw()
        {
            selectedSectorIndexes.Clear();
            DrawFraction();
        }

        private void DrawFraction()
        {
            DrawingCanvas.Children.Clear();

            if (Denominator <= 0 || DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0)
            {
                // Если нет размеров или знаменатель некорректен, не рисуем
                return;
            }

            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double radius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9; // 0.9 для небольшого отступа
            double angleStep = 360.0 / Denominator;

            for (int i = 0; i < Denominator; i++)
            {
                Path sectorPath = new Path
                {
                    Stroke = StrokeColor,      // Новый цвет обводки
                    StrokeThickness = 1.5      // Немного увеличенная толщина обводки
                    // Fill будет установлен в UpdateSectorColorsFromSelection
                };

                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure
                {
                    StartPoint = center,
                    IsClosed = true // Важно для правильной заливки фигуры
                };

                // Первая линия от центра к краю круга
                LineSegment line1 = new LineSegment(GetCartesianPoint(center, radius, i * angleStep), true);

                // Дуга по краю круга
                ArcSegment arc = new ArcSegment(
                    GetCartesianPoint(center, radius, (i + 1) * angleStep),
                    new Size(radius, radius),
                    angleStep, // Угол дуги
                    angleStep > 180, // IsLargeArc (если угол сектора больше 180 градусов)
                    SweepDirection.Clockwise,
                    true);

                pathFigure.Segments.Add(line1);
                pathFigure.Segments.Add(arc);
                // Вторая линия (от конца дуги к центру) не нужна, так как IsClosed = true

                pathGeometry.Figures.Add(pathFigure);
                sectorPath.Data = pathGeometry;

                int sectorIndex = i; // Захватываем индекс для лямбды
                sectorPath.MouseLeftButtonDown += (s, e_args) => Sector_Clicked(sectorIndex);

                DrawingCanvas.Children.Add(sectorPath);
            }
            UpdateSectorColorsFromSelection(); // Обновляем цвета всех секторов после их создания
        }

        // Обновляет цвета всех секторов на основе списка selectedSectorIndexes
        private void UpdateSectorColorsFromSelection()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++) // Предполагаем, что Children содержит только Path секторов
            {
                if (DrawingCanvas.Children[i] is Path sectorPath)
                {
                    sectorPath.Fill = selectedSectorIndexes.Contains(i) ? SelectedColor : UnselectedColor;
                }
            }
        }

        // Вспомогательный метод для получения координат точки на окружности
        private Point GetCartesianPoint(Point center, double radius, double angleDegrees)
        {
            double angleRadians = (angleDegrees - 90) * (Math.PI / 180.0); // -90 градусов, чтобы 0 градусов был сверху
            return new Point(
                center.X + radius * Math.Cos(angleRadians),
                center.Y + radius * Math.Sin(angleRadians)
            );
        }

        // Обработчик клика по сектору
        private void Sector_Clicked(int sectorIndex)
        {
            if (selectedSectorIndexes.Contains(sectorIndex))
            {
                selectedSectorIndexes.Remove(sectorIndex);
            }
            else
            {
                selectedSectorIndexes.Add(sectorIndex);
            }
            UpdateSectorColorsFromSelection(); // Обновляем визуальное представление
        }
    }
}