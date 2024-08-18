// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors.Media;

namespace WpfApp1
{
    public class Program
    {
        static void Main(string[] args)
        {
            var bootstrapper=new Bootstrapper();
            var app = new Application();

            app.Resources.Add("bootstrapper",bootstrapper);
            app.Run();
        }
    }
}
