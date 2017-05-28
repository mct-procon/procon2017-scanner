using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Threading;

using OpenCvSharp;

using Prism.Commands;
using Prism.Mvvm;

namespace PuzzleSupporter {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window {
        internal ViewModel _viewModel;

        public MainWindow() {
            InitializeComponent();
            _viewModel = new ViewModel();
            _viewModel._windowDispatcher = Dispatcher;
            _viewModel.BeginCaptureing();
            this.DataContext = _viewModel;

        }

        public class ViewModel : BindableBase {
            internal WriteableBitmap _cameraImage;
            internal Thread _cameraThread;
            internal bool _isAlive = true;
            internal Dispatcher _windowDispatcher;

            public WriteableBitmap CameraImage {
                get => _cameraImage;
                set => SetProperty(ref _cameraImage, value);
            }

            internal void BeginCaptureing() {
                _cameraThread = new Thread(() => {
                    using(var Camera = new VideoCapture(0)) {
                        _cameraImage = new WriteableBitmap(Camera.FrameWidth, Camera.FrameHeight, 96, 96, PixelFormats.Bgr24, null);
                        using (var img = new Mat()) {
                            Camera.Read(img);
                            while (_isAlive) {
                                _windowDispatcher.BeginInvoke((Action)(() => {
                                    if(!img.IsDisposed)
                                        CameraImage = OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(img);
                                }));
                                Thread.Sleep(1000 / 60);
                                Camera.Read(img);
                            }
                        }
                    }
                });
                _cameraThread.Start();
            }

            internal void Stop() {
                _isAlive = false;
            }
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

        private void ThisWindow_Closed(object sender, EventArgs e) {
            _viewModel.Stop();
            Thread.Sleep(1000 / 60 * 20);
        }
    }

    public class MaximizeNormalizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is WindowState)
                return (WindowState)value == WindowState.Normal ? "1" : "2";
            else
                throw new FormatException("Bad Type. Need WindowState Typed Value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
