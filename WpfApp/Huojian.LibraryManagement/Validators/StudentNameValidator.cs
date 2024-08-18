using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Huojian.LibraryManagement.Validators
{
    public class StudentNameValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value.ToString().Contains("A"))
            {
                return new ValidationResult(true,string.Empty);
            }
            return new ValidationResult(false, "Name must be started with Captial a");
        }
    }
}
