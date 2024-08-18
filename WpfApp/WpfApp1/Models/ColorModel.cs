// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Caliburn.Micro;

namespace WpfApp1.Models
{
    public class ColorModel : Screen
    {
        private string _color;
        private string _name;

        public ColorModel(string color, string name)
        {
            _color = color;
            _name = name;
        }

        public string Color
        {
            get => _color;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _color = value;
                    NotifyOfPropertyChange(nameof(Color));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrEmpty(_name))
                {
                    _name = value;
                    NotifyOfPropertyChange(nameof(Name));
                }
            }
        }
    }
}
