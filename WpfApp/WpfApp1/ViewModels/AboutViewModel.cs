// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Windows.Controls;
using Caliburn.Micro;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class AboutViewModel:Screen
    {
        private ColorModel _selectedColor;
        public ObservableCollection<ColorModel> Colors { get; set; }

        public ColorModel SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                NotifyOfPropertyChange(nameof(SelectedColor));
            }
        }

        public void OnSelectedColorChanged(SelectionChangedEventArgs e)
        { 

        }

        public AboutViewModel()
        {
            Colors=new ObservableCollection<ColorModel>();
            Colors.Add(new ColorModel("Red","Red"));
            Colors.Add(new ColorModel("Green","Green"));
            Colors.Add(new ColorModel("Blue","Blue"));
            Colors.Add(new ColorModel("White","White"));
        }
    }
}
