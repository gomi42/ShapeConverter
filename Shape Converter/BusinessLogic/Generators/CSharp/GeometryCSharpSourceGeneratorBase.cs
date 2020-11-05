//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
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
using System.Globalization;
using System.Text;
using System.Windows;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators.GeometryCSharpSourceGenerator
{
    /// <summary>
    /// The geometry source code generator base class
    /// </summary>
    internal abstract class GeometryCSharpSourceGeneratorBase
    {
        private double aspectRatio;
        private string scaleWidthVariableName;
        private string scaleHeightVariableName;

        /// <summary>
        /// Specifies which aspect to normalize
        /// </summary>
        private NormalizeGeometrySourceAspect normalizeAspect;

        /// <summary>
        /// Specifies whether to include an offset for all coordinates in the generated code
        /// </summary>
        private bool includeOffset;

        /// <summary>
        /// The filename to include in the generated source code
        /// </summary>
        private string filename;

        /// <summary>
        /// Indent level 1
        /// </summary>
        private string indent1;

        /// <summary>
        /// The created source code
        /// </summary>
        protected StringBuilder Code { get; private set; }

        /// <summary>
        /// Indent level 2
        /// </summary>
        protected string MethodBodyIndent { get; private set; }

        /// <summary>
        /// Generates geometry source code
        /// </summary>
        public string GenerateSource(GraphicVisual visual, 
                                     NormalizeGeometrySourceAspect normalizeGeometrySourceAspect,
                                     bool includeOffset,
                                     string filename)
        {
            this.normalizeAspect = normalizeGeometrySourceAspect;
            this.includeOffset = includeOffset;
            this.filename = filename;

            Code = new StringBuilder();

            var normalizer = new NormalizeVisual();
            NormalizeAspect normalizeType;

            switch (normalizeAspect)
            {
                case NormalizeGeometrySourceAspect.Width:
                    normalizeType = NormalizeAspect.Width;
                    scaleWidthVariableName = "scale";
                    scaleHeightVariableName = "scale";
                    break;

                case NormalizeGeometrySourceAspect.Height:
                    normalizeType = NormalizeAspect.Height;
                    scaleWidthVariableName = "scale";
                    scaleHeightVariableName = "scale";
                    break;

                default:
                    normalizeType = NormalizeAspect.Individual;
                    scaleWidthVariableName = "width";
                    scaleHeightVariableName = "height";
                    break;
            }

            var normalizedVisual = normalizer.Normalize(visual, normalizeType, 1.0);
            aspectRatio = normalizer.AspectRatio;

            InitCode(visual);
            Init(visual);

            GenerateSourceRecursive(normalizedVisual);

            Terminate();
            FinalizeCode();

            return Code.ToString();
        }

        /// <summary>
        /// Generates geometry source code recursively
        /// </summary>
        private void GenerateSourceRecursive(GraphicVisual visual)
        {
            switch (visual)
            {
                case GraphicGroup group:
                {
                    foreach (var childVisual in group.Children)
                    {
                        GenerateSourceRecursive(childVisual);
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
        /// Generate code for a path
        /// </summary>
        private void GeneratePath(GraphicPath graphicPath)
        {
            foreach (var segment in graphicPath.Geometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicsMove:
                    {
                        BeginFigure(graphicsMove.StartPoint, true, graphicsMove.IsClosed);
                        break;
                    }

                    case GraphicLineSegment graphicsLineTo:
                    {
                        LineTo(graphicsLineTo.To, true, true);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicsCubicBezier:
                    {
                        BezierTo(graphicsCubicBezier.ControlPoint1, graphicsCubicBezier.ControlPoint2, graphicsCubicBezier.EndPoint, true, true);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        QuadraticBezierTo(graphicQuadraticBezier.ControlPoint, graphicQuadraticBezier.EndPoint, true, true);
                        break;
                    }

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Format a point
        /// </summary>
        protected string FormatPoint(Point point)
        {
            if (includeOffset)
            {
                var xStr = FormatScaledValue(point.X, scaleWidthVariableName, "left");
                var yStr = FormatScaledValue(point.Y, scaleHeightVariableName, "top");

                return string.Format("new Point({0}, {1})", xStr, yStr);
            }
            else
            {
                var xStr = FormatScaledValue(point.X, scaleWidthVariableName);
                var yStr = FormatScaledValue(point.Y, scaleHeightVariableName);

                return string.Format("new Point({0}, {1})", xStr, yStr);
            }
        }

        /// <summary>
        /// Format one double value for scaling
        /// </summary>
        private string FormatScaledValue(double normalizedValue, string scaleVariableName, string offset)
        {
            if (IsEqual(normalizedValue, 0.0))
            {
                return offset;
            }
            else
            if (IsEqual(normalizedValue, 1.0))
            {
                return $"{scaleVariableName} + {offset}";
            }

            string dblStr = DoubleUtilities.FormatString(normalizedValue);
            return $"{dblStr} * {scaleVariableName} + {offset}";
        }

        /// <summary>
        /// Format one double value for scaling
        /// </summary>
        protected string FormatScaledValue(double normalizedValue, string scaleVariableName)
        {
            if (IsEqual(normalizedValue, 0.0))
            {
                return "0";
            }
            else
            if (IsEqual(normalizedValue, 1.0))
            {
                return scaleVariableName;
            }

            string dblStr = DoubleUtilities.FormatString(normalizedValue);
            return $"{dblStr} * {scaleVariableName}";
        }

        /// <summary>
        /// Test a double for equality
        /// </summary>
        /// <returns></returns>
        private static bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 1e-6;
        }

        /// <summary>
        /// Format a size
        /// </summary>
        protected string FormatSize(Size size)
        {
            var xStr = FormatScaledValue(size.Width, scaleWidthVariableName);
            var yStr = FormatScaledValue(size.Height, scaleHeightVariableName);

            return string.Format("new Size({0}, {1})", xStr, yStr);
        }

        /// <summary>
        /// Format a bool
        /// </summary>
        protected string FormatBool(Boolean bool1)
        {
            return bool1 ? "true" : "false";
        }

        /// <summary>
        /// Create the beginning of the source code
        /// </summary>
        private void InitCode(GraphicVisual visual)
        {
            indent1 = SourceFormatterHelper.GetSourceIndent(1);
            MethodBodyIndent = SourceFormatterHelper.GetSourceIndent(2);
            var indent2 = MethodBodyIndent;

            if (normalizeAspect != NormalizeGeometrySourceAspect.Individual)
            {
                string scaleVariableName = string.Empty;
                string opositeVariableName = string.Empty;
                string methodName = string.Empty;
                string opositeMethodName = string.Empty;

                switch (normalizeAspect)
                {
                    case NormalizeGeometrySourceAspect.Width:
                        scaleVariableName = "width";
                        opositeVariableName = "height";
                        methodName = "CreateFromWidth";
                        opositeMethodName = "CreateFromHeight";
                        break;

                    case NormalizeGeometrySourceAspect.Height:
                        scaleVariableName = "height";
                        opositeVariableName = "width";
                        methodName = "CreateFromHeight";
                        opositeMethodName = "CreateFromWidth";
                        break;
                }

                // class header 
                Code.AppendLine("/// <summary>");
                Code.AppendLine("/// Create a geometry.");
                Code.AppendLine("/// The origin is normalized to 0/0.");
                Code.AppendLine(string.Format("/// The {0} is normalized to 1.0, the aspect ratio is kept", scaleVariableName));
                Code.AppendLine(string.Format("/// from the original stream, which means the {0} might be", opositeVariableName));
                Code.AppendLine("/// greater than 1 (depending on the aspect ratio).");
                Code.AppendLine("/// </summary>");
                Code.AppendLine("private static class xyzGeometry");
                Code.AppendLine("{");

                // the aspect ratio constant
                Code.AppendLine($"{indent1}/// <summary>");
                Code.AppendLine($"{indent1}/// The aspect ratio (height/width) of the geometry");
                Code.AppendLine($"{indent1}/// </summary>");
                Code.AppendLine(string.Format("{0}public const double AspectRatio = {1};", indent1, aspectRatio.ToString("G", CultureInfo.InvariantCulture)));
                Code.AppendLine();

                // the creation method for the opposite
                string offsetMethodParameterString = string.Empty;
                string offsetParameterString = string.Empty;

                if (includeOffset)
                {
                    offsetMethodParameterString = "double left, double top, ";
                    offsetParameterString = "left, top, ";
                }

                Code.AppendLine($"{indent1}/// <summary>");
                Code.AppendLine($"{indent1}/// Create the geometry from the given {opositeVariableName} and keep the original aspect ratio");
                Code.AppendLine($"{indent1}/// </summary>");
                Code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, opositeVariableName));
                Code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                Code.AppendLine($"{indent1}public static Geometry {opositeMethodName}({offsetMethodParameterString}double {opositeVariableName})");
                Code.AppendLine($"{indent1}{{");

                if (normalizeAspect == NormalizeGeometrySourceAspect.Width)
                {
                    Code.AppendLine($"{indent2}return {methodName}({offsetParameterString}{opositeVariableName} / AspectRatio);");
                }
                else
                {
                    Code.AppendLine($"{indent2}return {methodName}({offsetParameterString}{opositeVariableName} * AspectRatio);");
                }

                Code.AppendLine($"{indent1}}}");
                Code.AppendLine();

                // primary creation method
                Code.AppendLine($"{indent1}/// <summary>");
                Code.AppendLine($"{indent1}/// Create the geometry from the given {scaleVariableName} and keep the original aspect ratio");

                if (!string.IsNullOrEmpty(filename))
                {
                    Code.AppendLine(string.Format("{0}/// Shapes extracted from file \"{1}\"", indent1, filename));
                }

                Code.AppendLine($"{indent1}/// Generated from the following stream geometry:");

                var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);

                foreach (var stream in streams)
                {
                    Code.Append($"{indent1}/// ");
                    Code.AppendLine(stream);
                }

                Code.AppendLine($"{indent1}/// </summary>");
                Code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, scaleVariableName));
                Code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                Code.AppendLine($"{indent1}public static Geometry {methodName}({offsetMethodParameterString}double {scaleVariableName})");

                Code.AppendLine($"{indent1}{{");
                Code.AppendLine($"{indent2}double scale = {scaleVariableName};");
            }
            else
            {
                // class header 
                Code.AppendLine("/// <summary>");
                Code.AppendLine("/// Create a geometry.");
                Code.AppendLine("/// The origin is normalized to 0/0.");
                Code.AppendLine("/// Width and height are normalized independently to 1.0");
                Code.AppendLine("/// </summary>");
                Code.AppendLine("private static class XYZGeometry");
                Code.AppendLine("{");

                // the aspect ratio constant
                Code.AppendLine($"{indent1}/// <summary>");
                Code.AppendLine($"{indent1}/// The aspect ratio (height/width) of the original geometry");
                Code.AppendLine($"{indent1}/// </summary>");
                Code.AppendLine(string.Format("{0}public const double AspectRatio = {1};", indent1, aspectRatio.ToString("G", CultureInfo.InvariantCulture)));
                Code.AppendLine();

                string offsetMethodParameterString = string.Empty;
                string offsetParameterString = string.Empty;

                if (includeOffset)
                {
                    offsetMethodParameterString = "double left, double top, ";
                    offsetParameterString = "left, top, ";
                }

                // the creation method for
                CreateSecondaryCreationMethod(Code, NormalizeGeometrySourceAspect.Height, "CreateFromHeight", "Create", "height", offsetMethodParameterString, offsetParameterString);
                CreateSecondaryCreationMethod(Code, NormalizeGeometrySourceAspect.Width, "CreateFromWidth", "Create", "width", offsetMethodParameterString, offsetParameterString);

                // primary creation method
                Code.AppendLine($"{indent1}/// <summary>");
                Code.AppendLine($"{indent1}/// Create the geometry from the given width and height with any aspect ratio");

                if (!string.IsNullOrEmpty(filename))
                {
                    Code.AppendLine(string.Format("{0}/// Shapes extracted from file \"{1}\"", indent1, filename));
                }

                Code.AppendLine($"{indent1}/// Generated from the following stream geometry:");

                var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);

                foreach (var stream in streams)
                {
                    Code.Append($"{indent1}/// ");
                    Code.AppendLine(stream);
                }

                Code.AppendLine($"{indent1}/// </summary>");
                Code.AppendLine($"{indent1}/// <param name=\"width\">the width in WPF units</param>");
                Code.AppendLine($"{indent1}/// <param name=\"height\">the height in WPF units</param>");
                Code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                Code.AppendLine($"{indent1}public static Geometry Create({offsetMethodParameterString}double width, double height)");
                Code.AppendLine($"{indent1}{{");
            }
        }

        /// <summary>
        /// Creates the helper method source code
        /// </summary>
        private void CreateSecondaryCreationMethod(StringBuilder code, NormalizeGeometrySourceAspect normalizeAspect, string methodName, string mainMethodName, string parameterName, string offsetMethodParameterString, string offsetParameterString)
        {
            var indent2 = MethodBodyIndent;

            code.AppendLine($"{indent1}/// <summary>");
            code.AppendLine($"{indent1}/// Create the geometry from the given {parameterName} and keep the original aspect ratio");
            code.AppendLine($"{indent1}/// </summary>");
            code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, parameterName));
            code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

            code.AppendLine($"{indent1}public static Geometry {methodName}({offsetMethodParameterString}double {parameterName})");
            code.AppendLine($"{indent1}{{");

            if (normalizeAspect == NormalizeGeometrySourceAspect.Height)
            {
                code.AppendLine($"{indent2}return {mainMethodName}({offsetParameterString}height / AspectRatio, height);");
            }
            else
            {
                code.AppendLine($"{indent2}return {mainMethodName}({offsetParameterString}width, width * AspectRatio);");
            }

            code.AppendLine($"{indent1}}}");
            code.AppendLine();
        }

        /// <summary>
        /// Create the finalization source code
        /// </summary>
        private void FinalizeCode()
        {
            Code.AppendLine($"{indent1}}}");
            Code.AppendLine("}");
        }

        /// <summary>
        /// Initialize a stream generation
        /// </summary>
        protected abstract void Init(GraphicVisual visual);

        /// <summary>
        /// Create code for beginning a figure
        /// </summary>
        protected abstract void BeginFigure(Point startPoint, bool isFilled, bool isClosed);

        /// <summary>
        /// Create code for a LineTo operation
        /// </summary>
        protected abstract void LineTo(Point point, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// Create code for a bezier operation
        /// </summary>
        protected abstract void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// Create code for a quadratic bezier operation
        /// </summary>
        protected abstract void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin);

        /// <summary>
        /// Create code for terminating a figure
        /// </summary>
        protected abstract void Terminate();
    }
}
