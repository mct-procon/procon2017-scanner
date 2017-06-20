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

        public MainWindow(int deviceId)
        {
            InitializeComponent();
            _viewModel = new ViewModel(Dispatcher, this);
            _viewModel.DeviceId = deviceId;
            this.Title = $"{deviceId} - PuzzleSupporter";
            this.DataContext = _viewModel;
        }

        public MainWindow() {
            InitializeComponent();
            _viewModel = new ViewModel(Dispatcher, this);
            this.DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _viewModel.BeginCaptureing();
        }


        public class ViewModel : BindableBase {
            internal int DeviceId = 0;
            internal WriteableBitmap _cameraImage;
            internal Thread _cameraThread;
            internal bool _isAlive = true;
            internal Dispatcher _windowDispatcher;
            internal MainWindow Window;
            internal Scalar Upper = new Scalar(180, 255, 255);
            internal Scalar Lower = new Scalar(0, 0, 0);
            internal FilterPreviewWindow FilterWindow;
            internal FilterPreviewWindow.ViewModel FilterViewModel;

            internal ViewModel(Dispatcher disp, MainWindow win) {
                _windowDispatcher = disp;
                Window = win;
            }

            public WriteableBitmap CameraImage {
                get => _cameraImage;
                set => SetProperty(ref _cameraImage, value);
            }

            public double H_Upper {
                get => Upper.Val0;
                set => SetProperty(ref Upper.Val0, value);
            }

            public double H_Lower {
                get => Lower.Val0;
                set => SetProperty(ref Lower.Val0, value);
            }

            public double S_Upper {
                get => Upper.Val1;
                set => SetProperty(ref Upper.Val1, value);
            }

            public double S_Lower {
                get => Lower.Val1;
                set => SetProperty(ref Lower.Val1, value);
            }

            public double V_Upper {
                get => Upper.Val2;
                set => SetProperty(ref Upper.Val2, value);
            }

            public double V_Lower {
                get => Lower.Val2;
                set => SetProperty(ref Lower.Val2, value);
            }


            private WriteableBitmap _back_thread_camera_img;
            private WriteableBitmap _back_thread_filter_img;
            internal void BeginCaptureing() {
                _cameraThread = new Thread(() => {
                    using(var Camera = new VideoCapture(DeviceId)) {
                        using (var img = new Mat()) {
                            using (var hsvimg = new Mat()) {
                                using (var bwimg = new Mat()) {
                                    _windowDispatcher.Invoke(() => {
                                        _back_thread_camera_img = new WriteableBitmap(Camera.FrameWidth, Camera.FrameHeight, 96, 96, PixelFormats.Bgr24, null);
                                        _back_thread_filter_img = new WriteableBitmap(Camera.FrameWidth, Camera.FrameHeight, 96, 96, PixelFormats.Gray8, null);
                                        FilterWindow = new FilterPreviewWindow(DeviceId);
                                        FilterViewModel = new FilterPreviewWindow.ViewModel();
                                        FilterWindow.DataContext = FilterViewModel; 
                                        FilterWindow.Show();
                                    });
                                    Camera.Read(img);
                                    while (_isAlive) {
                                        _windowDispatcher.Invoke(() => {
                                            if (img.IsDisposed) return;
                                            OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(img, _back_thread_camera_img);
                                            CameraImage = _back_thread_camera_img;
                                        });
                                        Cv2.CvtColor(img, hsvimg, ColorConversionCodes.BGR2HSV);
                                        Cv2.InRange(hsvimg, Lower, Upper, bwimg);
                                        _windowDispatcher.Invoke(() =>
                                        {
                                            if (bwimg.IsDisposed) return;
                                            OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(bwimg, _back_thread_filter_img);
                                            FilterViewModel.Img = _back_thread_filter_img;
                                        });
                                        Thread.Sleep(1000 / 60);
                                        Camera.Read(img);
                                    }
                                }
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
            Thread.Sleep(1000);
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
