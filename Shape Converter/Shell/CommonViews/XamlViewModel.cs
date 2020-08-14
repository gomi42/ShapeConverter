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
using System.Windows.Input;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter.Shell.CommonViews
{
    /// <summary>
    /// Code type
    /// </summary>
    public enum XamlCodeGeneratorType
    {
        DrawingBrush,
        Path
    }

    /// <summary>
    /// One entry of the code type combobox
    /// </summary>
    public class XamlCodeTypeItem
    {
        /// <summary>
        /// The text to display
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The type
        /// </summary>
        public XamlCodeGeneratorType CodeGeneratorType { get; set; }
    }

    /// <summary>
    /// The XamlViewModel
    /// </summary>
    public class XamlViewModel : ViewModelBase
    {
        private GraphicVisual selectedVisual;
        
        private string sourceCode;
        private bool normalize = true;
        private GeometryTypeItem selectedGeometryCreationType;
        private XamlCodeTypeItem selectedCodeTypeItem;
        private string colorPrecisionMessage;

        /// <summary>
        /// Constructor
        /// </summary>
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

        /// <summary>
        /// Trigger to reset the view 
        /// </summary>
        public ITrigger TriggerResetView { get; set; }

        /// <summary>
        /// Normalize the visual
        /// </summary>
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

        /// <summary>
        /// List of code types to create code for
        /// </summary>
        public List<XamlCodeTypeItem> CodeTypeItems { get; set; }

        /// <summary>
        /// Selected code type
        /// </summary>
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

        /// <summary>
        /// List of geometry types to create code for
        /// </summary>
        public List<GeometryTypeItem> GeometryTypes { get; set; }

        /// <summary>
        /// The selected geometry type
        /// </summary>
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

        /// <summary>
        /// The source code
        /// </summary>
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

        /// <summary>
        /// The color precision message
        /// </summary>
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

        /// <summary>
        /// Copy to clipboard command
        /// </summary>
        public DelegateCommand CopyToClipboard { get; set; }

        /// <summary>
        /// Reset the view
        /// </summary>
        public void Reset()
        {
            TriggerResetView.Fire();
        }

        /// <summary>
        /// Set a new visual
        /// </summary>
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
