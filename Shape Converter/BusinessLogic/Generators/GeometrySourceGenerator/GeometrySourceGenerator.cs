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
using System.Globalization;
using System.Text;
using System.Windows;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator
{
    /// <summary>
    /// The geometry source code generator base class
    /// </summary>
    public abstract class GeometrySourceGenerator : IGeometrySourceGenerator
    {
        private double aspectRatio;
        private string scaleWidthVariableName;
        private string scaleHeightVariableName;

        protected bool isClosed;
        protected bool finalized;

        protected StringBuilder code = new StringBuilder();

        /// <summary>
        /// Specifies which aspect to normalize
        /// </summary>
        public NormalizeGeometrySourceAspect NormalizeAspect { get; set; }

        /// <summary>
        /// Specifies whether to include an offset for all coordinates in the generated code
        /// </summary>
        public bool IncludeOffset { get; set; }

        /// <summary>
        /// The filename to include in the generated source code
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Indent level 1
        /// </summary>
        protected string Indent1 { get; private set; }

        /// <summary>
        /// Indent level 2
        /// </summary>
        protected string Indent2 { get; private set; }

        /// <summary>
        /// Generates geometry source code
        /// </summary>
        public string GenerateSource(GraphicVisual visual)
        {
            code = new StringBuilder();

            var normalizer = new NormalizeVisual();
            NormalizeAspect normalizeType;

            switch (NormalizeAspect)
            {
                case NormalizeGeometrySourceAspect.Width:
                    normalizeType = Generators.NormalizeAspect.Width;
                    scaleWidthVariableName = "scale";
                    scaleHeightVariableName = "scale";
                    break;

                case NormalizeGeometrySourceAspect.Height:
                    normalizeType = Generators.NormalizeAspect.Height;
                    scaleWidthVariableName = "scale";
                    scaleHeightVariableName = "scale";
                    break;

                default:
                    normalizeType = Generators.NormalizeAspect.Individual;
                    scaleWidthVariableName = "width";
                    scaleHeightVariableName = "height";
                    break;
            }

            var normalizedVisual = normalizer.Normalize(visual, normalizeType, 1.0);
            aspectRatio = normalizer.AspectRatio;

            Init(visual);
            GenerateSourceRecursive(normalizedVisual);
            Terminate();

            return code.ToString();
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
                    foreach (var childVisual in group.Childreen)
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
            if (IncludeOffset)
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
        protected string FormatScaledValue(double normalizedValue, string scaleVariableName, string offset)
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
        public static bool IsEqual(double d1, double d2)
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
        protected void InitCode(GraphicVisual visual)
        {
            Indent1 = SourceGeneratorHelper.GetSourceIndent(1);
            Indent2 = SourceGeneratorHelper.GetSourceIndent(2);

            var indent1 = Indent1;
            var indent2 = Indent2;

            this.finalized = false;

            if (NormalizeAspect != NormalizeGeometrySourceAspect.Individual)
            {
                string scaleVariableName = string.Empty;
                string opositeVariableName = string.Empty;
                string methodName = string.Empty;
                string opositeMethodName = string.Empty;

                switch (NormalizeAspect)
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
                code.AppendLine("/// <summary>");
                code.AppendLine("/// Create a geometry.");
                code.AppendLine("/// The origin is normalized to 0/0.");
                code.AppendLine(string.Format("/// The {0} is normalized to 1.0, the aspect ratio is kept", scaleVariableName));
                code.AppendLine(string.Format("/// from the original stream, which means the {0} might be", opositeVariableName));
                code.AppendLine("/// greater than 1 (depending on the aspect ratio).");
                code.AppendLine("/// </summary>");
                code.AppendLine("private static class xyzGeometry");
                code.AppendLine("{");

                // the aspect ratio constant
                code.AppendLine($"{indent1}/// <summary>");
                code.AppendLine($"{indent1}/// The aspect ratio (height/width) of the geometry");
                code.AppendLine($"{indent1}/// </summary>");
                code.AppendLine(string.Format("{0}public const double AspectRatio = {1};", indent1, aspectRatio.ToString("G", CultureInfo.InvariantCulture)));
                code.AppendLine("");

                // the creation method for the opposite
                string offsetMethodParameterString = string.Empty;
                string offsetParameterString = string.Empty;

                if (IncludeOffset)
                {
                    offsetMethodParameterString = "double left, double top, ";
                    offsetParameterString = "left, top, ";
                }

                code.AppendLine($"{indent1}/// <summary>");
                code.AppendLine($"{indent1}/// Create the geometry from the given {opositeVariableName} and keep the original aspect ratio");
                code.AppendLine($"{indent1}/// </summary>");
                code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, opositeVariableName));
                code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                code.AppendLine($"{indent1}public static Geometry {opositeMethodName}({offsetMethodParameterString}double {opositeVariableName})");
                code.AppendLine($"{indent1}{{");

                if (NormalizeAspect == NormalizeGeometrySourceAspect.Width)
                {
                    code.AppendLine($"{indent2}return {methodName}({offsetParameterString}{opositeVariableName} / AspectRatio);");
                }
                else
                {
                    code.AppendLine($"{indent2}return {methodName}({offsetParameterString}{opositeVariableName} * AspectRatio);");
                }

                code.AppendLine($"{indent1}}}");
                code.AppendLine("");

                // primary creation method
                code.AppendLine($"{indent1}/// <summary>");
                code.AppendLine($"{indent1}/// Create the geometry from the given {scaleVariableName} and keep the original aspect ratio");

                if (!string.IsNullOrEmpty(Filename))
                {
                    code.AppendLine(string.Format($"{indent1}/// Shapes extracted from file \"{0}\"", Filename));
                }

                code.AppendLine($"{indent1}/// Generated from the following stream geometry:");

                var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);

                foreach (var stream in streams)
                {
                    code.Append($"{indent1}/// ");
                    code.AppendLine(stream);
                }

                code.AppendLine($"{indent1}/// </summary>");
                code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, scaleVariableName));
                code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                code.AppendLine($"{indent1}public static Geometry {methodName}({offsetMethodParameterString}double {scaleVariableName})");

                code.AppendLine($"{indent1}{{");
                code.AppendLine($"{indent2}double scale = {scaleVariableName};");
            }
            else
            {
                // class header 
                code.AppendLine("/// <summary>");
                code.AppendLine("/// Create a geometry.");
                code.AppendLine("/// The origin is normalized to 0/0.");
                code.AppendLine("/// Width and height are normalized independently to 1.0");
                code.AppendLine("/// </summary>");
                code.AppendLine("private static class XYZGeometry");
                code.AppendLine("{");

                // the aspect ratio constant
                code.AppendLine($"{indent1}/// <summary>");
                code.AppendLine($"{indent1}/// The aspect ratio (height/width) of the original geometry");
                code.AppendLine($"{indent1}/// </summary>");
                code.AppendLine(string.Format("{0}public const double AspectRatio = {1};", indent1, aspectRatio.ToString("G", CultureInfo.InvariantCulture)));
                code.AppendLine("");

                string offsetMethodParameterString = string.Empty;
                string offsetParameterString = string.Empty;

                if (IncludeOffset)
                {
                    offsetMethodParameterString = "double left, double top, ";
                    offsetParameterString = "left, top, ";
                }

                // the creation method for
                CreateSecondaryCreationMethod(code, NormalizeGeometrySourceAspect.Height, "CreateFromHeight", "Create", "height", offsetMethodParameterString, offsetParameterString);
                CreateSecondaryCreationMethod(code, NormalizeGeometrySourceAspect.Width, "CreateFromWidth", "Create", "width", offsetMethodParameterString, offsetParameterString);

                // primary creation method
                code.AppendLine($"{indent1}/// <summary>");
                code.AppendLine($"{indent1}/// Create the geometry from the given width and height with any aspect ratio");

                if (!string.IsNullOrEmpty(Filename))
                {
                    code.AppendLine(string.Format("{0}/// Shapes extracted from file \"{1}\"", indent1, Filename));
                }

                code.AppendLine($"{indent1}/// Generated from the following stream geometry:");

                var streams = StreamSourceGenerator.GenerateStreamGeometries(visual);

                foreach (var stream in streams)
                {
                    code.Append($"{indent1}/// ");
                    code.AppendLine(stream);
                }

                code.AppendLine($"{indent1}/// </summary>");
                code.AppendLine($"{indent1}/// <param name=\"width\">the width in WPF units</param>");
                code.AppendLine($"{indent1}/// <param name=\"height\">the height in WPF units</param>");
                code.AppendLine($"{indent1}/// <returns>Returns the geometry</returns>");

                code.AppendLine($"{indent1}public static Geometry Create({offsetMethodParameterString}double width, double height)");
                code.AppendLine($"{indent1}{{");
            }
        }

        /// <summary>
        /// Creates the helper method source code
        /// </summary>
        private void CreateSecondaryCreationMethod(StringBuilder code, NormalizeGeometrySourceAspect normalizeAspect, string methodName, string mainMethodName, string parameterName, string offsetMethodParameterString, string offsetParameterString)
        {
            var indent1 = Indent1;
            var indent2 = Indent2;

            code.AppendLine($"{indent1}/// <summary>");
            code.AppendLine($"{indent1}/// Create the geometry from the given {parameterName} and keep the original aspect ratio");
            code.AppendLine($"{indent1}/// </summary>");
            code.AppendLine(string.Format("{0}/// <param name=\"{1}\">the {1} in WPF units</param>", indent1, parameterName));
            code.AppendLine("    /// <returns>Returns the geometry</returns>");

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
            code.AppendLine("");
        }

        /// <summary>
        /// Create the finalization source code
        /// </summary>
        public void FinalizeCode()
        {
            code.AppendLine($"{Indent1}}}");
            code.AppendLine("}");
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
