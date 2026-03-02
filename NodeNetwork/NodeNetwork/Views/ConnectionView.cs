using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    [TemplateVisualState(Name = HighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = NonHighlightedState, GroupName = HighlightVisualStatesGroup)]
    [TemplateVisualState(Name = ErrorState, GroupName = ErrorVisualStatesGroup)]
    [TemplateVisualState(Name = NonErrorState, GroupName = ErrorVisualStatesGroup)]
    [TemplateVisualState(Name = MarkedForDeleteState, GroupName = MarkedForDeleteVisualStatesGroup)]
    [TemplateVisualState(Name = NotMarkedForDeleteState, GroupName = MarkedForDeleteVisualStatesGroup)]
    public class ConnectionView : Control, IViewFor<ConnectionViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ConnectionViewModel), typeof(ConnectionView), new PropertyMetadata(null));

        public ConnectionViewModel ViewModel
        {
            get => (ConnectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ConnectionViewModel)value;
        }
        #endregion

        #region States
        #region HighlightStates
        public const string HighlightVisualStatesGroup = "HighlightStates";
        public const string HighlightedState = "Highlighted";
        public const string NonHighlightedState = "NonHighlighted";
        #endregion

        #region ErrorStates
        public const string ErrorVisualStatesGroup = "ErrorStates";
        public const string ErrorState = "Error";
        public const string NonErrorState = "NoError";
        #endregion

        #region ErrorStates
        public const string MarkedForDeleteVisualStatesGroup = "MarkedForDeleteStates";
        public const string MarkedForDeleteState = "Marked";
        public const string NotMarkedForDeleteState = "NotMarked";
        #endregion
        #endregion

        #region RegularBrush
        public Brush RegularBrush
        {
            get => (Brush)this.GetValue(RegularBrushProperty);
            set => this.SetValue(RegularBrushProperty, value);
        }
        public static readonly DependencyProperty RegularBrushProperty = DependencyProperty.Register(nameof(RegularBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region ErrorBrush
        public Brush ErrorBrush
        {
            get => (Brush)this.GetValue(ErrorBrushProperty);
            set => this.SetValue(ErrorBrushProperty, value);
        }
        public static readonly DependencyProperty ErrorBrushProperty = DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region HighlightBrush
        public Brush HighlightBrush
        {
            get => (Brush)this.GetValue(HighlightBrushProperty);
            set => this.SetValue(HighlightBrushProperty, value);
        }
        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region MarkedForDeleteBrush
        public Brush MarkedForDeleteBrush
        {
            get => (Brush)this.GetValue(MarkedForDeleteBrushProperty);
            set => this.SetValue(MarkedForDeleteBrushProperty, value);
        }
        public static readonly DependencyProperty MarkedForDeleteBrushProperty =
            DependencyProperty.Register(nameof(MarkedForDeleteBrush), typeof(Brush), typeof(ConnectionView), new PropertyMetadata());
        #endregion

        #region Geometry
        public Geometry Geometry
        {
            get => (Geometry)this.GetValue(GeometryProperty);
            private set => this.SetValue(GeometryProperty, value);
        }
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(nameof(Geometry), typeof(Geometry), typeof(ConnectionView));
        #endregion

        private const double ObstacleMargin = 15;
        private const double SegmentOffset = 30;
        
        public ConnectionView()
        {
            this.DefaultStyleKey = typeof(ConnectionView);
            
            SetupPathData();
            SetupBrushesBinding();
        }

        public override void OnApplyTemplate()
        {
            VisualStateManager.GoToState(this, NonHighlightedState, false);
            VisualStateManager.GoToState(this, NonErrorState, false);
            VisualStateManager.GoToState(this, NotMarkedForDeleteState, false);
        }

        private void SetupPathData()
        {
            this.WhenActivated(d => d(
                this.WhenAny(
                    v => v.ViewModel.Input.Port.CenterPoint, 
                    v => v.ViewModel.Input.PortPosition,
                    v => v.ViewModel.Output.Port.CenterPoint, 
                    v => v.ViewModel.Output.PortPosition,
                    (a, b, c, e) => (a, b, c, e))
                    .Select(_ => BuildPolylineWithAvoidance(
                        ViewModel.Output.Port.CenterPoint, 
                        ViewModel.Output.PortPosition, 
                        ViewModel.Input.Port.CenterPoint,
                        ViewModel.Input.PortPosition,
                        ViewModel.Parent,
                        ViewModel.Output.Parent,
                        ViewModel.Input.Parent))
                    .BindTo(this, v => v.Geometry)
            ));
        }

        private void SetupBrushesBinding()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel.IsHighlighted).Subscribe(isHighlighted =>
                {
                    VisualStateManager.GoToState(this, isHighlighted ? HighlightedState : NonHighlightedState, true);
                }).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.IsInErrorState).Subscribe(isInErrorState =>
                {
                    VisualStateManager.GoToState(this, isInErrorState ? ErrorState : NonErrorState, true);
                }).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.IsMarkedForDelete).Subscribe(isMarkedForDelete =>
                {
                    VisualStateManager.GoToState(this, isMarkedForDelete ? MarkedForDeleteState : NotMarkedForDeleteState, true);
                }).DisposeWith(d);
            });
        }

        public static PathGeometry BuildSmoothBezier(Point startPoint, PortPosition startPosition, Point endPoint, PortPosition endPosition)
        {
            Vector startGradient = ToGradient(startPosition);
            Vector endGradient = ToGradient(endPosition);

            return BuildSmoothBezier(startPoint, startGradient, endPoint, endGradient);
        }

        public static PathGeometry BuildSmoothBezier(Point startPoint, PortPosition startPosition, Point endPoint)
        {
            Vector startGradient = ToGradient(startPosition);
            Vector endGradient = -startGradient;

            return BuildSmoothBezier(startPoint, startGradient, endPoint, endGradient);
        }

        public static PathGeometry BuildSmoothBezier(Point startPoint, Point endPoint, PortPosition endPosition)
        {
            Vector endGradient = ToGradient(endPosition);
            Vector startGradient = -endGradient;

            return BuildSmoothBezier(startPoint, startGradient, endPoint, endGradient);
        }

        private static Vector ToGradient(PortPosition portPosition)
        {
            switch (portPosition)
            {
                case PortPosition.Left:
                    return new Vector(-1, 0);
                case PortPosition.Right:
                    return new Vector(1, 0);
                default:
                    throw new NotImplementedException();
            }
        }

        private const double MinGradient = 10;
        private const double WidthScaling = 5;

        private static PathGeometry BuildSmoothBezier(Point startPoint, Vector startGradient, Point endPoint, Vector endGradient)
        {
            double width = endPoint.X - startPoint.X;

            var gradientScale = Math.Sqrt(Math.Abs(width) * WidthScaling + MinGradient * MinGradient);

            Point startGradientPoint = startPoint + startGradient * gradientScale;
            Point endGradientPoint = endPoint + endGradient * gradientScale;

            Point midPoint = new Point((startGradientPoint.X + endGradientPoint.X) / 2d, (startPoint.Y + endPoint.Y) / 2d);

            PathFigure pathFigure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                Segments =
                {
                    new QuadraticBezierSegment(startGradientPoint, midPoint, true),
                    new QuadraticBezierSegment(endGradientPoint, endPoint, true)
                }
            };

            PathGeometry geom = new PathGeometry();
            geom.Figures.Add(pathFigure);

            return geom;
        }

        public static PathGeometry BuildPolylineWithAvoidance(Point startPoint, PortPosition startPosition, 
            Point endPoint, PortPosition endPosition, NetworkViewModel network, 
            NodeViewModel sourceNode, NodeViewModel targetNode)
        {
            var points = CalculateAvoidancePath(startPoint, startPosition, endPoint, endPosition, network, sourceNode, targetNode);
            
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false
            };

            for (int i = 1; i < points.Count; i++)
            {
                pathFigure.Segments.Add(new LineSegment(points[i], true));
            }

            PathGeometry geom = new PathGeometry();
            geom.Figures.Add(pathFigure);
            return geom;
        }

        private static List<Point> CalculateAvoidancePath(Point start, PortPosition startPos, 
            Point end, PortPosition endPos, NetworkViewModel network, 
            NodeViewModel sourceNode, NodeViewModel targetNode)
        {
            var points = new List<Point> { start };
            
            if (network == null)
            {
                AddSimplePolyline(points, start, startPos, end, endPos);
                return points;
            }

            var obstacles = GetObstacles(network, start, end, sourceNode, targetNode, startPos, endPos);
            
            bool startOnRight = startPos == PortPosition.Right;
            bool endOnLeft = endPos == PortPosition.Left;

            double directPathY = start.Y;
            
            bool needsAvoidance = obstacles.Any(rect => 
                LineIntersectsRectVertical(start, end, rect, startOnRight));
            
            if (!needsAvoidance)
            {
                AddSimplePolyline(points, start, startPos, end, endPos);
                return points;
            }

            var path = FindPathAroundObstacles(start, startPos, end, endPos, obstacles);
            points.AddRange(path);
            
            return points;
        }

        private static List<Rect> GetObstacles(NetworkViewModel network, Point start, Point end, 
            NodeViewModel sourceNode, NodeViewModel targetNode,
            PortPosition startPos, PortPosition endPos)
        {
            var obstacles = new List<Rect>();
            
            foreach (var node in network.Nodes.Items)
            {
                var nodeRect = new Rect(node.Position, node.Size);
                nodeRect.Inflate(ObstacleMargin, ObstacleMargin);
                
                if (node == sourceNode)
                {
                    if (startPos == PortPosition.Right && end.X >= start.X)
                    {
                        continue;
                    }
                    if (startPos == PortPosition.Left && end.X <= start.X)
                    {
                        continue;
                    }
                }
                
                if (node == targetNode)
                {
                    if (endPos == PortPosition.Left && start.X <= end.X)
                    {
                        continue;
                    }
                    if (endPos == PortPosition.Right && start.X >= end.X)
                    {
                        continue;
                    }
                }
                
                obstacles.Add(nodeRect);
            }
            
            return obstacles;
        }

        private static bool LineIntersectsRectVertical(Point start, Point end, Rect rect, bool startOnRight)
        {
            double minX = Math.Min(start.X, end.X);
            double maxX = Math.Max(start.X, end.X);
            double minY = Math.Min(start.Y, end.Y);
            double maxY = Math.Max(start.Y, end.Y);

            if (maxX < rect.Left || minX > rect.Right)
                return false;
            
            if (maxY < rect.Top || minY > rect.Bottom)
                return false;

            return true;
        }

        private static void AddSimplePolyline(List<Point> points, Point start, PortPosition startPos, 
            Point end, PortPosition endPos)
        {
            double midX;
            
            if (startPos == PortPosition.Right && endPos == PortPosition.Left)
            {
                midX = (start.X + end.X) / 2;
                if (Math.Abs(start.X - end.X) < SegmentOffset * 2)
                {
                    midX = start.X + SegmentOffset;
                }
            }
            else if (startPos == PortPosition.Right)
            {
                midX = start.X + SegmentOffset;
            }
            else
            {
                midX = start.X - SegmentOffset;
            }

            points.Add(new Point(midX, start.Y));
            points.Add(new Point(midX, end.Y));
            points.Add(end);
        }

        private static List<Point> FindPathAroundObstacles(Point start, PortPosition startPos, 
            Point end, PortPosition endPos, List<Rect> obstacles)
        {
            var path = new List<Point>();
            
            double startDir = startPos == PortPosition.Right ? 1 : -1;
            double endDir = endPos == PortPosition.Left ? -1 : 1;
            
            double firstBend = start.X + startDir * SegmentOffset;
            double lastBend = end.X + endDir * SegmentOffset;

            var candidateYs = new List<double> { start.Y, end.Y };
            
            foreach (var rect in obstacles)
            {
                candidateYs.Add(rect.Top - ObstacleMargin);
                candidateYs.Add(rect.Bottom + ObstacleMargin);
            }
            
            candidateYs.Sort();

            double? bestY = null;
            double bestLength = double.MaxValue;

            foreach (double y in candidateYs)
            {
                bool valid = true;
                double testFirstBend = start.X + startDir * SegmentOffset;
                double testLastBend = end.X + endDir * SegmentOffset;
                
                foreach (var rect in obstacles)
                {
                    if (SegmentIntersectsRect(testFirstBend, y, testLastBend, y, rect))
                    {
                        valid = false;
                        break;
                    }
                    
                    if (SegmentIntersectsRect(start.X, start.Y, testFirstBend, y, rect))
                    {
                        valid = false;
                        break;
                    }
                    
                    if (SegmentIntersectsRect(testLastBend, y, end.X, end.Y, rect))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    double length = Math.Abs(start.X - testFirstBend) + 
                                   Math.Abs(start.Y - y) + 
                                   Math.Abs(testFirstBend - testLastBend) + 
                                   Math.Abs(y - end.Y) + 
                                   Math.Abs(testLastBend - end.X);
                    
                    if (length < bestLength)
                    {
                        bestLength = length;
                        bestY = y;
                    }
                }
            }

            if (bestY.HasValue)
            {
                path.Add(new Point(firstBend, start.Y));
                path.Add(new Point(firstBend, bestY.Value));
                path.Add(new Point(lastBend, bestY.Value));
                path.Add(new Point(lastBend, end.Y));
                path.Add(end);
            }
            else
            {
                double escapeY = FindEscapeY(start, end, obstacles);
                
                path.Add(new Point(firstBend, start.Y));
                path.Add(new Point(firstBend, escapeY));
                path.Add(new Point(lastBend, escapeY));
                path.Add(new Point(lastBend, end.Y));
                path.Add(end);
            }

            return path;
        }

        private static bool SegmentIntersectsRect(double x1, double y1, double x2, double y2, Rect rect)
        {
            double minX = Math.Min(x1, x2);
            double maxX = Math.Max(x1, x2);
            double minY = Math.Min(y1, y2);
            double maxY = Math.Max(y1, y2);

            if (maxX < rect.Left || minX > rect.Right)
                return false;
            if (maxY < rect.Top || minY > rect.Bottom)
                return false;

            return true;
        }

        private static double FindEscapeY(Point start, Point end, List<Rect> obstacles)
        {
            double minY = Math.Min(start.Y, end.Y);
            double maxY = Math.Max(start.Y, end.Y);
            
            foreach (var rect in obstacles)
            {
                minY = Math.Min(minY, rect.Top);
                maxY = Math.Max(maxY, rect.Bottom);
            }

            double escapeTop = minY - SegmentOffset * 3;
            double escapeBottom = maxY + SegmentOffset * 3;

            double midY = (start.Y + end.Y) / 2;
            
            if (Math.Abs(escapeTop - midY) < Math.Abs(escapeBottom - midY))
            {
                return escapeTop;
            }
            return escapeBottom;
        }
    }
}
