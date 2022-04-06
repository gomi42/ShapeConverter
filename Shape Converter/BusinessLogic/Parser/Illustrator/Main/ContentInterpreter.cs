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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Pdf.Pattern;
using ShapeConverter.BusinessLogic.Parser.Pdf.Shading;
using ShapeConverter.BusinessLogic.Parser.Pdf.XObject;
using ShapeConverter.Parser.Pdf;
using ShapeConverter.Parser.Pdf.ExtendedStates;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Main
{
    /// <summary>
    /// Interprets the content of a form
    /// </summary>
    internal class ContentInterpreter
    {
        ColorSpaceManager colorSpaceManager;
        FontManager fontManager;
        ExtendedStatesManager extendedStatesManager;
        ShadingManager shadingManager;
        PatternManager patternManager;
        XObjectManager xObjectManager;
        TextVectorizer textVectorizer;
        PdfDictionary properties;

        Stack<GraphicsState> graphicsStateStack;

        GraphicsState currentGraphicsState;
        FontState fontState;
        Stack<bool> visibilityStack;

        GraphicPathGeometry currentGeometry = null;

        GraphicGroup returnGraphicGroup;
        GraphicGroup graphicGroup;

        /// <summary>
        /// Init all managers
        /// </summary>
        private void Init(PdfDictionary form)
        {
            PdfResources resources = null;

            if (form != null)
            {
                var resourcesDict = form.Elements.GetDictionary(PdfKeys.Resources);
                resources = new PdfResources(resourcesDict);
            }

            extendedStatesManager = new ExtendedStatesManager();
            extendedStatesManager.Init(resources);

            fontManager = new FontManager();
            fontManager.Init(resources);

            patternManager = new PatternManager();
            patternManager.Init(resources);

            colorSpaceManager = new ColorSpaceManager(patternManager);
            colorSpaceManager.Init(resources);

            shadingManager = new ShadingManager();
            shadingManager.Init(resources);

            xObjectManager = new XObjectManager();
            xObjectManager.Init(resources);

            properties = resources.Properties;
        }

        /// <summary>
        /// Set the initial color space and color
        /// </summary>
        private void InitColor()
        {
            var seq = new CSequence();
            var real = new CReal { Value = 0.0 };
            seq.Add(real);

            currentGraphicsState.ColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceGray);
            currentGraphicsState.FillBrush =
                currentGraphicsState.ColorSpace.GetBrushDescriptor(seq,
                                                                   currentGraphicsState.CurrentTransformationMatrix,
                                                                   currentGraphicsState.FillAlpha.Current);
            currentGraphicsState.StrokeBrush =
                currentGraphicsState.ColorSpace.GetBrushDescriptor(seq,
                                                                   currentGraphicsState.CurrentTransformationMatrix,
                                                                   currentGraphicsState.FillAlpha.Current);
        }

        /// <summary>
        /// Parse a single PDF page
        /// </summary>
        public GraphicGroup Run(PdfDictionary form, CSequence sequence, GraphicsState graphicsState, PdfDictionary[] visibleGroups)
        {
            this.returnGraphicGroup = new GraphicGroup();
            graphicGroup = returnGraphicGroup;

            graphicsStateStack = new Stack<GraphicsState>();
            visibilityStack = new Stack<bool>();

            fontState = new FontState();
            textVectorizer = new TextVectorizer();
            currentGraphicsState = graphicsState;

            Init(form);
            InitColor();

            Point currentPoint = new Point(0, 0);
            GraphicMoveSegment lastMove = null;

            ResetCurrentGeometry();

            bool isVisible = true;

            for (int index = 0; index < sequence.Count; index++)
            {
                var contentOperator = sequence[index] as COperator;

                switch (contentOperator.OpCode.OpCodeName)
                {
                    // begin marked content with property
                    case OpCodeName.BDC:
                    {
                        //var tag = ((CName)contentOperator.Operands[0]).Name;
                        var props = ((CName)contentOperator.Operands[1]).Name;
                        var group = properties.Elements.GetDictionary(props);

                        visibilityStack.Push(isVisible);
                        isVisible = visibleGroups == null || visibleGroups.Contains(group);
                        break;
                    }

                    // end marked content
                    case OpCodeName.EMC:
                        isVisible = visibilityStack.Pop();
                        break;

                    // path construction operators
                    // rectangle
                    case OpCodeName.re:
                    {
                        if (currentGeometry == null)
                        {
                            currentGeometry = new GraphicPathGeometry();
                        }

                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        var width = PdfUtilities.GetDouble(contentOperator.Operands[2]);
                        var height = PdfUtilities.GetDouble(contentOperator.Operands[3]);

                        var point1 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);
                        var point2 = MatrixUtilities.TransformPoint(x + width, y + height, currentGraphicsState.CurrentTransformationMatrix);

                        var move = new GraphicMoveSegment { StartPoint = point1 };
                        currentGeometry.Segments.Add(move);

                        var lineTo = new GraphicLineSegment { To = new Point(point2.X, point1.Y) };
                        currentGeometry.Segments.Add(lineTo);

                        lineTo = new GraphicLineSegment { To = new Point(point2.X, point2.Y) };
                        currentGeometry.Segments.Add(lineTo);

                        lineTo = new GraphicLineSegment { To = new Point(point1.X, point2.Y) };
                        currentGeometry.Segments.Add(lineTo);

                        move.IsClosed = true;

                        lastMove = move;
                        currentPoint = point1;
                        break;
                    }

                    // move to
                    case OpCodeName.m:
                    {
                        if (currentGeometry == null)
                        {
                            currentGeometry = new GraphicPathGeometry();
                        }

                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        var point = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        var move = new GraphicMoveSegment { StartPoint = point };
                        currentGeometry.Segments.Add(move);

                        lastMove = move;
                        currentPoint = point;
                        break;
                    }

                    // line to
                    case OpCodeName.l:
                    {
                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        var point = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        var lineTo = new GraphicLineSegment { To = point };
                        currentGeometry.Segments.Add(lineTo);

                        currentPoint = point;
                        break;
                    }

                    // cubic bezier
                    case OpCodeName.c:
                    {
                        var bezier = new GraphicCubicBezierSegment();
                        currentGeometry.Segments.Add(bezier);

                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        bezier.ControlPoint1 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        x = PdfUtilities.GetDouble(contentOperator.Operands[2]);
                        y = PdfUtilities.GetDouble(contentOperator.Operands[3]);
                        bezier.ControlPoint2 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        x = PdfUtilities.GetDouble(contentOperator.Operands[4]);
                        y = PdfUtilities.GetDouble(contentOperator.Operands[5]);
                        bezier.EndPoint = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        currentPoint = bezier.EndPoint;
                        break;
                    }

                    // quadratic bezier
                    case OpCodeName.v:
                    {
                        var bezier = new GraphicCubicBezierSegment();
                        currentGeometry.Segments.Add(bezier);

                        bezier.ControlPoint1 = currentPoint;

                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        bezier.ControlPoint2 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        x = PdfUtilities.GetDouble(contentOperator.Operands[2]);
                        y = PdfUtilities.GetDouble(contentOperator.Operands[3]);
                        bezier.EndPoint = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        currentPoint = bezier.EndPoint;
                        break;
                    }

                    // quadratic bezier
                    case OpCodeName.y:
                    {
                        var bezier = new GraphicCubicBezierSegment();
                        currentGeometry.Segments.Add(bezier);

                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        bezier.ControlPoint1 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);

                        x = PdfUtilities.GetDouble(contentOperator.Operands[2]);
                        y = PdfUtilities.GetDouble(contentOperator.Operands[3]);
                        bezier.ControlPoint2 = MatrixUtilities.TransformPoint(x, y, currentGraphicsState.CurrentTransformationMatrix);
                        bezier.EndPoint = bezier.ControlPoint2;

                        currentPoint = bezier.EndPoint;
                        break;
                    }

                    // path painting operators
                    // end the path without filling and stroking
                    case OpCodeName.n:
                    {
                        ResetCurrentGeometry();
                        break;
                    }

                    // set clipping path
                    case OpCodeName.W:
                    case OpCodeName.Wx:
                    {
                        currentGraphicsState.ClippingPath = currentGeometry;

                        graphicGroup = new GraphicGroup();
                        graphicGroup.Clip = currentGeometry;
                        returnGraphicGroup.Children.Add(graphicGroup);
                        break;
                    }

                    // close path
                    case OpCodeName.h:
                        lastMove.IsClosed = true;
                        break;

                    // close and fill the path
                    case OpCodeName.s:
                    {
                        lastMove.IsClosed = true;

                        if (isVisible)
                        {
                            var path = GetCurrentPathFilled();
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // stroke the path
                    case OpCodeName.S:
                    {
                        if (isVisible)
                        {
                            var path = GetCurrentPathStroked();
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // close, fill and stroke the path
                    case OpCodeName.b:
                    case OpCodeName.bx:
                    {
                        lastMove.IsClosed = true;

                        if (isVisible)
                        {
                            var path = GetCurrentPathFilledAndStroked();
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // fill and stroke the path
                    case OpCodeName.B:
                    {
                        if (isVisible)
                        {
                            var path = GetCurrentPathFilledAndStroked();
                            currentGeometry.FillRule = GraphicFillRule.NoneZero;
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // fill and stroke the path
                    case OpCodeName.Bx:
                    {
                        if (isVisible)
                        {
                            var path = GetCurrentPathFilledAndStroked();
                            currentGeometry.FillRule = GraphicFillRule.NoneZero;
                            currentGeometry.FillRule = GraphicFillRule.EvenOdd;
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // fill the path
                    case OpCodeName.F:
                    case OpCodeName.f:
                    {
                        if (isVisible)
                        {
                            var path = GetCurrentPathFilled();
                            currentGeometry.FillRule = GraphicFillRule.NoneZero;
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // fill the path
                    case OpCodeName.fx:
                    {
                        if (isVisible)
                        {
                            var path = GetCurrentPathFilled();
                            currentGeometry.FillRule = GraphicFillRule.EvenOdd;
                            graphicGroup.Children.Add(path);
                        }

                        ResetCurrentGeometry();
                        break;
                    }

                    // set color space for stroking operations
                    case OpCodeName.CS:
                    {
                        var colorSpaceName = ((CName)contentOperator.Operands[0]).Name;
                        currentGraphicsState.StrokeColorSpace = colorSpaceManager.GetColorSpace(colorSpaceName);
                        break;
                    }

                    // set color space for nonstroking operations
                    case OpCodeName.cs:
                    {
                        var colorSpaceName = ((CName)contentOperator.Operands[0]).Name;
                        currentGraphicsState.ColorSpace = colorSpaceManager.GetColorSpace(colorSpaceName);
                        break;
                    }

                    // set /DeviceRGB and non-stroked color
                    case OpCodeName.rg:
                    {
                        currentGraphicsState.ColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceRGB);
                        currentGraphicsState.FillBrush =
                            currentGraphicsState.ColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                 currentGraphicsState.CurrentTransformationMatrix,
                                                                                 currentGraphicsState.FillAlpha.Current);
                        break;
                    }

                    // set /DeviceCMYK and non-stroked color
                    case OpCodeName.k:
                    {
                        currentGraphicsState.ColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceCMYK);
                        currentGraphicsState.FillBrush =
                            currentGraphicsState.ColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                 currentGraphicsState.CurrentTransformationMatrix,
                                                                                 currentGraphicsState.FillAlpha.Current);
                        break;
                    }

                    // set /DeviceGray and non-stroked color
                    case OpCodeName.g:
                    {
                        currentGraphicsState.ColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceGray);
                        currentGraphicsState.FillBrush =
                            currentGraphicsState.ColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                 currentGraphicsState.CurrentTransformationMatrix,
                                                                                 currentGraphicsState.FillAlpha.Current);
                        break;
                    }

                    // non-stroked color
                    case OpCodeName.sc:
                    {
                        currentGraphicsState.FillBrush =
                            currentGraphicsState.ColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                 currentGraphicsState.CurrentTransformationMatrix,
                                                                                 currentGraphicsState.FillAlpha.Current);
                        break;
                    }

                    // ICC based non-stroked color
                    case OpCodeName.scn:
                    {
                        currentGraphicsState.FillBrush =
                            currentGraphicsState.ColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                 currentGraphicsState.CurrentTransformationMatrix,
                                                                                 currentGraphicsState.FillAlpha.Current);
                        break;
                    }

                    // ICC based stroked color
                    case OpCodeName.SCN:
                    {
                        currentGraphicsState.StrokeBrush =
                            currentGraphicsState.StrokeColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                       currentGraphicsState.CurrentTransformationMatrix,
                                                                                       currentGraphicsState.StrokeAlpha.Current);
                        break;
                    }

                    // set /DeviceRGB and stroked color
                    case OpCodeName.RG:
                    {
                        currentGraphicsState.StrokeColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceRGB);
                        currentGraphicsState.StrokeBrush =
                            currentGraphicsState.StrokeColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                       currentGraphicsState.CurrentTransformationMatrix,
                                                                                       currentGraphicsState.StrokeAlpha.Current);
                        break;
                    }

                    // set /DeviceGray and stroked color
                    case OpCodeName.G:
                    {
                        currentGraphicsState.StrokeColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceGray);
                        currentGraphicsState.StrokeBrush =
                            currentGraphicsState.StrokeColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                       currentGraphicsState.CurrentTransformationMatrix,
                                                                                       currentGraphicsState.StrokeAlpha.Current);
                        break;
                    }

                    // set /DeviceCMYK and stroked color
                    case OpCodeName.K:
                    {
                        currentGraphicsState.StrokeColorSpace = colorSpaceManager.GetColorSpace(PdfKeys.DeviceCMYK);
                        currentGraphicsState.StrokeBrush =
                            currentGraphicsState.StrokeColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                       currentGraphicsState.CurrentTransformationMatrix,
                                                                                       currentGraphicsState.StrokeAlpha.Current);
                        break;
                    }

                    // set stroked color
                    case OpCodeName.SC:
                    {
                        currentGraphicsState.StrokeBrush =
                            currentGraphicsState.StrokeColorSpace.GetBrushDescriptor(contentOperator.Operands,
                                                                                       currentGraphicsState.CurrentTransformationMatrix,
                                                                                       currentGraphicsState.StrokeAlpha.Current);
                        break;
                    }

                    // shading
                    case OpCodeName.sh:
                    {
                        if (isVisible)
                        {
                            var graphicPath = new GraphicPath();
                            var shadingDescriptor = shadingManager.GetShading(contentOperator.Operands);

                            graphicPath.Geometry = currentGraphicsState.ClippingPath;
                            graphicPath.FillBrush = shadingDescriptor.GetBrush(currentGraphicsState.CurrentTransformationMatrix,
                                                                                               currentGraphicsState.ClippingPath.Bounds,
                                                                                               currentGraphicsState.FillAlpha.Current,
                                                                                               currentGraphicsState.SoftMask);
                            graphicPath.ColorPrecision = shadingDescriptor.ColorPrecision;

                            graphicGroup.Children.Add(graphicPath);
                        }
                        break;
                    }

                    // begin text
                    case OpCodeName.BT:
                    {
                        fontState.TextLineMatrix = Matrix.Identity;
                        fontState.TextMatrix = Matrix.Identity;
                        break;
                    }

                    // set current font
                    case OpCodeName.Tf:
                    {
                        var fontName = ((CName)contentOperator.Operands[0]).Name;
                        fontState.FontSize = PdfUtilities.GetDouble(contentOperator.Operands[1]);
                        fontState.FontDescriptor = fontManager.GetFont(fontName);
                        break;
                    }

                    // set rendering mode
                    case OpCodeName.Tr:
                    {
                        fontState.RenderingMode = PdfUtilities.GetRenderingMode(contentOperator.Operands[0]);
                        break;
                    }

                    // set font transformation matrix
                    case OpCodeName.Tm:
                    {
                        var matrix = PdfUtilities.GetMatrix(contentOperator.Operands);
                        fontState.TextMatrix = matrix;
                        fontState.TextLineMatrix = matrix;
                        break;
                    }

                    // translate line matrix
                    case OpCodeName.Td:
                    {
                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);

                        // for some unknown reason the simple next statement doesn't
                        // work, do it in small steps instead
                        //fontState.TextLineMatrix.TranslatePrepend(x, y);
                        var m = fontState.TextLineMatrix;
                        m.TranslatePrepend(x, y);
                        fontState.TextLineMatrix = m;

                        fontState.TextMatrix = fontState.TextLineMatrix;
                        break;
                    }

                    case OpCodeName.TD:
                    {
                        var x = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        var y = PdfUtilities.GetDouble(contentOperator.Operands[1]);

                        //fontState.TextLineMatrix.Translate(x, y);
                        var m = fontState.TextLineMatrix;
                        m.TranslatePrepend(x, y);
                        fontState.TextLineMatrix = m;

                        fontState.Leading = -y;
                        fontState.TextMatrix = fontState.TextLineMatrix;
                        break;
                    }

                    case OpCodeName.TL:
                    {
                        fontState.Leading = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        break;
                    }

                    case OpCodeName.Tx:
                    {
                        var m = fontState.TextLineMatrix;
                        m.TranslatePrepend(0, -fontState.Leading);
                        fontState.TextLineMatrix = m;

                        fontState.TextMatrix = fontState.TextLineMatrix;
                        break;
                    }

                    case OpCodeName.QuoteSingle:
                    {
                        break;
                    }

                    case OpCodeName.QuoteDbl:
                    {
                        break;
                    }

                    // a single string
                    case OpCodeName.Tj:
                    {
                        var text = (CString)contentOperator.Operands[0];
                        var textGraphic = textVectorizer.Vectorize(text.Value, currentGraphicsState, fontState);

                        if (isVisible)
                        {
                            graphicGroup.Children.AddRange(textGraphic);
                        }

                        break;
                    }

                    // multiple strings plus formatting
                    case OpCodeName.TJ:
                    {
                        var array = (CArray)contentOperator.Operands[0];
                        HandleMultipleTextCommand(array, isVisible);
                        break;
                    }

                    // graphics state operators
                    // push state onto the stack
                    case OpCodeName.q:
                    {
                        var clone = currentGraphicsState.Clone();
                        graphicsStateStack.Push(clone);
                        break;
                    }

                    // pop state from the stack
                    case OpCodeName.Q:
                    {
                        currentGraphicsState = graphicsStateStack.Pop();
                        break;
                    }

                    // current transform matrix
                    case OpCodeName.cm:
                    {
                        var matrix = PdfUtilities.GetMatrix(contentOperator.Operands);
                        currentGraphicsState.TransformationMatrix *= matrix;
                        break;
                    }

                    case OpCodeName.J:
                    {
                        currentGraphicsState.LineCap = GetLineCap(((CInteger)contentOperator.Operands[0]).Value);
                        break;
                    }

                    case OpCodeName.j:
                    {
                        currentGraphicsState.LineJoin = GetLineJoin(((CInteger)contentOperator.Operands[0]).Value);
                        break;
                    }

                    case OpCodeName.M:
                    {
                        currentGraphicsState.MiterLimit = PdfUtilities.GetDouble(contentOperator.Operands[0]);
                        break;
                    }

                    case OpCodeName.d:
                    {
                        SetDashPattern(contentOperator.Operands);
                        break;
                    }

                    // line width
                    case OpCodeName.w:
                    {
                        currentGraphicsState.LineWidth = MatrixUtilities.TransformScale(PdfUtilities.GetDouble(contentOperator.Operands[0]), currentGraphicsState.CurrentTransformationMatrix);
                        break;
                    }

                    // set parameters in the current graphic state of the given state name
                    case OpCodeName.gs:
                    {
                        var name = contentOperator.Operands[0] as CName;
                        extendedStatesManager.SetExtendedGraphicState(currentGraphicsState, fontState, name.Name);
                        break;
                    }

                    case OpCodeName.Do:
                    {
                        var xObjectName = ((CName)contentOperator.Operands[0]).Name;
                        RunXObject(xObjectName);
                        break;
                    }

                    default:
                        break;
                }
            }

            return this.returnGraphicGroup;
        }

        /// <summary>
        /// Gets the line cap
        /// </summary>
        /// <returns></returns>
        private GraphicLineCap GetLineCap(int lineCapCode)
        {
            GraphicLineCap lineCap = GraphicLineCap.Flat;

            switch (lineCapCode)
            {
                case 0:
                    lineCap = GraphicLineCap.Flat;
                    break;

                case 1:
                    lineCap = GraphicLineCap.Round;
                    break;

                case 2:
                    lineCap = GraphicLineCap.Square;
                    break;
            }

            return lineCap;
        }

        /// <summary>
        /// Get the line join method
        /// </summary>
        /// <returns></returns>
        private GraphicLineJoin GetLineJoin(int lineJoinCode)
        {
            GraphicLineJoin lineJoin = GraphicLineJoin.Miter;

            switch (lineJoinCode)
            {
                case 0:
                    lineJoin = GraphicLineJoin.Miter;
                    break;

                case 1:
                    lineJoin = GraphicLineJoin.Round;
                    break;

                case 2:
                    lineJoin = GraphicLineJoin.Bevel;
                    break;
            }

            return lineJoin;
        }

        /// <summary>
        /// Sets the dash pattern
        /// </summary>
        private void SetDashPattern(CSequence operands)
        {
            var dashArray = (CArray)operands[0];
            var dashPhase = PdfUtilities.GetDouble(operands[1]);

            if (dashArray.Count == 0)
            {
                currentGraphicsState.Dashes = null;
                currentGraphicsState.DashOffset = 0;
                return;
            }

            var dashes = new List<double>();

            foreach (var val in dashArray)
            {
                var dbl = MatrixUtilities.TransformScale(PdfUtilities.GetDouble(val), currentGraphicsState.CurrentTransformationMatrix);
                dashes.Add(dbl);
            }

            currentGraphicsState.Dashes = dashes;
            currentGraphicsState.DashOffset = dashPhase;
        }

        /// <summary>
        /// Handle multiple strings command
        /// </summary>
        private void HandleMultipleTextCommand(CArray commandParameter, bool isVisible)
        {
            foreach (var parameter in commandParameter)
            {
                switch (parameter)
                {
                    case CString str:
                    {
                        var textGraphic = textVectorizer.Vectorize(str.Value, currentGraphicsState, fontState);

                        if (isVisible)
                        {
                            graphicGroup.Children.AddRange(textGraphic);
                        }

                        break;
                    }

                    case CInteger integer:
                    {
                        textVectorizer.AdjustPosition(integer.Value, fontState);
                        break;
                    }

                    case CReal real:
                    {
                        textVectorizer.AdjustPosition(real.Value, fontState);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Run the XObject with the given name
        /// </summary>
        private void RunXObject(string name)
        {
            var xobjectDict = xObjectManager.GetXObject(name);
            var subType = xobjectDict.Elements.GetName(PdfKeys.Subtype);

            if (subType != PdfKeys.Form)
            {
                return;
            }

            var cloneCurrentGraphicsState = currentGraphicsState.Clone();

            var matrixArray = xobjectDict.Elements.GetArray(PdfKeys.Matrix);
            Matrix matrix = Matrix.Identity;

            if (matrixArray != null)
            {
                matrix = PdfUtilities.GetMatrix(matrixArray);
                cloneCurrentGraphicsState.TransformationMatrix *= matrix;
            }

            cloneCurrentGraphicsState.FillAlpha.Layer = 1.0;
            cloneCurrentGraphicsState.FillAlpha.Object = 1.0;

            cloneCurrentGraphicsState.StrokeAlpha.Layer = 1.0;
            cloneCurrentGraphicsState.StrokeAlpha.Object = 1.0;

            cloneCurrentGraphicsState.SoftMask = null;

            CSequence sequence = ContentReader.ReadContent(xobjectDict.Stream.UnfilteredValue);

            var interpreter = new ContentInterpreter();
            var group = interpreter.Run(xobjectDict, sequence, cloneCurrentGraphicsState, null);

            // do some optimizations that the post-processor cannot do

            if (group.Children.Count == 1 && group.Clip == null && !DoubleUtilities.IsEqual(currentGraphicsState.FillAlpha.Object, 1.0))
            {
                // the layer has only 1 child and the layer has an opacity set other than 1 -> 
                // recreate the layer but with the layer opacity set which gets "added" to each single object
                // on that layer. Because there is only 1 object the result is the same as if the object sits
                // on a semi transparent layer. That saves a group for a single object.

                cloneCurrentGraphicsState = currentGraphicsState.Clone();

                if (matrixArray != null)
                {
                    cloneCurrentGraphicsState.TransformationMatrix *= matrix;
                }

                cloneCurrentGraphicsState.FillAlpha.Layer = currentGraphicsState.FillAlpha.Object;
                cloneCurrentGraphicsState.StrokeAlpha.Layer = currentGraphicsState.StrokeAlpha.Object;

                group = interpreter.Run(xobjectDict, sequence, cloneCurrentGraphicsState, null);
                graphicGroup.Children.Add(group.Children[0]);
            }
            else
            {
                group.Opacity = currentGraphicsState.FillAlpha.Object;
                graphicGroup.Children.Add(group);
            }
        }

        /// <summary>
        /// Reset the current path
        /// </summary>
        private void ResetCurrentGeometry()
        {
            currentGeometry = null;
        }

        /// <summary>
        /// Build up a path from the current geometry and the current fill
        /// </summary>
        private GraphicPath GetCurrentPathFilled()
        {
            var path = new GraphicPath();
            path.Geometry = currentGeometry;

            path.FillBrush = currentGraphicsState.FillBrush.GetBrush(currentGeometry.Bounds, currentGraphicsState.SoftMask);
            path.ColorPrecision = currentGraphicsState.FillBrush.ColorPrecision;

            return path;
        }

        /// <summary>
        /// Build up a path from the current geometry and the current stroke
        /// </summary>
        private GraphicPath GetCurrentPathStroked()
        {
            var path = new GraphicPath();
            path.Geometry = currentGeometry;

            path.StrokeThickness = currentGraphicsState.LineWidth;
            path.StrokeBrush = currentGraphicsState.StrokeBrush.GetBrush(currentGeometry.Bounds, currentGraphicsState.SoftMask);
            path.StrokeLineCap = currentGraphicsState.LineCap;
            path.StrokeLineJoin = currentGraphicsState.LineJoin;

            SetFinalDashes(path);

            path.ColorPrecision = currentGraphicsState.StrokeBrush.ColorPrecision;

            return path;
        }

        /// <summary>
        /// Build up a path from the current geometry and the current fill and stroke
        /// </summary>
        private GraphicPath GetCurrentPathFilledAndStroked()
        {
            var path = new GraphicPath();
            path.Geometry = currentGeometry;

            path.FillBrush = currentGraphicsState.FillBrush.GetBrush(currentGeometry.Bounds, currentGraphicsState.SoftMask);

            path.StrokeThickness = currentGraphicsState.LineWidth;
            path.StrokeBrush = currentGraphicsState.StrokeBrush.GetBrush(currentGeometry.Bounds, currentGraphicsState.SoftMask);
            path.StrokeLineCap = currentGraphicsState.LineCap;
            path.StrokeLineJoin = currentGraphicsState.LineJoin;
            path.StrokeMiterLimit = currentGraphicsState.MiterLimit;

            SetFinalDashes(path);

            path.ColorPrecision = EvalWeakestColorPrecision(currentGraphicsState.FillBrush.ColorPrecision, currentGraphicsState.StrokeBrush.ColorPrecision);

            return path;
        }

        /// <summary>
        /// Set the dashes of the final path
        /// </summary>
        /// <param name="path"></param>
        private void SetFinalDashes(GraphicPath path)
        {
            if (currentGraphicsState.Dashes == null)
            {
                return;
            }

            var relativeDashes = new List<double>();
            var thickness = currentGraphicsState.LineWidth;

            foreach (var dash in currentGraphicsState.Dashes)
            {
                var relativeDash = dash / thickness;
                relativeDashes.Add(relativeDash);
            }

            path.StrokeDashes = relativeDashes;
            path.StrokeDashOffset = currentGraphicsState.DashOffset / thickness;
        }

        /// <summary>
        /// Compute the weakest color precision
        /// </summary>
        private GraphicColorPrecision EvalWeakestColorPrecision(GraphicColorPrecision currrentColorPrecision, GraphicColorPrecision colorPrecisionToAdd)
        {
            return colorPrecisionToAdd > currrentColorPrecision ? colorPrecisionToAdd : currrentColorPrecision;
        }
    }
}
