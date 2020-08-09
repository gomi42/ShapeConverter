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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShapeConverter;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator;
using ShapeConverter.Parser.StreamGeometry;
using ShapeConverter.Shell.Exporter;

namespace StreamConversion
{
    /// <summary>
    /// Interaction logic for StreamConverter.xaml
    /// See FileConverter.xaml.cs about using MVVM.
    /// </summary>
    public partial class StreamConverter : UserControl
    {
        private GraphicPath selectedPath;

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamConverter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle modified input stream
        /// </summary>
        private void OnStreamTextChanged(object sender, TextChangedEventArgs e)
        {
            CreatePaths(Stream.Text);
        }

        /// <summary>
        /// Parse and display the new input stream
        /// </summary>
        private void CreatePaths(string stream)
        {
            try
            {
                var streamGeometryParser = new StreamGeometryParser();
                var graphicPathGeometry = streamGeometryParser.ParseGeometry(stream);

                var path = new GraphicPath();
                selectedPath = path;
                path.Geometry = graphicPathGeometry;
                path.FillBrush = new GraphicSolidColorBrush { Color = Color.FromRgb(128, 128, 128) };

                ErrorIndicator.Visibility = Visibility.Collapsed;

                if (path.Geometry.Segments.Count > 0)
                {
                    ExportButton.IsEnabled = true;
                }
                else
                {
                    DisableExportButtons();
                }
            }
            catch
            {
                selectedPath = null;
                ErrorIndicator.Visibility = Visibility.Visible;
                DisableExportButtons();
            }

            UpdateAll();
        }

        /// <summary>
        /// Disable all elements
        /// </summary>
        private void DisableExportButtons()
        {
            ExportButton.IsEnabled = false;
        }

        /// <summary>
        /// Update all code generators
        /// </summary>
        private void UpdateAll()
        {
            if (StreamCode == null || DrawingBrushCode == null || Preview == null)
            {
                return;
            }

            var path = selectedPath;

            if (path == null)
            {
                Preview.Data = null;
                StreamCode.Text = string.Empty;
                DrawingBrushCode.Text = string.Empty;
                GeometryCode.Text = string.Empty;
                return;
            }

            var geometry = GeometryBinaryGenerator.GenerateGeometry(path.Geometry);
            Preview.Data = geometry;
            UpdatePreviewAll();

            UpdateStreamSourceCode();
            UpdateDrawingBrushSourceCode();

            UpdateGeometrySourceCode();
        }

        /// <summary>
        /// Update the preview
        /// </summary>
        private void UpdatePreviewAll()
        {
            UpdatePreviewFill();
            UpdatePreviewStroke();
        }

        /// <summary>
        /// Update the fill of the preview
        /// </summary>
        private void UpdatePreviewFill()
        {
            if (Preview == null)
            {
                return;
            }

            if (PreviewShowComboBox.SelectedIndex == 0 || PreviewShowComboBox.SelectedIndex == 2)
            {
                Preview.Fill = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
            }
            else
            {
                Preview.Fill = null;
            }
        }

        /// <summary>
        /// Update the stroke of the preview
        /// </summary>
        private void UpdatePreviewStroke()
        {
            if (Preview == null)
            {
                return;
            }

            if (PreviewShowComboBox.SelectedIndex == 1 || PreviewShowComboBox.SelectedIndex == 2)
            {
                Preview.Stroke = new SolidColorBrush(Color.FromRgb(0x0, 0x0, 0x0));
                Preview.StrokeThickness = 2;
            }
            else
            {
                Preview.Stroke = null;
                Preview.StrokeThickness = 0;
            }
        }

        /// <summary>
        /// Update the resource source code
        /// </summary>
        private void UpdateStreamSourceCode()
        {
            if (StreamCode == null)
            {
                return;
            }

            var path = selectedPath;

            if (path == null)
            {
                Preview.Data = null;
                StreamCode.Text = string.Empty;
                DrawingBrushCode.Text = string.Empty;
                GeometryCode.Text = string.Empty;
                return;
            }

            if (NormalizeCheckBox.IsChecked == true)
            {
                var normalizer = new NormalizeVisual();
                path = (GraphicPath)normalizer.Normalize(selectedPath, NormalizeAspect.Both, 100);
            }

            string xamlStream;

            if (TypeComboBox.SelectedIndex == 0)
            {
                var streams = StreamSourceGenerator.GenerateStreamGeometries(path);
                xamlStream = string.Join("\n", streams);
            }
            else
            if (TypeComboBox.SelectedIndex == 1)
            {
                xamlStream = StreamSourceGenerator.GenerateGeometry(path);
            }
            else
            {
                xamlStream = StreamSourceGenerator.GeneratePathGeometry(path);
            }

            StreamCode.Text = xamlStream;
        }

        /// <summary>
        /// Update the XAML source code
        /// </summary>
        private void UpdateDrawingBrushSourceCode()
        {
            if (DrawingBrushCode == null)
            {
                return;
            }

            var path = selectedPath;

            if (path == null)
            {
                DrawingBrushCode.Text = string.Empty;
                return;
            }

            if (DrawingBrushNormalizeCheckBox.IsChecked == true)
            {
                var normalizer = new NormalizeVisual();
                path = (GraphicPath)normalizer.Normalize(selectedPath, NormalizeAspect.Both, 100);
            }

            bool generatePathFigure = TypeComboBoxDrawingBrush.SelectedIndex == 1;

            if (CodeComboBoxDrawingBrush.SelectedIndex == 0)
            {
                DrawingBrushCode.Text = DrawingBrushSourceGenerator.Generate(path, generatePathFigure);
            }
            else
            {
                DrawingBrushCode.Text = StreamSourceGenerator.GeneratePath(path, generatePathFigure);
            }
        }

        /// <summary>
        /// Update the geometry source code
        /// </summary>
        private void UpdateGeometrySourceCode()
        {
            if (selectedPath == null || CreationComboBox == null)
            {
                return;
            }

            IGeometrySourceGenerator geometrySourceGenerator;

            if (CreationComboBox.SelectedIndex == 0)
            {
                geometrySourceGenerator = new StreamGeometrySourceGenerator();
            }
            else
            {
                geometrySourceGenerator = new PathGeometrySourceGenerator();
            }

            geometrySourceGenerator.IncludeOffset = AddLeftTopCheckBox.IsChecked == true;

            if (ParameterComboBox.SelectedIndex == 0)
            {
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Height;
            }
            else
            if (ParameterComboBox.SelectedIndex == 1)
            {
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Width;
            }
            else
                geometrySourceGenerator.NormalizeAspect = NormalizeGeometrySourceAspect.Individual;

            GeometryCode.Text = geometrySourceGenerator.GenerateSource(selectedPath);
        }

        /// <summary>
        /// Normalize checkbox handler
        /// </summary>
        private void OnNormalizeCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            UpdateStreamSourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStreamSourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void OnDrawingBrushNormalizeCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            UpdateDrawingBrushSourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void OnDrawingBrushCodeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDrawingBrushSourceCode();
        }

        /// <summary>
        /// Type ComboBox selection change handler
        /// </summary>
        private void OnDrawingBrushTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDrawingBrushSourceCode();
        }

        /// <summary>
        /// Checkbox handler
        /// </summary>
        private void OnUpdateGeometrySourceCode(object sender, RoutedEventArgs e)
        {
            UpdateGeometrySourceCode();
        }

        /// <summary>
        /// Handler for all settings that change the source code generation method
        /// </summary>
        private void ParameterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGeometrySourceCode();
        }

        /// <summary>
        /// Show ComboBox selection change handler
        /// </summary>
        private void PreviewShowSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreviewAll();
        }

        /// <summary>
        /// Validate the width entry
        /// </summary>
        private void ExportWidthChanged(object sender, TextChangedEventArgs e)
        {
            if (ExportButton == null)
            {
                return;
            }

            if (int.TryParse(WidthTextBox.Text, out int width))
            {
                ExportButton.IsEnabled = true;
                WidthEntryErrorIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExportButton.IsEnabled = false;
                WidthEntryErrorIndicator.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Export the shapes
        /// </summary>
        private void ExportClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthTextBox.Text, out int width))
            {
                return;
            }

            VisualExporter.Export(selectedPath, width, MarginCheckBox.IsChecked == true, out string message);
        }

        /// <summary>
        /// Copy stream code to the clipboard
        /// </summary>
        private void OnCopyStreamToClipBoard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(StreamCode.Text);
        }

        /// <summary>
        /// Copy drawing brush code to the clipboard
        /// </summary>
        private void OnCopyDrawingBrushToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DrawingBrushCode.Text);
        }

        /// <summary>
        /// Copy geometr C# code to the clipboard
        /// </summary>
        private void OnCopyGeometryToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GeometryCode.Text);
        }
    }
}
