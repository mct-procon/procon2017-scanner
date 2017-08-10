using System;
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
            (new MainWindow(0)).Show();
        }
    }
}
