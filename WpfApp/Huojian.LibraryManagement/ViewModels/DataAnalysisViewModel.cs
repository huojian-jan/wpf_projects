using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Huojian.LibraryManagement.ViewModels
{
    public class DataAnalysisViewModel:Screen
    {
        public delegate DataAnalysisViewModel Factory();
        public DataAnalysisViewModel()
        {
            DisplayName = "数据分析";
        }
    }
}
