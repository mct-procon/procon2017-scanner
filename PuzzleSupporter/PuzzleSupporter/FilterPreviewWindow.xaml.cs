using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Prism.Commands;
using Prism.Mvvm;

using OpenCvSharp;

namespace PuzzleSupporter {
    /// <summary>
    /// FilterPreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FilterPreviewWindow : System.Windows.Window {
        public FilterPreviewWindow(int DeviceId)
        {
            InitializeComponent();
            Title = $"FilterWindow {DeviceId} - PuzzleSupporter";
        }

        public FilterPreviewWindow() {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void NormalMaxButton_Click(object sender, RoutedEventArgs e) {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public class ViewModel : BindableBase {
            internal WriteableBitmap _Img;

            public WriteableBitmap Img {
                get => _Img;
                set => SetProperty(ref _Img, value);
            }
        }
    }
}
