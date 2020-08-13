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

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShapeConverter
{
    /// <summary>
    /// Interaction logic for IllustratorConverter.xaml
    /// At the moment we don't follow the MVVM paradigm that is more or less standard for
    /// WPF applications. The main focus of the ShapeConverter are the algorithms. And it
    /// turned out we don't need much UI logic to present the results. 
    /// </summary>
    public partial class FileConverter : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FileConverter()
        {
            InitializeComponent();
            AllowDrop = true;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Binding binding = new Binding();
            binding.Source = DataContext;
            binding.Path = new PropertyPath("ResetView");
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, FileConverter.InitProperty, binding);
        }

        public bool Init
        {
            get
            {
                return (bool)GetValue(InitProperty); 
            }

            set
            {
                SetValue(InitProperty, value); 
            }
        }

        public static readonly DependencyProperty InitProperty =
            DependencyProperty.Register("Init", typeof(bool), typeof(FileConverter), new PropertyMetadata(false, InitChanged));

        private static void InitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FileConverter)d).OnInitChanged();
        }

        /// <summary>
        /// Test whether drag and drop is allowed
        /// </summary>
        protected override void OnDragOver(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 1)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Accept dropping a file
        /// </summary>
        protected override void OnDrop(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1)
            {
                return;
            }

            PropertyInfo prop = DataContext.GetType().GetProperty("Filename", BindingFlags.Public | BindingFlags.Instance);

            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(DataContext, files[0], null);
            }
        }

        private void OnInitChanged()
        {
            if (!Init)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() => Init = false));

            var enumerator = ShapeSelectionBox.ItemsSource.GetEnumerator();
            enumerator.Reset();

            if (enumerator.MoveNext())
            {
                ShapeSelectionBox.ScrollIntoView(enumerator.Current);
            }
        }
    }
}
