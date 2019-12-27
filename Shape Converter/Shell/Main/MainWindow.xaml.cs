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
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace ShapeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly FileConverter illustratorConverter;
        readonly StreamConversion.StreamConverter streamConverter;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            illustratorConverter = new FileConverter();
            streamConverter = new StreamConversion.StreamConverter();

            ButtonFile.IsChecked = true;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            Copyright.Text = string.Format("© Michael Göricke v{0}.{1}.{2}", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
        }

        /// <summary>
        /// The file tab is checked.
        /// </summary>
        private void ButtonFileChecked(object sender, RoutedEventArgs e)
        {
            if (ConverterView == null)
            {
                return;
            }

            ConverterView.Content = illustratorConverter;
        }

        /// <summary>
        /// The stream tab is checked.
        /// </summary>
        private void ButtonStreamChecked(object sender, RoutedEventArgs e)
        {
            if (ConverterView == null)
            {
                return;
            }

            ConverterView.Content = streamConverter;
        }
    }
}
