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
using System.Windows.Media;
using EpsSharp.Eps.Helper;
using EpsSharp.Eps.Pattern;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The pattern color space
    /// </summary>
    internal class PatternColorSpace :IColorSpace
    {
        private EpsInterpreter interpreter;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Placeholder;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceArray)
        {
            this.interpreter = interpreter;
        }

        /// <summary>
        /// Gets the name of the color space
        /// </summary>
        public ArrayOperand GetColorSpaceName()
        {
            return OperandHelper.CreateColorSpaceDescription(EpsKeys.DevicePattern);
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values)
        {
            byte color = 0x00;
            return Color.FromArgb((byte)255, color, color, color);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        public int GetNumberOfValuesPerColor()
        {
            return 1;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            return new PatternBrushDescriptor(interpreter, operands[0], matrix);
        }
    }

    /// <summary>
    /// The pattern brush Descriptor
    /// </summary>
    internal class PatternBrushDescriptor : IBrushDescriptor
    {
        private DictionaryOperand patternDict;
        private IPattern pattern;
        private Matrix matrix;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => pattern.ColorPrecision;

        /// <summary>
        /// Constructor
        /// </summary>
        public PatternBrushDescriptor(EpsInterpreter interpreter, Operand operand, Matrix matrix)
        {
            patternDict = (DictionaryOperand)operand;
            pattern = PatternActivator.CreatePattern(interpreter, patternDict);

            this.matrix = matrix;
        }

        /// <summary>
        /// Return the color components as operands that define this brush
        /// </summary>
        public List<Operand> GetColorOperands()
        {
            List<Operand> operands = new List<Operand>();
            operands.Add(patternDict);

            return operands;
        }

        /// <summary>
        /// Return the color as gray components operands that define this brush
        /// </summary>
        public List<Operand> GetGrayColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(0);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as RGB components as operands that define this brush
        /// </summary>
        public List<Operand> GetRGBColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            for (int i = 0; i < 3; i++)
            {
                var realOp = new RealOperand(0);
                operands.Add(realOp);
            }

            return operands;
        }

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// </summary>
        public List<Operand> GetCMYKColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(0);
            operands.Add(realOp);
            realOp = new RealOperand(0);
            operands.Add(realOp);
            realOp = new RealOperand(0);
            operands.Add(realOp);
            realOp = new RealOperand(1.0);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(EpsRect boundingBox)
        {
            return pattern.GetBrush(matrix, boundingBox);
        }
    }
}
