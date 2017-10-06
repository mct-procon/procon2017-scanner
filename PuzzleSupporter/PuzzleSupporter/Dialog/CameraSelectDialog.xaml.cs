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

namespace PuzzleSupporter.Dialog {
    /// <summary>
    /// CameraSelectDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class CameraSelectDialog : Window {
        uint CameraNum = 0;
        bool IgnoreChanges = false;

        public CameraSelectDialog() {
            InitializeComponent();
        }

        private void CameraNumBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (IgnoreChanges) return;
            if (uint.TryParse(CameraNumBox.Text, out uint u)) {
                CameraNum = u;
            } else {
                IgnoreChanges = true;
                CameraNumBox.Text = CameraNum.ToString();
                IgnoreChanges = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var window = new MainWindow((int)CameraNum);
            window.Show();
            this.Close();
        }
    }
}
