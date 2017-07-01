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
using Prism.Mvvm;

namespace PuzzleSupporter {
    /// <summary>
    /// InformationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InformationWindow : Window {
        public InformationWindow() {
            InitializeComponent();
        }

        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
                System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
        }

        public class ViewModel : BindableBase {
            internal string testdata;
            public string TestData {
                get => testdata;
                set => SetProperty(ref testdata, value);
            }

        }
    }
}
