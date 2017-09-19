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

using ZXing;

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
            private WriteableBitmap _cameraImage;
            private Thread _cameraThread;
            private bool _isAlive = true;
            private Dispatcher _windowDispatcher;
            private MainWindow Window;
            private Scalar Upper = new Scalar(180, 255, 255);
            private Scalar Lower = new Scalar(0, 0, 0);
            private FilterPreviewWindow FilterWindow;
            private FilterPreviewWindow.ViewModel FilterViewModel;
            private double _ApproxDPEpsilon;
            private Procon2017MCTProtocol.IProconPuzzleService PuzzService;

            private string _DetectedQrCode = "Nothing";

            private DelegateCommand _StartDetectingQRCodeCommand;
            private DelegateCommand _SendQRCodeAsHintCommand;
            private DelegateCommand _AppendQRCodeCommand;

            internal ViewModel(Dispatcher disp, MainWindow win) {
                _windowDispatcher = disp;
                Window = win;
                try {
                    PuzzService = Network.WCF.StartWCFSender();
                } catch {
                    MessageBox.Show("ソルバとの接続に失敗しました．", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    PuzzService = new Network.WCF.Dummy();
                }
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

            public double ApproxDPEpsilon {
                get => _ApproxDPEpsilon;
                set => SetProperty(ref _ApproxDPEpsilon, value);
            }

            private bool isQrCodeDetected = false;
            private bool isQrCodeDetecting = false;
            private bool isBlueButtonEnable = false;

            public bool IsQrCodeDetected {
                get => isQrCodeDetected;
                set => SetProperty(ref isQrCodeDetected, value);
            }
            public bool IsQrCodeDetecting {
                get => isQrCodeDetecting;
                set => SetProperty(ref isQrCodeDetecting, value);
            }

            public bool IsBlueButtonEnable {
                get => isBlueButtonEnable;
                set => SetProperty(ref isBlueButtonEnable, value);
            }

            public string DetectedQrCode {
                get => _DetectedQrCode;
                set => SetProperty(ref _DetectedQrCode, value);
            }

            public DelegateCommand SendQrCodeAsHintCommand =>
                _SendQRCodeAsHintCommand ?? (_SendQRCodeAsHintCommand = new DelegateCommand(SendQrCodeAsHint));

            public DelegateCommand StartDetectingQRCodeCommand =>
                _StartDetectingQRCodeCommand ?? (_StartDetectingQRCodeCommand = new DelegateCommand(StartDetectingQRCode));

            public DelegateCommand AppendQRCodeCommand =>
                _AppendQRCodeCommand ?? (_AppendQRCodeCommand = new DelegateCommand(AppendQRCode));

            private OpenCvSharp.Point[] DetectedPolygon;
            private Parser.PolygonParser polygonParser;
            internal void BeginCaptureing() {
                _cameraThread = new Thread(() => {
                    WriteableBitmap _back_thread_camera_img = null;
                    WriteableBitmap _back_thread_filter_img = null;
                    using(var Camera = new VideoCapture(DeviceId)) {
                        if (!Camera.IsOpened()) {
                            _isAlive = false;
                            _windowDispatcher.Invoke(() => {
                                Window.Close();
                            });
                            return;
                        }
                        BarcodeReader QrReader = new BarcodeReader();
                        Result QrResult = null;
                        var QrSource = new PuzzleSupporter.ZXingNet.BitmapSourceLuminanceSourceEx(Camera.FrameWidth, Camera.FrameHeight);
                        PointCollection PointCollectionCache;
                        using (var img = new Mat()) {
                            using (var hsvimg = new Mat()) {
                                using (var bwimg = new Mat()) {
                                    _windowDispatcher.Invoke(() => {
                                        _back_thread_camera_img = new WriteableBitmap(Camera.FrameWidth, Camera.FrameHeight, 96, 96, PixelFormats.Bgr24, null);
                                        _back_thread_filter_img = new WriteableBitmap(Camera.FrameWidth, Camera.FrameHeight, 96, 96, PixelFormats.Gray8, null);
                                        PointCollectionCache = new PointCollection(20);
                                        FilterWindow = new FilterPreviewWindow(DeviceId);
                                        FilterViewModel = new FilterPreviewWindow.ViewModel();
                                        FilterWindow.DataContext = FilterViewModel;
                                        FilterWindow.Closing += (ss, ee) => {
                                            if (_isAlive)
                                                ee.Cancel = true;
                                        };
                                        ApproxDPEpsilon = 1.5;
                                        FilterWindow.Show();
                                    });

                                    if (!Camera.Read(img)) { 
                                        MessageBox.Show("Cannot read image from imaging Device...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        _windowDispatcher.Invoke(() => {
                                            FilterWindow.Close();
                                        });
                                        _isAlive = false;
                                    }
                                    while (_isAlive) {
                                        _windowDispatcher.Invoke(() => {
                                            if (img.IsDisposed || img.Rows * img.Cols == 0) return;
                                            OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(img, _back_thread_camera_img);
                                            CameraImage = _back_thread_camera_img;
                                            QrSource.UpdateImage(_back_thread_camera_img);
                                        });
                                        Cv2.CvtColor(img, hsvimg, ColorConversionCodes.BGR2HSV);
                                        Cv2.InRange(hsvimg, Lower, Upper, bwimg);
                                        DetectedPolygon = Cv2.FindContoursAsArray(bwimg, RetrievalModes.List, ContourApproximationModes.ApproxSimple).Where(c => Cv2.ContourArea(c) > 1000).Select(c => Cv2.ApproxPolyDP(c, _ApproxDPEpsilon, true)).FirstOrDefault();
                                        QrResult = QrReader.Decode(QrSource);
                                        _windowDispatcher.Invoke(() =>
                                        {
                                            if (bwimg.IsDisposed || bwimg.Rows * bwimg.Cols == 0) return;
                                            OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(bwimg, _back_thread_filter_img);
                                            FilterViewModel.Img = _back_thread_filter_img;
                                            if (QrResult != null) {
                                                IsQrCodeDetected = true;
                                                IsBlueButtonEnable = true;
                                                DetectedQrCode = QrResult.Text;
                                            }
                                            Window.DetectPoly.Points.Clear();
                                            if (DetectedPolygon != null) {
                                                foreach (var p in DetectedPolygon) {
                                                    Window.DetectPoly.Points.Add(new System.Windows.Point(p.X, p.Y));
                                                }
                                            }
                                        });
                                        Thread.Sleep(App._fps);
                                        Camera.Read(img);
                                    }
                                    _windowDispatcher.Invoke(() => {
                                        FilterWindow.Close();
                                    });
                                }
                            }
                        }
                    }
                });
                _cameraThread.Start();
            }

            public void StartDetectingQRCode() {
                Send(false);
            }

            public void SendQrCodeAsHint() {
                Send(true);
            }

            private void Send(bool isHint) {
                if (IsQrCodeDetecting) {
                    IsQrCodeDetecting = false;
                    IsQrCodeDetected = false;
                    IsBlueButtonEnable = false;
                    Procon2017MCTProtocol.QRCodeData data = polygonParser.SendData;
                    data.IsHint = isHint;
                    //using (var sw = new System.IO.StreamWriter("out.txt")) {
                    //    sw.AutoFlush = false;
                    //    sw.WriteLine(data.Frames.Count);
                    //    foreach (var p in data.Frames) {
                    //        sw.WriteLine(p.Points.Count);
                    //        foreach (var pp in p.Points) {
                    //            sw.WriteLine($"{pp.X} {pp.Y}");
                    //        }
                    //    }
                    //    sw.WriteLine();
                    //    sw.WriteLine(data.Polygons.Count);
                    //    foreach (var p in data.Polygons) {
                    //        sw.WriteLine(p.Points.Count);
                    //        foreach (var pp in p.Points) {
                    //            sw.WriteLine($"{pp.X} {pp.Y}");
                    //        }
                    //    }
                    //    sw.Flush();
                    //}
                    Task.Run(() => PuzzService.QRCode(data)).ContinueWith(res => {
                        if (res.IsFaulted)
                            MessageBox.Show("QRコードデータの送信に失敗しました．", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            IsBlueButtonEnable = true;
                    }, TaskScheduler.Current);
                } else {
                    IsQrCodeDetecting = true;
                    Parser.PolygonParser.Read(DetectedQrCode).ContinueWith(res => {
                        if (res.IsFaulted) 
                            MessageBox.Show("QRコードデータの解析に失敗しました．", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                            polygonParser = res.Result;
                    }, TaskScheduler.Current);
                }
            }

            public void AppendQRCode() {
                if (!IsQrCodeDetecting)
                    return;
                polygonParser.Append(DetectedQrCode).ContinueWith(res => {
                    if (res.IsFaulted) 
                        MessageBox.Show("QRコードデータの解析に失敗しました．", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskScheduler.Current);
            }

            public void SendPolygon() {
                Procon2017MCTProtocol.SendablePolygon send = new Procon2017MCTProtocol.SendablePolygon();
                send.Points.AddRange(DetectedPolygon.Select((p) => new Procon2017MCTProtocol.SendablePoint(p.X, p.Y)));
                try {
                    PuzzService.Polygon(send);
                }catch {
                    MessageBox.Show("図形データの送信に失敗しました．", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
}
