// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Caliburn.Micro;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class EditStudentInfoViewModel:Screen
    {
        private StudentModel _currentStudent;

        public delegate EditStudentInfoViewModel Factory(StudentModel model);
        public StudentModel CurrentStudent
        {
            get=>_currentStudent;
            set
            {
                _currentStudent=value;
                NotifyOfPropertyChange(nameof(CurrentStudent));
            }
        }

        public EditStudentInfoViewModel(StudentModel model)
        {
            CurrentStudent = model;
        }

        public void Cancel()
        {
            
        }

        public void Add()
        {

        }
    }
}
