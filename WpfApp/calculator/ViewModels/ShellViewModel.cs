using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Accessibility;
using Caliburn.Micro;
using Microsoft.CSharp;

namespace calculator.ViewModels
{
    public class ShellViewModel : Screen
    {
        private double _result;
        private string _input;
        private string _currentTime;
        private readonly System.Timers.Timer _currentTimeDispather;
        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        public double Result
        {
            get => _result;
            set
            {
                _result = value;
                NotifyOfPropertyChange(() => Result);
            }
        }

        public string Input
        {
            get => _input;
            set
            {
                if (value == null)
                    return;
                _input = value;
                NotifyOfPropertyChange(() => Input);
            }
        }

        public ShellViewModel()
        {
            _currentTimeDispather = new System.Timers.Timer();
        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _currentTimeDispather.Interval = 1000;
            _currentTimeDispather.Elapsed += _currentTimeDispather_Elapsed;
            _currentTimeDispather.Start();
        }

        private void _currentTimeDispather_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CurrentTime = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
        }


        public void InputAction(string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            Input +=content; 
        }


        public void Equal()
        {
            var expression = PrepareExpression();
            if (IsValidExpression(expression))
            {
                Result = ComputeResult(expression);
            }
            else
            {
                MessageBox.Show("invalid expression");
            }
        }

        public void Dot()
        {
            Input += ".";
        }

        public void Backward()
        {
            if (Input.Length > 0)
            {
                Input = Input.Remove(Input.Length - 1);
            }
        }

        #region Function

        public void Clear()
        {
            Result = 0;
            Input = string.Empty;
        }

        public void Remainder()
        {
            Input += "%";
        }

        private string PrepareExpression()
        {
            var originExpress = Input;
            originExpress = originExpress.Replace("u00f7", "/");
            originExpress = originExpress.Replace("x", "*");

            return originExpress;
        }

        private double ComputeResult(string expression)
        {
            DataTable dt = new DataTable();
            object result = dt.Compute(expression, "");
            return Convert.ToDouble(result);
        }

        public static bool IsValidExpression(string expression)
        {
            // Use a regular expression to check for basic syntax
            string pattern = @"^[\d+\-*/\(\)]+$";
            if (!Regex.IsMatch(expression, pattern))
            {
                return false;
            }

            // Try to evaluate the expression using DataTable
            try
            {
                DataTable dt = new DataTable();
                object result = dt.Compute(expression, "");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        #endregion


    }


    public class ExpressionWarningAdorner : Adorner
    {
        public ExpressionWarningAdorner(UIElement adornedElement) : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }


    public class ExpressionValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string expression)
            {
                if (expression.Contains("1"))
                {
                    return new ValidationResult(false, "!!!");
                }
                //if (IsValidExpression(expression))
                //{
                //    return new ValidationResult(true, "");
                //}
                //else
                //{
                //    return new ValidationResult(false, "invalid expression");
                //}
            }

            return new ValidationResult(true, "");
        }
        public static bool IsValidExpression(string expression)
        {
            // Use a regular expression to check for basic syntax
            string pattern = @"^[\d+\-*/\(\)]+$";
            if (!Regex.IsMatch(expression, pattern))
            {
                return false;
            }

            // Try to evaluate the expression using DataTable
            try
            {
                DataTable dt = new DataTable();
                object result = dt.Compute(expression, "");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
