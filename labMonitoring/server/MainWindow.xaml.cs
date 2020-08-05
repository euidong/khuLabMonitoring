using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace server {
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window {
        private static Management lab01_manager;
        private static Management lab05_manager;
        private static Management lab07_manager;
        private static Management lab09_manager;
        private static Management lab11_manager;
        private static Management lab06_manager;

        NotifyIcon notify;
        public MainWindow() {
            try {
                System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
                notify = new NotifyIcon();
                //notify.Icon = new System.Drawing.Icon(@"TiimeAlarm.ico");
                System.Drawing.Icon myIcon = new System.Drawing.Icon("../../Resources/icon.ico");

                notify.Icon = myIcon;
                notify.Visible = true;
                notify.ContextMenu = menu;
                notify.Text = "khu_monitor";

                notify.DoubleClick += Notify_DoubleClick;

                System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
                menu.MenuItems.Add(item1);
                item1.Index = 0;
                item1.Text = "종료";
                item1.Click += delegate (object click, EventArgs eClick) {
                    System.Windows.Application.Current.Shutdown();
                    notify.Dispose();
                };
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            lab01_manager = new Management("01");
            lab05_manager = new Management("05");
            lab07_manager = new Management("07");
            lab09_manager = new Management("09");
            lab11_manager = new Management("11");
            lab06_manager = new Management("06");
            InitializeComponent();
        }

        private void Notify_DoubleClick(object sender, EventArgs e) {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Button Clicked = sender as System.Windows.Controls.Button;
            SubLabWindow sub = null;
            //랩실별로 컴퓨터 정보 가져오기
            string labNo = Clicked.Content.ToString();
            switch (labNo) {
                case "Lab01":
                    sub = new SubLabWindow(labNo, 54, ref lab01_manager);//파라미터에 랩실별 컴퓨터 수 넣기
                    break;
                case "Lab05":
                    sub = new SubLabWindow(labNo, 42, ref lab05_manager);
                    break;
                case "Lab07":
                    sub = new SubLabWindow(labNo, 42, ref lab07_manager);
                    break;
                case "Lab09":
                    sub = new SubLabWindow(labNo, 42, ref lab09_manager);
                    break;
                case "Lab11":
                    sub = new SubLabWindow(labNo, 42, ref lab11_manager);
                    break;
                case "Lab06":
                    sub = new SubLabWindow(labNo, 50, ref lab06_manager);
                    break;
                default:
                    break;
            }
            sub.Show();

        }
    }
}
