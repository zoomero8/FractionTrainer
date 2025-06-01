using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractionTrainer
{
    public enum ShapeType
    {
        Circle,
        Triangle
    }

    public partial class FractionShapeVisualizer : UserControl
    {
        // Цвета
        private Brush UnselectedColor = new SolidColorBrush(Color.FromRgb(222, 226, 230));
        private Brush SelectedColor = new SolidColorBrush(Color.FromRgb(0, 122, 255));
        private Brush StrokeColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));

        private List<int> selectedSectorIndexes = new List<int>();

        // ShapeType Property
        public static readonly DependencyProperty CurrentShapeTypeProperty =
            DependencyProperty.Register("CurrentShapeType", typeof(ShapeType), typeof(FractionShapeVisualizer),
            new PropertyMetadata(ShapeType.Circle, OnVisualPropertiesChanged));

        public ShapeType CurrentShapeType
        {
            get { return (ShapeType)GetValue(CurrentShapeTypeProperty); }
            set { SetValue(CurrentShapeTypeProperty, value); }
        }

        // Denominator Property
        public static readonly DependencyProperty DenominatorProperty =
            DependencyProperty.Register("Denominator", typeof(int), typeof(FractionShapeVisualizer),
            new PropertyMetadata(1, OnVisualPropertiesChanged));

        public int Denominator
        {
            get { return (int)GetValue(DenominatorProperty); }
            set { SetValue(DenominatorProperty, value); }
        }

        // TargetNumerator Property
        public static readonly DependencyProperty TargetNumeratorProperty =
            DependencyProperty.Register("TargetNumerator", typeof(int), typeof(FractionShapeVisualizer),
            new PropertyMetadata(0, OnVisualPropertiesChanged));

        public int TargetNumerator // Используется для информации, не для прямого рисования выбора
        {
            get { return (int)GetValue(TargetNumeratorProperty); }
            set { SetValue(TargetNumeratorProperty, value); }
        }

        public int UserSelectedSectorsCount
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"[FractionShapeVisualizer GETTER] Внутри UserSelectedSectorsCount. selectedSectorIndexes.Count = {selectedSectorIndexes.Count}");
                return selectedSectorIndexes.Count;
            }
        }

        public FractionShapeVisualizer()
        {
            InitializeComponent(); // Убедитесь, что эта строка не вызывает ошибок
        }

        private static void OnVisualPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FractionShapeVisualizer control = d as FractionShapeVisualizer;
            if (control != null)
            {
                // Сброс выбора, если меняется знаменатель или тип фигуры, так как старый выбор некорректен
                if (e.Property == DenominatorProperty || e.Property == CurrentShapeTypeProperty)
                {
                    control.selectedSectorIndexes.Clear();
                }
                control.DrawShape();
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawShape();
        }

        public void ResetUserSelectionAndDraw()
        {
            selectedSectorIndexes.Clear();
            DrawShape();
        }

        private void DrawShape()
        {
            DrawingCanvas.Children.Clear();

            if (DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0) return;
            if (Denominator <= 0 && CurrentShapeType == ShapeType.Circle) return; // Для круга знаменатель важен
            if (CurrentShapeType == ShapeType.Triangle && Denominator != 3)
            {
                // Для треугольника мы ожидаем Denominator = 3. Если это не так, можно не рисовать или выдать предупреждение.
                // Или можно принудительно установить Denominator = 3 если CurrentShapeType == ShapeType.Triangle
                // в обработчике OnVisualPropertiesChanged или здесь.
                // Пока что, если Denominator не 3 для треугольника, он не будет корректно нарисован текущей логикой.
                System.Diagnostics.Debug.WriteLineIf(Denominator != 3, "[DrawShape] Triangle expects 3 sectors (Denominator=3). Current Denominator: " + Denominator);
                // Не будем рисовать, если Denominator для треугольника не 3, чтобы избежать ошибок в DrawTriangleSectors
                if (CurrentShapeType == ShapeType.Triangle && Denominator != 3) return;
            }


            switch (CurrentShapeType)
            {
                case ShapeType.Circle:
                    DrawCircleSectors();
                    break;
                case ShapeType.Triangle:
                    DrawTriangleSectors();
                    break;
            }
            UpdateSectorColorsFromSelection();
        }

        private void DrawCircleSectors()
        {
            // Эта проверка уже есть в DrawShape, но дублирование не повредит
            if (Denominator <= 0 || DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0) return;

            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double radius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9;
            double angleStep = 360.0 / Denominator;

            for (int i = 0; i < Denominator; i++)
            {
                Path sectorPath = new Path { Stroke = StrokeColor, StrokeThickness = 1.5 };
                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure { StartPoint = center, IsClosed = true };
                LineSegment line1 = new LineSegment(GetCartesianCoordinate(center, radius, i * angleStep), true);
                ArcSegment arc = new ArcSegment(
                    GetCartesianCoordinate(center, radius, (i + 1) * angleStep),
                    new Size(radius, radius), angleStep, angleStep > 180,
                    SweepDirection.Clockwise, true);
                pathFigure.Segments.Add(line1);
                pathFigure.Segments.Add(arc);
                pathGeometry.Figures.Add(pathFigure);
                sectorPath.Data = pathGeometry;
                int sectorIndex = i;
                sectorPath.MouseLeftButtonDown += (s, e_args) => Sector_Clicked(sectorIndex);
                DrawingCanvas.Children.Add(sectorPath);
            }
        }

        private void DrawTriangleSectors()
        {
            // Ожидается, что Denominator == 3
            if (Denominator != 3 || DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0) return;


            double width = DrawingCanvas.ActualWidth;
            double height = DrawingCanvas.ActualHeight;
            double padding = Math.Min(width, height) * 0.1; // Отступ от краев

            // Вершины равностороннего треугольника, вписанного в уменьшенную область
            Point p1 = new Point(width / 2, padding); // Верхняя
            Point p2 = new Point(padding, height - padding); // Левая нижняя
            Point p3 = new Point(width - padding, height - padding); // Правая нижняя

            // Центроид
            Point centroid = new Point((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3);

            // Создаем 3 сектора (треугольника)
            Path sectorA = CreateTriangleSectorPath(centroid, p1, p2); // Сектор 0
            sectorA.MouseLeftButtonDown += (s, e) => Sector_Clicked(0);
            DrawingCanvas.Children.Add(sectorA);

            Path sectorB = CreateTriangleSectorPath(centroid, p2, p3); // Сектор 1
            sectorB.MouseLeftButtonDown += (s, e) => Sector_Clicked(1);
            DrawingCanvas.Children.Add(sectorB);

            Path sectorC = CreateTriangleSectorPath(centroid, p3, p1); // Сектор 2
            sectorC.MouseLeftButtonDown += (s, e) => Sector_Clicked(2);
            DrawingCanvas.Children.Add(sectorC);
        }

        private Path CreateTriangleSectorPath(Point c, Point v1, Point v2)
        {
            Path sectorPath = new Path { Stroke = StrokeColor, StrokeThickness = 1.5 };
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = c, IsClosed = true };
            figure.Segments.Add(new LineSegment(v1, true));
            figure.Segments.Add(new LineSegment(v2, true));
            geometry.Figures.Add(figure);
            sectorPath.Data = geometry;
            return sectorPath;
        }

        private void UpdateSectorColorsFromSelection()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
            {
                if (DrawingCanvas.Children[i] is Path sectorPath)
                {
                    // i здесь - это индекс добавления в DrawingCanvas.Children,
                    // который должен соответствовать sectorIndex (0, 1, 2 для треугольника; 0..N-1 для круга)
                    sectorPath.Fill = selectedSectorIndexes.Contains(i) ? SelectedColor : UnselectedColor;
                }
            }
        }

        private Point GetCartesianCoordinate(Point center, double radius, double angleDegrees)
        {
            double angleRadians = (angleDegrees - 90) * (Math.PI / 180.0);
            return new Point(
                center.X + radius * Math.Cos(angleRadians),
                center.Y + radius * Math.Sin(angleRadians)
            );
        }

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
            UpdateSectorColorsFromSelection();
        }
    }
}