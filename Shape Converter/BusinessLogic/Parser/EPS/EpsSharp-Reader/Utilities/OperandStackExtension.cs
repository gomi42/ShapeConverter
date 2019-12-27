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
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Helper
{
    /// <summary>
    /// Operand stack extensions
    /// </summary>
    internal static class OperandStackExtension
    {
        /// <summary>
        /// Pop a boolean operand from the stack
        /// </summary>
        public static BooleanOperand PopBoolean(this EpsStack<Operand> stack)
        {
            return (BooleanOperand)stack.Pop();
        }

        /// <summary>
        /// Pop a string operand from the stack
        /// </summary>
        public static StringOperand PopString(this EpsStack<Operand> stack)
        {
            return (StringOperand)stack.Pop();
        }

        /// <summary>
        /// Pop a name operand from the stack
        /// </summary>
        public static NameOperand PopName(this EpsStack<Operand> stack)
        {
            return (NameOperand)stack.Pop();
        }

        /// <summary>
        /// Pop an integer operand from the stack
        /// </summary>
        public static IntegerOperand PopInteger(this EpsStack<Operand> stack)
        {
            return (IntegerOperand)stack.Pop();
        }

        /// <summary>
        /// Pop an array operand from the stack
        /// </summary>
        public static ArrayOperand PopArray(this EpsStack<Operand> stack)
        {
            return (ArrayOperand)stack.Pop();
        }

        /// <summary>
        /// Peek an array operand from the stack
        /// </summary>
        public static ArrayOperand PeekArray(this EpsStack<Operand> stack)
        {
            return (ArrayOperand)stack.Peek();
        }

        /// <summary>
        /// Pop a Dictionary operand from the stack
        /// </summary>
        public static DictionaryOperand PopDictionary(this EpsStack<Operand> stack)
        {
            return (DictionaryOperand)stack.Pop();
        }

        /// <summary>
        /// Pop a file operand from the stack
        /// </summary>
        public static FileOperand PopFile(this EpsStack<Operand> stack)
        {
            return (FileOperand)stack.Pop();
        }

        /// <summary>
        /// Pop a string value from the stack
        /// </summary>
        public static string PopStringValue(this EpsStack<Operand> stack)
        {
            return OperandHelper.GetStringValue(stack.Pop());
        }

        /// <summary>
        /// Pop an integer value from the stack
        /// </summary>
        public static int PopIntegerValue(this EpsStack<Operand> stack)
        {
            return ((IntegerOperand)stack.Pop()).Value;
        }

        /// <summary>
        /// Pop a real or integer operand from the stack and return 
        /// its value as double.
        /// </summary>
        /// <returns>a double value</returns>
        public static double PopRealValue(this EpsStack<Operand> stack)
        {
            return OperandHelper.GetRealValue(stack.Pop());
        }

        /// <summary>
        /// Move top of operands stack into a list
        /// </summary>
        public static List<Operand> PopToList(this EpsStack<Operand> stack, int count)
        {
            var operands = new List<Operand>();

            for (int i = 0; i < count; i++)
            {
                operands.Add(stack.Pop());
            }

            operands.Reverse();

            return operands;
        }

        /// <summary>
        /// Push list of operands onto the operands stack
        /// </summary>
        public static void PushFromList(this EpsStack<Operand> stack, List<Operand> operands)
        {
            foreach (var operand in operands)
            {
                stack.Push(operand);
            }
        }
    }
}
