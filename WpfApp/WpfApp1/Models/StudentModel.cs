// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace WpfApp1.Models
{
    public class StudentModel:Screen
    {
        private int _id;
        private string _name;
        private string _email;
        private string _phone;

        public StudentModel(int id, string name, string email, string phone)
        {
            _id = id;
            _name = name;
            _email = email;
            _phone = phone;
        }

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyOfPropertyChange(nameof(Id));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name=value;
                NotifyOfPropertyChange(nameof(Name));
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                NotifyOfPropertyChange(nameof(Phone));
            }
        }

        public string Email
        {
            get=>_email;
            set
            {
                _email = value;
                NotifyOfPropertyChange(nameof(Email));
            }
        }
    }

    public class UpdatedStudentInfo
    {
        public StudentModel Data { get; private set; }

        public UpdatedStudentInfo( StudentModel model)
        {
            Data = model;
        }
    }

}
