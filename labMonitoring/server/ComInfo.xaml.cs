using System;
using System.Collections.Generic;
using System.IO;
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

namespace server {
    /// <summary>
    /// ComInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ComInfo : Window {
        private Management labManager;
        string labNo;
        int num; // pc num (int)
        private System.Timers.Timer bTimer = new System.Timers.Timer(1000);
        string[] info;

        public ComInfo(string comNo, string labNo, ref Management labManager) {
            InitializeComponent();
            this.labNo = labNo;
            this.comNo.Text = comNo;
            this.labManager = labManager;
            bTimer.Elapsed += setComStatus;
            bTimer.Enabled = true;
            /*
            랩넘버와 컴넘버 이용해서 아이피 파일에서 읽어오기
             */

            num = int.Parse(comNo.Substring(4)) - 1;
            if ((info = labManager.GetAllData(num)) != null) {
                this.ipAddress.Text = info[0];
                this.macAddress.Text = info[1];
                this.cpuUsage.Text = info[2];
                this.memoryUsage.Text = info[3];
                this.hddCapcity.Text = info[4];
            }
        }

        public void setComStatus(Object source, System.Timers.ElapsedEventArgs e) {
            this.cpuUsage.Dispatcher.Invoke(
                () => {
                    String cpuUse = labManager.GetCpuUsage(num);
                    if (!cpuUse.Contains("alive") && cpuUsage != null)
                        this.cpuUsage.Text = cpuUse;
                }
                );
            this.memoryUsage.Dispatcher.Invoke(
                () => {
                    String ramUse = labManager.GetRamRemain(num);
                    if (!ramUse.Contains("alive") && cpuUsage != null)
                        this.memoryUsage.Text = ramUse;
                }
                );
        }
        //해당 컴퓨터 전원켜기 구현
        private void PcPowerOn(object sender, RoutedEventArgs e) {
            labManager.PcPowerOn(num);
        }
        //해당 컴퓨터 전원끄기 구현
        private void PcPowerOff(object sender, RoutedEventArgs e) {
            labManager.PcPowerOff(num);
        }

        private void PcPowerReboot(object sender, RoutedEventArgs e) {
            labManager.PcPowerReboot(num);
        }

        private void Window_Closed(object sender, EventArgs e) {
            bTimer.Enabled = false;
        }
    }
}
