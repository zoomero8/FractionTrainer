using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractionTrainer
{
    /// <summary>
    /// Перечисление для типов фигур, которые может отображать визуализатор.
    /// </summary>
    public enum ShapeType
    {
        Circle,
        Triangle,
        Octagon,
        Diamond
    }

    /// <summary>
    /// Пользовательский элемент управления для визуального представления дробей в виде различных фигур,
    /// разделенных на сектора. Поддерживает два режима: интерактивный (пользователь выбирает сектора)
    /// и режим отображения (показывает предустановленную дробь).
    /// </summary>
    public partial class FractionShapeVisualizer : UserControl
    {
        // --- Поля и Свойства ---
        private readonly Brush UnselectedColor = new SolidColorBrush(Color.FromRgb(222, 226, 230));
        private readonly Brush SelectedColor = new SolidColorBrush(Color.FromRgb(0, 122, 255));
        private readonly Brush StrokeColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));
        private readonly List<int> selectedSectorIndexes = new List<int>();

        public static readonly DependencyProperty IsInteractionEnabledProperty = DependencyProperty.Register("IsInteractionEnabled", typeof(bool), typeof(FractionShapeVisualizer), new PropertyMetadata(true, OnVisualPropertiesChanged));
        public bool IsInteractionEnabled { get => (bool)GetValue(IsInteractionEnabledProperty); set => SetValue(IsInteractionEnabledProperty, value); }

        public static readonly DependencyProperty CurrentShapeTypeProperty = DependencyProperty.Register("CurrentShapeType", typeof(ShapeType), typeof(FractionShapeVisualizer), new PropertyMetadata(ShapeType.Circle, OnVisualPropertiesChanged));
        public ShapeType CurrentShapeType { get => (ShapeType)GetValue(CurrentShapeTypeProperty); set => SetValue(CurrentShapeTypeProperty, value); }

        public static readonly DependencyProperty DenominatorProperty = DependencyProperty.Register("Denominator", typeof(int), typeof(FractionShapeVisualizer), new PropertyMetadata(1, OnVisualPropertiesChanged));
        public int Denominator { get => (int)GetValue(DenominatorProperty); set => SetValue(DenominatorProperty, value); }

        public static readonly DependencyProperty TargetNumeratorProperty = DependencyProperty.Register("TargetNumerator", typeof(int), typeof(FractionShapeVisualizer), new PropertyMetadata(0, OnVisualPropertiesChanged));
        public int TargetNumerator { get => (int)GetValue(TargetNumeratorProperty); set => SetValue(TargetNumeratorProperty, value); }

        public int UserSelectedSectorsCount => selectedSectorIndexes.Count;

        // --- Конструктор и обработчики ---
        public FractionShapeVisualizer()
        {
            InitializeComponent();
        }

        private static void OnVisualPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FractionShapeVisualizer control)
            {
                // При любом изменении основного свойства сбрасываем пользовательские клики и перерисовываем
                control.selectedSectorIndexes.Clear();
                control.DrawShape();
            }
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawShape();

        public void ResetUserSelectionAndDraw()
        {
            selectedSectorIndexes.Clear();
            DrawShape();
        }

        // --- Главный метод отрисовки ---
        private void DrawShape()
        {
            DrawingCanvas.Children.Clear();
            if (DrawingCanvas.ActualWidth == 0 || DrawingCanvas.ActualHeight == 0) return;

            if (Denominator == 1 && IsInteractionEnabled)
            {
                DrawUndividedShapeOutline();
                return;
            }

            if (!CheckIfCanDraw())
            {
                System.Diagnostics.Debug.WriteLine($"[DrawShape] Невозможно нарисовать {CurrentShapeType} со знаменателем {Denominator}.");
                return;
            }

            switch (CurrentShapeType)
            {
                case ShapeType.Circle: DrawCircleSectors(); break;
                case ShapeType.Triangle: DrawTriangleSectors(); break;
                case ShapeType.Octagon: DrawOctagonSectors(); break;
                case ShapeType.Diamond: DrawDiamondSectors(); break;
            }

            // Закраска происходит в зависимости от режима
            if (!IsInteractionEnabled)
                UpdateSectorColorsFromTargetNumerator();
            else
                UpdateSectorColorsFromUserSelection();
        }

        private bool CheckIfCanDraw()
        {
            switch (CurrentShapeType)
            {
                case ShapeType.Circle: return Denominator > 0;
                case ShapeType.Triangle: return Denominator == 3;
                case ShapeType.Octagon: return Denominator == 8;
                case ShapeType.Diamond: return Denominator == 4;
                default: return false;
            }
        }

        // --- Методы создания геометрии секторов ---

        private void DrawUndividedShapeOutline()
        {
            Path outlinePath = new Path { Stroke = StrokeColor, StrokeThickness = 1.5, Fill = UnselectedColor };
            double width = DrawingCanvas.ActualWidth;
            double height = DrawingCanvas.ActualHeight;
            Point center = new Point(width / 2, height / 2);
            double radius = Math.Min(width, height) / 2 * 0.9;
            double padding = Math.Min(width, height) * 0.1;

            switch (CurrentShapeType)
            {
                case ShapeType.Circle:
                    outlinePath.Data = new EllipseGeometry(center, radius, radius);
                    break;
                case ShapeType.Triangle:
                    Point p1 = new Point(width / 2, padding);
                    Point p2 = new Point(padding, height - padding);
                    Point p3 = new Point(width - padding, height - padding);
                    PathGeometry pgT = new PathGeometry(new PathFigure[] { new PathFigure(p1, new PathSegment[] { new LineSegment(p2, true), new LineSegment(p3, true) }, true) });
                    outlinePath.Data = pgT;
                    break;
                case ShapeType.Octagon:
                    PathGeometry pgO = new PathGeometry();
                    PathFigure pfO = new PathFigure { IsClosed = true };
                    double angleStep = 360.0 / 8.0;
                    pfO.StartPoint = GetCartesianCoordinate(center, radius, 0);
                    for (int i = 1; i < 8; i++) { pfO.Segments.Add(new LineSegment(GetCartesianCoordinate(center, radius, i * angleStep), true)); }
                    pgO.Figures.Add(pfO);
                    outlinePath.Data = pgO;
                    break;
                case ShapeType.Diamond:
                    PathGeometry pgD = new PathGeometry();
                    PathFigure pfD = new PathFigure { IsClosed = true };
                    pfD.StartPoint = new Point(center.X, center.Y - radius);
                    pfD.Segments.Add(new LineSegment(new Point(center.X + radius, center.Y), true));
                    pfD.Segments.Add(new LineSegment(new Point(center.X, center.Y + radius), true));
                    pfD.Segments.Add(new LineSegment(new Point(center.X - radius, center.Y), true));
                    pgD.Figures.Add(pfD);
                    outlinePath.Data = pgD;
                    break;
            }
            DrawingCanvas.Children.Add(outlinePath);
        }

        private void DrawCircleSectors()
        {
            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double radius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9;
            double angleStep = 360.0 / Denominator;

            for (int i = 0; i < Denominator; i++)
            {
                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure(center, new PathSegment[] {
                    new LineSegment(GetCartesianCoordinate(center, radius, i * angleStep), true),
                    new ArcSegment(GetCartesianCoordinate(center, radius, (i + 1) * angleStep), new Size(radius, radius), angleStep, angleStep > 180, SweepDirection.Clockwise, true)
                }, true);
                geometry.Figures.Add(figure);

                Path sectorPath = new Path { Data = geometry, Stroke = StrokeColor, StrokeThickness = 1.5 };
                AddClickableSector(sectorPath, i);
            }
        }

        private void DrawTriangleSectors()
        {
            double w = DrawingCanvas.ActualWidth, h = DrawingCanvas.ActualHeight, p = Math.Min(w, h) * 0.1;
            Point p1 = new Point(w / 2, p), p2 = new Point(p, h - p), p3 = new Point(w - p, h - p);
            Point centroid = new Point((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3);

            AddClickableSector(CreateTriangleSectorPath(centroid, p1, p2), 0);
            AddClickableSector(CreateTriangleSectorPath(centroid, p2, p3), 1);
            AddClickableSector(CreateTriangleSectorPath(centroid, p3, p1), 2);
        }

        private void DrawOctagonSectors()
        {
            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double radius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9;
            double angleStep = 360.0 / 8.0;
            Point[] vertices = new Point[8];
            for (int i = 0; i < 8; i++) { vertices[i] = GetCartesianCoordinate(center, radius, i * angleStep); }

            for (int i = 0; i < 8; i++)
                AddClickableSector(CreateTriangleSectorPath(center, vertices[i], vertices[(i + 1) % 8]), i);
        }

        private void DrawDiamondSectors()
        {
            Point center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
            double outerRadius = Math.Min(DrawingCanvas.ActualWidth, DrawingCanvas.ActualHeight) / 2 * 0.9;
            Point[] vertices = {
                new Point(center.X, center.Y - outerRadius),
                new Point(center.X + outerRadius, center.Y),
                new Point(center.X, center.Y + outerRadius),
                new Point(center.X - outerRadius, center.Y)
            };
            for (int i = 0; i < 4; i++)
                AddClickableSector(CreateTriangleSectorPath(center, vertices[i], vertices[(i + 1) % 4]), i);
        }

        // --- Методы закраски и вспомогательные методы ---

        private void UpdateSectorColorsFromTargetNumerator()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
                if (DrawingCanvas.Children[i] is Path path) path.Fill = (i < TargetNumerator) ? SelectedColor : UnselectedColor;
        }

        private void UpdateSectorColorsFromUserSelection()
        {
            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
                if (DrawingCanvas.Children[i] is Path path) path.Fill = selectedSectorIndexes.Contains(i) ? SelectedColor : UnselectedColor;
        }

        private void Sector_Clicked(int sectorIndex)
        {
            // Эта строка - ключ к диагностике. Если она появляется, событие работает.
            System.Diagnostics.Debug.WriteLine($"КЛИК по сектору {sectorIndex}! Интерактивность: {IsInteractionEnabled}");

            if (!IsInteractionEnabled) return;

            if (selectedSectorIndexes.Contains(sectorIndex))
                selectedSectorIndexes.Remove(sectorIndex);
            else
                selectedSectorIndexes.Add(sectorIndex);

            UpdateSectorColorsFromUserSelection();
        }

        // --- Вспомогательные методы для рисования ---

        private void AddClickableSector(Path sectorPath, int index)
        {
            sectorPath.MouseLeftButtonDown += (s, e) => Sector_Clicked(index);
            DrawingCanvas.Children.Add(sectorPath);
        }

        private Path CreateTriangleSectorPath(Point p1, Point p2, Point p3)
        {
            return new Path
            {
                Stroke = StrokeColor,
                StrokeThickness = 1.5,
                Data = new PathGeometry(new PathFigure[] { new PathFigure(p1, new PathSegment[] { new LineSegment(p2, true), new LineSegment(p3, true) }, true) })
            };
        }

        private Point GetCartesianCoordinate(Point center, double radius, double angleDegrees)
        {
            double angleRadians = (angleDegrees - 90) * (Math.PI / 180.0);
            return new Point(center.X + radius * Math.Cos(angleRadians), center.Y + radius * Math.Sin(angleRadians));
        }
    }
}
