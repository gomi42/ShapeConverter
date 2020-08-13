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

using System.Collections.Generic;
using System.Windows;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    public enum XamlCodeGeneratorType
    {
        DrawingBrush,
        Path
    }

    public class XamlCodeTypeItem
    {
        public string Label { get; set; }

        public XamlCodeGeneratorType CodeGeneratorType { get; set; }
    }

    public class XamlViewModel : ViewModelBase
    {
        private GraphicVisual selectedVisual;
        
        private string sourceCode;
        private bool normalize = true;
        private GeometryTypeItem selectedGeometryCreationType;
        private XamlCodeTypeItem selectedCodeTypeItem;
        private string colorPrecisionMessage;
        private bool resetView;

        public XamlViewModel()
        {
            var xamlCodeTypeItems = new List<XamlCodeTypeItem>();
            xamlCodeTypeItems.Add(new XamlCodeTypeItem { Label = "DrawingBrush", CodeGeneratorType = XamlCodeGeneratorType.DrawingBrush });
            xamlCodeTypeItems.Add(new XamlCodeTypeItem { Label = "Path", CodeGeneratorType = XamlCodeGeneratorType.Path });
            CodeTypeItems = xamlCodeTypeItems;
            SelectedCodeTypeItem = CodeTypeItems[0];

            var geometrytypes = new List<GeometryTypeItem>();
            geometrytypes.Add(new GeometryTypeItem { Label = "Stream", GeometryGeneratorType = GeometryGeneratorType.Stream });
            geometrytypes.Add(new GeometryTypeItem { Label = "PathGeometry", GeometryGeneratorType = GeometryGeneratorType.PathGeometry });
            GeometryTypes = geometrytypes;
            SelectedGeometryType = GeometryTypes[0];

            CopyToClipboard = new DelegateCommand(OnCopySourceCodeToClipboard);
        }

        public bool ResetView
        {
            get
            {
                return resetView;
            }

            set
            {
                resetView = value;
                NotifyPropertyChanged();
            }
        }

        public bool Normalize
        {
            get
            {
                return normalize;
            }

            set
            {
                normalize = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }

        public List<XamlCodeTypeItem> CodeTypeItems { get; set; }

        public XamlCodeTypeItem SelectedCodeTypeItem
        {
            get
            {
                return selectedCodeTypeItem;
            }

            set
            {
                selectedCodeTypeItem = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }


        public List<GeometryTypeItem> GeometryTypes { get; set; }

        public GeometryTypeItem SelectedGeometryType
        {
            get
            {
                return selectedGeometryCreationType;
            }

            set
            {
                selectedGeometryCreationType = value;
                NotifyPropertyChanged();
                UpdateSourceCode();
            }
        }

        public string SourceCode
        {
            get
            {
                return sourceCode;
            }

            set
            {
                sourceCode = value;
                NotifyPropertyChanged();
            }
        }

        public string ColorPrecisionMessage
        {
            get
            {
                return colorPrecisionMessage;
            }

            set
            {
                colorPrecisionMessage = value;
                NotifyPropertyChanged();
            }
        }

        public DelegateCommand CopyToClipboard { get; set; }

        public void Reset()
        {
            ResetView = true;
        }

        public void SetNewGraphicVisual(GraphicVisual visual, GraphicColorPrecision? colorPrecision = null)
        {
            selectedVisual = visual;

            if (visual == null)
            {
                ColorPrecisionMessage = null;
                SourceCode = null;
            }

            ColorPrecisionMessage = Helper.GetColorPrecisionText(colorPrecision);
            UpdateSourceCode();
        }

        /// <summary>
        /// Update the XAML source code
        /// </summary>
        private void UpdateSourceCode()
        {
            var path = selectedVisual;

            if (path == null)
            {
                return;
            }

            if (Normalize)
            {
                var normalizer = new NormalizeVisual();
                path = normalizer.Normalize(selectedVisual, NormalizeAspect.Both, 100);
            }

            var geometryGeneratorType = SelectedGeometryType.GeometryGeneratorType;

            switch (SelectedCodeTypeItem.CodeGeneratorType)
            {
                case XamlCodeGeneratorType.DrawingBrush:
                    SourceCode = DrawingBrushSourceGenerator.Generate(path, geometryGeneratorType);
                    break;

                case XamlCodeGeneratorType.Path:
                    SourceCode = PathSourceGenerator.GeneratePath(path, geometryGeneratorType);
                    break;
            }
        }

        /// <summary>
        /// Copy code to the clipboard
        /// </summary>
        private void OnCopySourceCodeToClipboard()
        {
            Clipboard.SetText(SourceCode);
        }
    }
}
