// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using WpfApp1.Models;
using WpfApp1.Views;

namespace WpfApp1.ViewModels
{
    public class StudentInfoViewModel : Conductor<Screen>.Collection.OneActive, IHandle<UpdatedStudentInfo>
    {
        public ObservableCollection<StudentModel> Students { get; set; }
        private readonly IEventAggregator _eventAggregator;
        private readonly EditStudentInfoViewModel _editStudentInfoViewModel;
        private readonly AddStudentInfoViewModel _addStudentInfoViewModel;

        public StudentInfoViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Students = new ObservableCollection<StudentModel>();
            InitData();
        }

        private void InitData()
        {
            var stu1 = new StudentModel(1, "张三", "zhangsan@example.com", "13067104199");
            var stu2 = new StudentModel(2, "王五", "wangwu@example.com", "13067104299");
            var stu3 = new StudentModel(3, "李四", "lisi@example.com", "13067104399");
            var stu4 = new StudentModel(4, "小明", "xiaoming@example.com", "13067104499");

            Students.Add(stu1);
            Students.Add(stu2);
            Students.Add(stu3);
            Students.Add(stu4);
        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            _eventAggregator.Subscribe(this);
        }

        public void AddStudent()
        {

        }

        public void Remove(StudentModel model)
        {

        }

        public async Task Edit(StudentModel model)
        {
            _editStudentInfoViewModel.CurrentStudent = model;
            await ActivateItemAsync(_editStudentInfoViewModel);
        }

        public void Handle(UpdatedStudentInfo message)
        {
        }

        public Task HandleAsync(UpdatedStudentInfo message, CancellationToken cancellationToken)
        {
            var editedStudent=Students.FirstOrDefault(x => x.Id == message.Data.Id);
            editedStudent = message.Data;
            return Task.CompletedTask;
        }
    }
}
