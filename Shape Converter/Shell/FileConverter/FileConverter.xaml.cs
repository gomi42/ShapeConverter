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

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ShapeConverter.Shell.MVVM;

namespace ShapeConverter
{
    /// <summary>
    /// Interaction logic for IllustratorConverter.xaml
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

        /// <summary>
        /// Handle DataContext changed
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var triggerResetViewProp = DataContext.GetType().GetProperty("TriggerResetView", BindingFlags.Public | BindingFlags.Instance);

            if (triggerResetViewProp == null || triggerResetViewProp.PropertyType != typeof(ITrigger))
            {
                return;
            }

            var command = new DelegateTrigger(OnReset);
            triggerResetViewProp.SetValue(DataContext, command, null);
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

        /// <summary>
        /// Handle view reset request
        /// We move up the preview shape list to the first entry
        /// </summary>
        private void OnReset()
        {
            var enumerator = ShapeSelectionBox.ItemsSource.GetEnumerator();
            enumerator.Reset();

            if (enumerator.MoveNext())
            {
                ShapeSelectionBox.ScrollIntoView(enumerator.Current);
            }
        }
    }
}
