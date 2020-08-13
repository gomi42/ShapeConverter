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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShapeConverter.Shell.CommonViews
{
    /// <summary>
    /// Interaction logic for ResourceView.xaml
    /// </summary>
    public partial class ResourceView : UserControl
    {
        public ResourceView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Binding binding = new Binding();
            binding.Source = DataContext;
            binding.Path = new PropertyPath("ResetView");
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ResourceView.InitProperty, binding);
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
            DependencyProperty.Register("Init", typeof(bool), typeof(ResourceView), new PropertyMetadata(false, InitChanged));

        private static void InitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ResourceView)d).OnInitChanged();
        }

        private void OnInitChanged()
        {
            if (!Init)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() => Init = false));

            SourceCode.ScrollToHome();
        }
    }
}
