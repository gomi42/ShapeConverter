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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using ShapeConverter.BusinessLogic.Parser.Pdf.ColorSpace.Indexed;
using ShapeConverter.BusinessLogic.Parser.Pdf.Pattern;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// ColorSpaceManager
    /// </summary>
    internal class ColorSpaceManager
    {
        private Dictionary<string, IColorSpace> colorSpaces;
        private PatternManager patternManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorSpaceManager(PatternManager patternManager)
        {
            this.patternManager = patternManager;
        }

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfResources resourcesDict)
        {
            colorSpaces = new Dictionary<string, IColorSpace>();

            IColorSpace predefinedColorSpace = new RGBDeviceColorSpace();
            colorSpaces.Add(PdfKeys.DeviceRGB, predefinedColorSpace);

            predefinedColorSpace = new CMYKDeviceColorSpace();
            colorSpaces.Add(PdfKeys.DeviceCMYK, predefinedColorSpace);

            predefinedColorSpace = new GrayDeviceColorSpace();
            colorSpaces.Add(PdfKeys.DeviceGray, predefinedColorSpace);

            var predefinedColorSpacePattern = new PatternColorSpace(patternManager);
            colorSpaces.Add(PdfKeys.DevicePattern, predefinedColorSpacePattern);

            if (resourcesDict != null)
            {
                ReadResourceDefinedColorSpaces(resourcesDict);
            }
        }

        /// <summary>
        /// Read all color spaces defined the resources
        /// </summary>
        private void ReadResourceDefinedColorSpaces(PdfResources resourcesDict)
        {
            var colorSpaceDict = resourcesDict.ColorSpaces;

            if (colorSpaceDict == null)
            {
                return;
            }

            foreach (var name in colorSpaceDict.Elements.Keys)
            {
                var colorSpaceArray = colorSpaceDict.Elements.GetArray(name);
                var type = colorSpaceArray.Elements.GetName(0);

                IColorSpace colorSpace = null;

                switch (type)
                {
                    case PdfKeys.DevicePattern:
                    {
                        colorSpace = new PatternColorSpace(patternManager);
                        colorSpace.Init(colorSpaceArray);
                        break;
                    }

                    default:
                        colorSpace = CreateColorSpace(colorSpaceArray);
                        break;
                }

                colorSpaces.Add(name, colorSpace);
            }
        }

        /// <summary>
        /// Get one color space
        /// </summary>
        public static IColorSpace CreateColorSpace(PdfArray colorSpaceArray)
        {
            var colorSpaceName = colorSpaceArray.Elements.GetName(0);
            return CreateColorSpace(colorSpaceName, colorSpaceArray);
        }

        /// <summary>
        /// Get one color space
        /// </summary>
        public static IColorSpace CreateColorSpace(string colorSpaceName)
        {
            return CreateColorSpace(colorSpaceName, null);
        }

        /// <summary>
        /// Get one color space
        /// </summary>
        public static IColorSpace CreateColorSpace(string colorSpaceName, PdfArray colorSpaceArray)
        {
            IColorSpace colorSpace = null;

            switch (colorSpaceName)
            {
                case PdfKeys.DefaultRGB:
                case PdfKeys.DeviceRGB:
                {
                    colorSpace = new RGBDeviceColorSpace();
                    break;
                }

                case PdfKeys.DefaultCMYK:
                case PdfKeys.DeviceCMYK:
                {
                    colorSpace = new CMYKDeviceColorSpace();
                    break;
                }

                case PdfKeys.DefaultGray:
                case PdfKeys.DeviceGray:
                {
                    colorSpace = new GrayDeviceColorSpace();
                    break;
                }

                case PdfKeys.ICCBased:
                {
                    colorSpace = new ICCBasedColorSpace();
                    break;
                }

                case PdfKeys.Indexed:
                {
                    colorSpace = new IndexedColorSpace();
                    break;
                }

                case PdfKeys.DeviceN:
                {
                    colorSpace = new DeviceNColorSpace();
                    break;
                }

                case PdfKeys.CalGray:
                {
                    colorSpace = new UnknownColorSpace(1);
                    break;
                }

                case PdfKeys.CalRGB:
                {
                    colorSpace = new UnknownColorSpace(3);
                    break;
                }

                case PdfKeys.Lab:
                {
                    colorSpace = new UnknownColorSpace(3);
                    break;
                }

                default:
                {
                    colorSpace = new UnknownColorSpace(1);
                    break;
                }
            }

            colorSpace.Init(colorSpaceArray);

            return colorSpace;
        }

        /// <summary>
        /// Get a color space from the given color space name
        /// </summary>
        public IColorSpace GetColorSpace(string colorSpaceName)
        {
            return colorSpaces[colorSpaceName];
        }
    }
}
