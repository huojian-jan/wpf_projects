using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Huojian.LibraryManagement.Common
{
    public class RichWindowManager : WindowManager
    {
        //public override bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        //{
        //    //此处不用显式的调用ApplySettings，CreateWindow中会调用
        //    var window = CreateWindow(rootModel, true, context, settings);
        //    return window.ShowDialog();
        //}

        public void CloseAllPopups()
        {
            //PopupCloser.CloseAllPopups();
        }

        //protected override Window EnsureWindow(object model, object view, bool isDialog)
        //{
        //    if (isDialog && !(view is Window))
        //    {
        //        var dialog = new RichDialog()
        //        {
        //            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        //            Content = view,
        //            SizeToContent = SizeToContent.WidthAndHeight,
        //            ResizeMode = ResizeMode.NoResize,
        //            ShowInTaskbar = false
        //        };
        //        return dialog;
        //    }
        //    else
        //    {
        //        return base.EnsureWindow(model, view, isDialog);
        //    }
        //}
    }
}
