using System;
using System.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
  using System.Globalization;
  using System.Reflection;

  using WpfApplication1.Properties;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      //FormattedText formattedText = new FormattedText(
      //  "gomi",
      //  CultureInfo.GetCultureInfo("en-us"),
      //  FlowDirection.LeftToRight,
      //  new Typeface("Verdana"),
      //  (300.0),
      //  Brushes.Black);

      var tf = FontFamily.GetTypefaces().ToArray();
      var tfn = tf.First(x => x.Style == FontStyles.Normal && x.Weight == FontWeights.Normal);
      var tfn2 = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

      FormattedText formattedText = new FormattedText(
        "gomi",
        Thread.CurrentThread.CurrentCulture,
        FlowDirection.LeftToRight,
        tfn2,
        (300.0),
        Brushes.Black);

      var myPathGeometry = formattedText.BuildGeometry(new Point(0, 0));
      myPath.Data = myPathGeometry;

      var list = new List<string>();
      var sb = new StringBuilder();
      WalkGeometryChildren(list, myPathGeometry);

      var s = sb.ToString();
      myPath.Data = Geometry.Parse(string.Join(" ", list));
    }

    private void myPath_Loaded(object sender, RoutedEventArgs e)
    {
      MyRect.Fill = new DrawingBrush(DrawMyText("My Custom Label"));

    }
    // Convert the text string to a geometry and draw it to the control's DrawingContext.
    private Drawing DrawMyText(string textString)
    {
      // Create a new DrawingGroup of the control.
      DrawingGroup drawingGroup = new DrawingGroup();

      // Open the DrawingGroup in order to access the DrawingContext.
      using (DrawingContext drawingContext = drawingGroup.Open())
      {
        // Create the formatted text based on the properties set.
        FormattedText formattedText = new FormattedText(
          textString,
          CultureInfo.GetCultureInfo("en-us"),
          FlowDirection.LeftToRight,
          new Typeface("Comic Sans MS Bold"),
          48,
          System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
        );

        // Build the geometry object that represents the text.
        Geometry textGeometry = formattedText.BuildGeometry(new System.Windows.Point(20, 0));

        // Draw a rounded rectangle under the text that is slightly larger than the text.
        drawingContext.DrawRoundedRectangle(System.Windows.Media.Brushes.PapayaWhip, null, new Rect(new System.Windows.Size(formattedText.Width + 50, formattedText.Height + 5)), 5.0, 5.0);

        // Draw the outline based on the properties that are set.
        drawingContext.DrawGeometry(System.Windows.Media.Brushes.Gold, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Maroon, 1.5), textGeometry);

        // Return the updated DrawingGroup content to be used by the control.
        return drawingGroup;
      }
    }

    void WalkGeometryChildren(List<string> list, Geometry geometry)
    {
      switch (geometry)
      {
        case GeometryGroup group:
        {
          foreach (var child in group.Children)
          {
            WalkGeometryChildren(list, child);
          }

          break;
        }

        case CombinedGeometry combined:
        {
          break;
        }

        case EllipseGeometry ellipse:
        {
          break;
        }

        case LineGeometry line:
        {
          break;
        }

        case RectangleGeometry rectangle:
        {
          break;
        }

        case StreamGeometry stream:
        {
          break;
        }

        case PathGeometry path:
        {
          var sb = new StringBuilder();
          foreach (var figure in path.Figures)
          {
            sb.Append(string.Format(CultureInfo.InvariantCulture, "M{0:F3},{1:F3}", figure.StartPoint.X, figure.StartPoint.Y));

            foreach (var seg in figure.Segments)
            {
              switch (seg)
              {
                case LineSegment line:
                  sb.Append(string.Format(CultureInfo.InvariantCulture, " L{0:F3},{1:F3}", line.Point.X, line.Point.Y));
                  break;

                case BezierSegment bezier:
                  sb.Append(string.Format(CultureInfo.InvariantCulture, " C{0:F3},{1:F3} {2:F3},{3:F3} {4:F3},{5:F3} ", bezier.Point1.X, bezier.Point1.Y, bezier.Point2.X, bezier.Point2.Y, bezier.Point3.X, bezier.Point3.Y));
                  break;

                case ArcSegment arc:
                  break;

                case PolyLineSegment polyLine:
                {
                  sb.Append("L");

                  foreach (var point in polyLine.Points)
                  {
                    sb.Append(string.Format(CultureInfo.InvariantCulture, " {0:F3},{1:F3}", point.X, point.Y));
                  }
                  break;
                }

                case PolyBezierSegment polyBezier:
                {
                  for (int i = 0; i < polyBezier.Points.Count; i += 3)
                  {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " C{0:F3},{1:F3} {2:F3},{3:F3} {4:F3},{5:F3} ", polyBezier.Points[i].X, polyBezier.Points[i].Y, polyBezier.Points[i + 1].X, polyBezier.Points[i + 1].Y, polyBezier.Points[i + 2].X, polyBezier.Points[i + 2].Y);
                  }
                  break;
                }

                case PolyQuadraticBezierSegment polyQuadratic:
                  break;

                case QuadraticBezierSegment quadraticBezier:
                  break;

                default:
                  break;
              }
            }

            if (figure.IsClosed)
              sb.Append("z");


          }
          list.Add(sb.ToString());

          break;
        }

        default:
          break;
      }
    }

  }
}

                                                  