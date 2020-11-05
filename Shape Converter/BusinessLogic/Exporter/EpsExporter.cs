//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//
// This file is part of ShapeConverter.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Exporter
{
    /// <summary>
    /// The EPS exporter
    /// </summary>
    public class EpsExporter
    {
        private StreamWriter outputStream;
        private bool nonSupportedOpacityFound;

        /// <summary>
        /// Export to EPS
        /// </summary>
        public void Export(GraphicVisual visual, int width, string filename, out string message)
        {
            outputStream = new StreamWriter(filename, false, Encoding.ASCII);

            var normalizer = new NormalizeVisual();
            var normalizedVisual = normalizer.Normalize(visual, NormalizeAspect.Width, width);

            double height = normalizer.AspectRatio * width;
            InitEpsFile(width, height);
            Generate(normalizedVisual);
            FinalizeEpsFile();

            outputStream.Close();
            outputStream.Dispose();

            if (nonSupportedOpacityFound)
            {
                message = "EPS does not support opacity, opacity ignored";
            }
            else
            {
                message = string.Empty;
            }
        }

        /// <summary>
        /// Write start of the EPS file
        /// </summary>
        private void InitEpsFile(double width, double height)
        {
            var widthStr = DoubleUtilities.FormatString(Math.Ceiling(width));
            var heightStr = DoubleUtilities.FormatString(Math.Ceiling(height));

            outputStream.WriteLine("%!PS-Adobe-3.1 EPSF-3.0");
            outputStream.WriteLine($"%%BoundingBox: 0 0 {widthStr} {heightStr}");
            outputStream.WriteLine("%%Pages: 1");
            outputStream.WriteLine("%%EndComments");
            outputStream.WriteLine("%%BeginProlog");
            outputStream.WriteLine("%%EndProlog");
            outputStream.WriteLine("%%BeginSetup");
            outputStream.WriteLine("%%EndSetup");
            outputStream.WriteLine("%%Page: 1 1");
            outputStream.WriteLine("%%BeginPageSetup");
            outputStream.WriteLine("%%EndPageSetup");
            outputStream.WriteLine($"1 -1 scale 0 -{heightStr} translate");
        }

        /// <summary>
        /// Finalize the EPS file
        /// </summary>
        private void FinalizeEpsFile()
        {
            outputStream.WriteLine("%%Trailer");
            outputStream.WriteLine("%%EOF");
        }

        /// <summary>
        /// Generates the source code for a given visual.
        /// </summary>
        private void Generate(GraphicVisual visual)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    bool graphicStateSaved = false;

                    if (!DoubleUtilities.IsEqual(group.Opacity, 1.0))
                    {
                        // EPS doesn't support opacity
                        nonSupportedOpacityFound = true;
                    }

                    if (group.Clip != null)
                    {
                        graphicStateSaved = true;
                        outputStream.WriteLine("gsave");

                        GenerateShape(group.Clip);
                        outputStream.WriteLine("clippath");
                    }

                    foreach (var childVisual in group.Children)
                    {
                        Generate(childVisual);
                    }

                    if (graphicStateSaved)
                    {
                        outputStream.WriteLine("grestore");
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    GeneratePath(graphicPath);
                    break;
                }
            }
        }

        /// <summary>
        /// Generate the source code for the given path
        /// </summary>
        private void GeneratePath(GraphicPath graphicPath)
        {
            GenerateShape(graphicPath.Geometry);

            if (graphicPath.FillBrush != null)
            {
                bool graphicStateSaved = false;

                if (graphicPath.StrokeBrush != null)
                {
                    graphicStateSaved = true;
                    outputStream.WriteLine("gsave");
                }

                WriteBrush(graphicPath.FillBrush, graphicPath.Geometry.Bounds);
                outputStream.WriteLine("fill");

                if (graphicStateSaved)
                {
                    outputStream.WriteLine("grestore");
                }
            }

            if (graphicPath.StrokeBrush != null)
            {
                WriteBrush(graphicPath.StrokeBrush, graphicPath.Geometry.Bounds);

                var widthStr = DoubleUtilities.FormatString(graphicPath.StrokeThickness);
                outputStream.WriteLine($"{widthStr} setlinewidth");

                WriteLineCap(graphicPath.StrokeLineCap);
                WriteLineJoin(graphicPath.StrokeLineJoin);
                WriteDash(graphicPath);

                var miterStr = DoubleUtilities.FormatString(graphicPath.StrokeMiterLimit);
                outputStream.WriteLine($"{miterStr} setmiterlimit");

                outputStream.WriteLine("stroke");
            }
        }

        /// <summary>
        /// Generate the source code for the given geometry
        /// </summary>
        public void GenerateShape(GraphicPathGeometry geometry)
        {
            outputStream.WriteLine("newpath");
            bool closeLastPath = false;

            foreach (var segment in geometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        if (closeLastPath)
                        {
                            outputStream.WriteLine("closepath");
                        }

                        var pointStr = FormatPoint(graphicMove.StartPoint);
                        outputStream.WriteLine($"{pointStr} moveto");
                        closeLastPath = graphicMove.IsClosed;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        var pointStr = FormatPoint(graphicLineTo.To);
                        outputStream.WriteLine($"{pointStr} lineto");
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        var point1Str = FormatPoint(graphicCubicBezier.ControlPoint1);
                        var point2Str = FormatPoint(graphicCubicBezier.ControlPoint2);
                        var endPointStr = FormatPoint(graphicCubicBezier.EndPoint);
                        outputStream.WriteLine($"{point1Str} {point2Str} {endPointStr} curveto");
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        var pointStr = FormatPoint(graphicQuadraticBezier.ControlPoint);
                        var endPointStr = FormatPoint(graphicQuadraticBezier.EndPoint);
                        outputStream.WriteLine($"{pointStr} {pointStr} {endPointStr} curveto");
                        break;
                    }

                    default:
                        break;
                }
            }

            if (closeLastPath)
            {
                outputStream.WriteLine("closepath");
            }
        }

        /// <summary>
        /// Write a brush to the stream
        /// </summary>
        private void WriteBrush(GraphicBrush graphicBrush, Rect boundingBox)
        {
            switch (graphicBrush)
            {
                case GraphicSolidColorBrush solidStrokeColor:
                {
                    WriteSetColor(solidStrokeColor.Color);
                    break;
                }

                case GraphicLinearGradientBrush linearGradientBrush:
                {
                    WriteLinearShading(linearGradientBrush, boundingBox);
                    break;
                }

                case GraphicRadialGradientBrush radialGradientBrush:
                {
                    WriteRadialShading(radialGradientBrush, boundingBox);
                    break;
                }
            }
        }

        /// <summary>
        /// Write a linear shading definition
        /// </summary>
        private void WriteLinearShading(GraphicGradientBrush gradientBrush, Rect boundingBox)
        {
            string startPointStr = FormatPoint(GetAbsolutePosition(boundingBox, gradientBrush.StartPoint));
            string endPointStr = FormatPoint(GetAbsolutePosition(boundingBox, gradientBrush.EndPoint));
            string coordsStr = $"/Coords [{startPointStr} {endPointStr}]";

            WriteShadingPattern("2", gradientBrush, boundingBox, coordsStr);
        }

        /// <summary>
        /// Write a radial shading definition
        /// </summary>
        private void WriteRadialShading(GraphicRadialGradientBrush gradientBrush, Rect boundingBox)
        {
            string center0Str = FormatPoint(GetAbsolutePosition(boundingBox, gradientBrush.StartPoint));
            string center1Str = FormatPoint(GetAbsolutePosition(boundingBox, gradientBrush.EndPoint));

            string radius0Str = DoubleUtilities.FormatString(GetAbsoluteLength(boundingBox, 0));
            string radius1Str = DoubleUtilities.FormatString(GetAbsoluteLength(boundingBox, gradientBrush.RadiusX));

            string coordsStr = $"/Coords [{center0Str} {radius0Str} {center1Str} {radius1Str}]";
            WriteShadingPattern("3", gradientBrush, boundingBox, coordsStr);
        }

        /// <summary>
        /// Write a shading pattern definition
        /// </summary>
        private void WriteShadingPattern(string shadingType, GraphicGradientBrush gradientBrush, Rect boundingBox, string coords)
        {
            outputStream.WriteLine("[/Pattern] setcolorspace");

            outputStream.WriteLine("<<");
            outputStream.WriteLine("/PatternType 2");
            outputStream.WriteLine("/Shading");

            outputStream.WriteLine("<<");
            outputStream.WriteLine($"/ShadingType {shadingType}");
            outputStream.WriteLine("/ColorSpace [/DeviceRGB]");

            outputStream.WriteLine(coords);

            outputStream.WriteLine("/Domain [0 1]");
            outputStream.WriteLine("/Extend [true true]");
            outputStream.WriteLine("/Function");

            if (gradientBrush.GradientStops.Count == 2)
            {
                WriteExponentialFunction(gradientBrush.GradientStops[0].Color, gradientBrush.GradientStops[1].Color);
            }
            else
            {
                WriteStitchingFunction(gradientBrush.GradientStops);
            }

            outputStream.WriteLine(">>");

            outputStream.WriteLine(">>");

            outputStream.WriteLine("matrix");
            outputStream.WriteLine("makepattern");
            outputStream.WriteLine("setcolor");
        }

        /// <summary>
        /// Write a stitching function definition describing the gradient stops
        /// </summary>
        private void WriteStitchingFunction(List<GraphicGradientStop> gradientStops)
        {
            outputStream.WriteLine("<<");
            outputStream.WriteLine("/FunctionType 3");

            string boundFromStr = DoubleUtilities.FormatString(gradientStops[0].Position);
            string boundToStr = DoubleUtilities.FormatString(gradientStops[gradientStops.Count - 1].Position);
            outputStream.WriteLine($"/Domain[{boundFromStr} {boundToStr}]");

            outputStream.WriteLine("/Range [0 1 0 1 0 1]");
            outputStream.WriteLine("/Encode [0 1 0 1 0 1]");

            outputStream.Write("/Bounds [");

            for (int i = 1; i < gradientStops.Count - 1; i++)
            {
                if (i > 1)
                {
                    outputStream.Write(" ");
                }

                string boundStr = DoubleUtilities.FormatString(gradientStops[i].Position);
                outputStream.Write(boundStr);
            }

            outputStream.WriteLine("]");

            outputStream.WriteLine("/Functions [");

            for (int i = 0; i < gradientStops.Count - 1; i++)
            {
                var stopFrom = gradientStops[i];
                var stopTo = gradientStops[i + 1];

                WriteExponentialFunction(stopFrom.Color, stopTo.Color);
            }

            outputStream.WriteLine("]");
            outputStream.WriteLine(">>");
        }

        /// <summary>
        /// Write an exponential function definition describing a single gradient stop
        /// </summary>
        private void WriteExponentialFunction(Color colorFrom, Color colorTo)
        {
            var colorFromStr = FormatColor(colorFrom);
            var colorToStr = FormatColor(colorTo);

            outputStream.WriteLine("<<");
            outputStream.WriteLine("/FunctionType 2");
            outputStream.WriteLine("/Domain [0 1]");
            outputStream.WriteLine("/Range [0 1 0 1 0 1]");
            outputStream.WriteLine("/Order 1");
            outputStream.WriteLine($"/C0 [{colorFromStr}]");
            outputStream.WriteLine($"/C1 [{colorToStr}]");
            outputStream.WriteLine("/N 1");
            outputStream.WriteLine(">>");
        }

        /// <summary>
        /// Write a setrgbcolor statement
        /// </summary>
        /// <param name="color"></param>
        private void WriteSetColor(Color color)
        {
            var colorStr = FormatColor(color);
            outputStream.WriteLine($"{colorStr} setrgbcolor");
        }

        /// <summary>
        /// Write a GraphicLineCap
        /// </summary>
        public void WriteLineCap(GraphicLineCap graphicLineCap)
        {
            int lineCap = 0;

            switch (graphicLineCap)
            {
                case GraphicLineCap.Flat:
                    lineCap = 0;
                    break;

                case GraphicLineCap.Round:
                    lineCap = 1;
                    break;

                case GraphicLineCap.Square:
                    lineCap = 2;
                    break;
            }

            outputStream.WriteLine($"{lineCap.ToString()} setlinecap");
        }

        /// <summary>
        /// Write a GraphicLineJoin
        /// </summary>
        public void WriteLineJoin(GraphicLineJoin graphicLineJoin)
        {
            int lineJoin = 0;

            switch (graphicLineJoin)
            {
                case GraphicLineJoin.Miter:
                    lineJoin = 0;
                    break;

                case GraphicLineJoin.Bevel:
                    lineJoin = 2;
                    break;

                case GraphicLineJoin.Round:
                    lineJoin = 1;
                    break;
            }

            outputStream.WriteLine($"{lineJoin.ToString()} setlinejoin");
        }

        /// <summary>
        /// Write the dashes
        /// </summary>
        public void WriteDash(GraphicPath graphicPath)
        {
            if (graphicPath.StrokeDashes != null)
            {
                var result = new StringBuilder();
                result.Append("[");

                for (int i = 0; i < graphicPath.StrokeDashes.Count; i++)
                {
                    if (i != 0)
                    {
                        result.Append(" ");
                    }

                    result.Append(DoubleUtilities.FormatString(graphicPath.StrokeDashes[i] * graphicPath.StrokeThickness));
                }

                result.Append("] ");
                result.Append(DoubleUtilities.FormatString(graphicPath.StrokeDashOffset));

                outputStream.WriteLine($"{result.ToString()} setdash");
            }
        }

        /// <summary>
        /// Format a point to a string
        /// </summary>
        private string FormatPoint(double x, double y)
        {
            string xstr = DoubleUtilities.FormatString(x);
            string ystr = DoubleUtilities.FormatString(y);

            return $"{xstr} {ystr}";
        }

        /// <summary>
        /// Format a point to a string
        /// </summary>
        private string FormatPoint(Point point)
        {
            return FormatPoint(point.X, point.Y);
        }

        /// <summary>
        /// Format a color to a string
        /// </summary>
        /// <param name="color"></param>
        private string FormatColor(Color color)
        {
            var rStr = DoubleUtilities.FormatString(color.R / 255.0);
            var gStr = DoubleUtilities.FormatString(color.G / 255.0);
            var bStr = DoubleUtilities.FormatString(color.B / 255.0);

            return $"{rStr} {gStr} {bStr}";
        }

        /// <summary>
        /// Get absolut point
        /// </summary>
        private Point GetAbsolutePosition(Rect rect, Point relPosition)
        {
            var x = relPosition.X * (rect.Right - rect.Left) + rect.Left;
            var y = relPosition.Y * (rect.Bottom - rect.Top) + rect.Top;

            return new Point(x, y);
        }

        /// <summary>
        /// Get absolut extension
        /// </summary>
        private double GetAbsoluteLength(Rect rect, double extension)
        {
            return extension * (rect.Right - rect.Left);
        }
    }
}
