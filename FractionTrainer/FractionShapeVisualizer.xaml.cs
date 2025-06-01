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
        Triangle,
        Octagon,
        Diamond
    }

    public partial class FractionShapeVisualizer : UserControl
    {
        // Цвета
        private Brush UnselectedColor = new SolidColorBrush(Color.FromRgb(222, 226, 230));
        private Brush SelectedColor = new SolidColorBrush(Color.FromRgb(0, 122, 255));
        private Brush StrokeColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));

        private List<int> selectedSectorIndexes = new List<int>();

        // НОВОЕ СВОЙСТВО ЗАВИСИМОСТИ для включения/отключения интерактивности
        public static readonly DependencyProperty IsInteractionEnabledProperty =
            DependencyProperty.Register("IsInteractionEnabled", typeof(bool), typeof(FractionShapeVisualizer),
            new PropertyMetadata(true)); // По умолчанию интерактивность включена

        public bool IsInteractionEnabled
        {
            get { return (bool)GetValue(IsInteractionEnabledProperty); }
            set { SetValue(IsInteractionEnabledProperty, value); }
        }

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

        public int TargetNumerator
        {
            get { return (int)GetValue(TargetNumeratorProperty); }
            set { SetValue(TargetNumeratorProperty, value); }
        }

        public int UserSelectedSectorsCount // Используется для режима, где пользователь КЛИКАЕТ по секторам
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"[FractionShapeVisualizer GETTER] Внутри UserSelectedSectorsCount. selectedSectorIndexes.Count = {selectedSectorIndexes.Count}");
                return selectedSectorIndexes.Count;
            }
        }

        public FractionShapeVisualizer()
        {
            InitializeComponent();
        }

        private static void OnVisualPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FractionShapeVisualizer control = d as FractionShapeVisualizer;
            if (control != null)
            {
                // Сброс пользовательского выбора, если меняется знаменатель, тип фигуры или целевой числитель (для предустановленной закраски)
                if (e.Property == DenominatorProperty ||
                    e.Property == CurrentShapeTypeProperty ||
                    e.Property == TargetNumeratorProperty) // Добавлена проверка на TargetNumerator
                {
                    control.selectedSectorIndexes.Clear(); // Очищаем клики пользователя
                }
                control.DrawShape(); // Перерисовываем фигуру
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawShape();
        }

        public void ResetUserSelectionAndDraw()
        {
            selectedSectorIndexes.Clear(); // Сбрасываем клики пользователя
            DrawShape(); // Перерисовываем (фигура будет закрашена согласно TargetNumerator)
        }

        private void DrawShape()
        {
            DrawingCanvas.Children.Clear();

            if (DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0) return;

            bool canDraw = true;
            switch (CurrentShapeType)
            {
                case ShapeType.Circle: if (Denominator <= 0) canDraw = false; break;
                case ShapeType.Triangle: if (Denominator != 3) canDraw = false; break;
                case ShapeType.Octagon: if (Denominator != 8) canDraw = false; break;
                case ShapeType.Diamond: if (Denominator != 4) canDraw = false; break;
            }

            if (!canDraw)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawShape] Cannot draw {CurrentShapeType} with Denominator {Denominator}.");
                return;
            }

            // Создаем контуры секторов
            switch (CurrentShapeType)
            {
                case ShapeType.Circle: DrawCircleSectors(); break;
                case ShapeType.Triangle: DrawTriangleSectors(); break;
                case ShapeType.Octagon: DrawOctagonSectors(); break;
                case ShapeType.Diamond: DrawDiamondSectors(); break;
            }

            // ИЗМЕНЕННАЯ ЛОГИКА ЗАЛИВКИ:
            if (!IsInteractionEnabled) // Если интерактивность ВЫКЛЮЧЕНА (например, в режиме выбора вариантов)
            {
                // Закрашиваем сектора на основе TargetNumerator (предустановленная дробь)
                UpdateSectorColorsFromTargetNumerator();
            }
            else // Если интерактивность ВКЛЮЧЕНА (например, в режиме обучения)
            {
                // Закрашиваем сектора на основе того, что выбрал пользователь (selectedSectorIndexes)
                // Это важно, чтобы при изменении размера окна или другой перерисовке
                // не сбрасывался выбор пользователя в режиме обучения.
                UpdateSectorColorsFromUserSelection();
            }
        }

        private void DrawCircleSectors()
        {
            if (Denominator <= 0) return; // Дополнительная проверка
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
            if (Denominator != 3) return; // Дополнительная проверка
            double width = DrawingCanvas.ActualWidth;
            double height = DrawingCanvas.ActualHeight;
            double padding = Math.Min(width, height) * 0.1;
            Point p1 = new Point(width / 2, padding);
            Point p2 = new Point(padding, height - padding);
            Point p3 = new Point(width - padding, height - padding);
            Point centroid = new Point((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3);

            Path sectorA = CreateTriangleSectorPath(centroid, p1, p2);
            sectorA.MouseLeftButtonDown += (s, e) => Sector_Clicked(0);
            DrawingCanvas.Children.Add(sectorA);

            Path sectorB = CreateTriangleSectorPath(centroid, p2, p3);
            sectorB.MouseLeftButtonDown += (s, e) => Sector_Clicked(1);
            DrawingCanvas.Children.Add(sectorB);

            Path sectorC = CreateTriangleSectorPath(centroid, p3, p1);
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

        private void DrawOctagonSectors()
        {
            if (Denominator != 8) return; // Дополнительная проверка
            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double radius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9;
            double angleStep = 360.0 / 8.0;
            Point[] vertices = new Point[8];
            for (int i = 0; i < 8; i++)
            {
                vertices[i] = GetCartesianCoordinate(center, radius, i * angleStep);
            }
            for (int i = 0; i < 8; i++)
            {
                Point p1 = vertices[i];
                Point p2 = vertices[(i + 1) % 8];
                Path sectorPath = CreateTriangleSectorPath(center, p1, p2); // Используем общий метод
                int sectorIndex = i;
                sectorPath.MouseLeftButtonDown += (s, e_args) => Sector_Clicked(sectorIndex);
                DrawingCanvas.Children.Add(sectorPath);
            }
        }

        private void DrawDiamondSectors()
        {
            if (Denominator != 4) return; // Дополнительная проверка
            double width = DrawingCanvas.ActualWidth;
            double height = DrawingCanvas.ActualHeight;
            Point center = new Point(width / 2, height / 2);
            double outerRadius = Math.Min(width, height) / 2 * 0.9;
            Point topVertex = new Point(center.X, center.Y - outerRadius);
            Point rightVertex = new Point(center.X + outerRadius, center.Y);
            Point bottomVertex = new Point(center.X, center.Y + outerRadius);
            Point leftVertex = new Point(center.X - outerRadius, center.Y);
            Point[] vertices = { topVertex, rightVertex, bottomVertex, leftVertex };
            for (int i = 0; i < 4; i++)
            {
                Point p1 = vertices[i];
                Point p2 = vertices[(i + 1) % 4];
                Path sectorPath = CreateTriangleSectorPath(center, p1, p2); // Используем общий метод
                int sectorIndex = i;
                sectorPath.MouseLeftButtonDown += (s, e_args) => Sector_Clicked(sectorIndex);
                DrawingCanvas.Children.Add(sectorPath);
            }
        }

        // НОВЫЙ МЕТОД для закраски секторов на основе TargetNumerator
        private void UpdateSectorColorsFromTargetNumerator()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
            {
                if (DrawingCanvas.Children[i] is Path sectorPath)
                {
                    // Закрашиваем, если индекс сектора меньше TargetNumerator
                    sectorPath.Fill = (i < this.TargetNumerator) ? SelectedColor : UnselectedColor;
                }
            }
        }

        // Этот метод теперь будет использоваться ТОЛЬКО если IsInteractionEnabled = true
        private void UpdateSectorColorsFromUserSelection()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
            {
                if (DrawingCanvas.Children[i] is Path sectorPath)
                {
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
            if (!IsInteractionEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[FractionShapeVisualizer] Sector_Clicked ({sectorIndex}), но IsInteractionEnabled = false.");
                return; // Ничего не делаем, если интерактивность отключена
            }
            System.Diagnostics.Debug.WriteLine($"[FractionShapeVisualizer] Sector_Clicked ({sectorIndex}), IsInteractionEnabled = true.");

            if (selectedSectorIndexes.Contains(sectorIndex))
            {
                selectedSectorIndexes.Remove(sectorIndex);
            }
            else
            {
                selectedSectorIndexes.Add(sectorIndex);
            }
            UpdateSectorColorsFromUserSelection(); // Обновляем цвета на основе пользовательского выбора
        }
    }
}
