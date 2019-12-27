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

using PdfSharp.Pdf;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Function
{
    /// <summary>
    /// The function manager
    /// </summary>
    internal class FunctionManager
    {
        public static IFunction ReadFunction(PdfDictionary functionDict)
        {
            IFunction function;

            var functionType = functionDict.Elements.GetInteger(PdfKeys.FunctionType);

            switch (functionType)
            {
                // exponential
                case 2:
                    function = new ExponentialFunction();
                    break;

                // stitching
                case 3:
                    function = new StitchingFunction();
                    break;

                // postscript calculator
                case 4:
                    function = new PostScriptCalculatorFunction();
                    break;

                // sampled
                case 0:
                default:
                    function = new UnknownFunction();
                    break;
            }

            function.Init(functionDict);

            return function;
        }
    }
}
