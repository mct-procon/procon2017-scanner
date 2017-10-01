﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PuzzleSupporter {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private const int FPS = 10;            // ここでFPSを設定（多少ぶれます）

        public const int _fps = 1000 / FPS;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] commandargs = System.Environment.GetCommandLineArgs();
            int o;
            if(commandargs.Length > 2 && int.TryParse(commandargs[1], out o)) {
                (new MainWindow(o)).Show();
            } else {
                (new Dialog.CameraSelectDialog()).Show();
            }
        }
    }
}
