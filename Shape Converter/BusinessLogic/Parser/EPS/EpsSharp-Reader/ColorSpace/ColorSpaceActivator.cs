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

using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// ColorSpace activator
    /// </summary>
    internal static class ColorSpaceActivator
    {
        /// <summary>
        /// Create a color space
        /// </summary>
        public static IColorSpace CreateColorSpace(EpsInterpreter interpreter, Operand colorSpaceOperand)
        {
            string colorSpaceName = null;
            ArrayOperand colorSpaceDetails = null;

            switch (colorSpaceOperand)
            {
                case NameOperand nameOperand:
                {
                    colorSpaceName = nameOperand.Value;
                    break;
                }

                case ArrayOperand arrayOperand:
                {
                    colorSpaceName = OperandHelper.GetStringValue(arrayOperand.Values[0].Operand);
                    colorSpaceDetails = arrayOperand;
                    break;
                }
            }

            return CreateColorSpace(interpreter, colorSpaceName, colorSpaceDetails);
        }

        /// <summary>
        /// Create a color space
        /// </summary>
        public static IColorSpace CreateColorSpace(EpsInterpreter interpreter, string colorSpaceName, ArrayOperand colorSpaceDetails = null)
        {
            IColorSpace colorSpace = null;

            switch (colorSpaceName)
            {
                case EpsKeys.DeviceRGB:
                {
                    colorSpace = new RGBDeviceColorSpace();
                    break;
                }

                case EpsKeys.DeviceCMYK:
                {
                    colorSpace = new CMYKDeviceColorSpace();
                    break;
                }

                case EpsKeys.DeviceGray:
                {
                    colorSpace = new GrayDeviceColorSpace();
                    break;
                }

                case EpsKeys.DevicePattern:
                {
                    colorSpace = new PatternColorSpace();
                    break;
                }

                case EpsKeys.Separation:
                case EpsKeys.DeviceN:
                {
                    colorSpace = new DeviceNColorSpace();
                    break;
                }

                case EpsKeys.Indexed:
                {
                    colorSpace = new IndexedColorSpace();
                    break;
                }

                case EpsKeys.CIEBasedABC:
                {
                    colorSpace = new UnknownColorSpace(3);
                    break;
                }

                case EpsKeys.CIEBasedA:
                {
                    colorSpace = new UnknownColorSpace(1);
                    break;
                }

                default:
                {
                    colorSpace = new UnknownColorSpace(1);
                    break;
                }
            }

            colorSpace.Init(interpreter, colorSpaceDetails);

            return colorSpace;
        }
    }
}
