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

namespace server
{
    /// <summary>
    /// subLabWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubLabWindow : Window
    {
        string labNo;
        private Management labManager;
        List<Button> buttonList = new List<Button>();
        public SubLabWindow()
        {
            InitializeComponent();
        }

        public SubLabWindow(string LabNo, int ComputerNum, ref Management labManager)
        {
            InitializeComponent();
            this.labNo = LabNo;//현재 어느 랩실, 컴퓨터 정보 출력할때 사용하기
            this.Title = LabNo;
            this.labManager = labManager;

            
            //컴퓨터 버튼 만들기
            for (int y = 0; y < MainGrid.RowDefinitions.Count; y++)
            {
                for (int x = 0; x < MainGrid.ColumnDefinitions.Count; x++)
                {


                    if (this.labNo== "Lab06")
                    {
                        if((x == 0 && y == 3) || (x == 0 &&y == 8))
                        {
                            x += 2;
                        }
                    }

                    int computerNo = (x + 1) + (y * 6);
                    if (this.labNo == "Lab06")
                    {
                        if (y >= 3)
                        {
                            computerNo -= 2;
                            if (y == 8)
                                computerNo -= 2;
                        }
                    }
                    Button computer = new Button();
                    computer.Content = "COM_" + computerNo;
                    Grid.SetRow(computer, y);
                    Grid.SetColumn(computer, x);
                    computer.Click += Computer_Click;//클릭 이벤트
                    MainGrid.Children.Add(computer);

                    buttonList.Add(computer);

                    if (computerNo == ComputerNum)
                    {
                        labManager.setButtonList(buttonList);
                        return;
                    }
                }
            }
            

        }


        String selectedBtn = null;
        private void Computer_Click(object sender, RoutedEventArgs e)
        {
            saveComText();
            ComInfo comInfo = new ComInfo((sender as Button).Content.ToString(), labNo, ref labManager);
            selectedBtn = (sender as Button).Content.ToString();
            SelectedCom.Text = selectedBtn;

            try
            {
                comText.Text = File.ReadAllText("../../memo/" + this.labNo + "/" + (sender as Button).Content.ToString() + ".txt");

            }
            catch(FileNotFoundException)
            {
                File.WriteAllText("../../memo/" + this.labNo + "/" + (sender as Button).Content.ToString() + ".txt", "");
            }
            comInfo.Show();
        }

        private void saveComText()
        {
            if (selectedBtn != null)
            {
                //
                File.WriteAllText("../../memo/" + this.labNo + "/" + selectedBtn + ".txt", comText.Text);
            }

        }
        //전체 컴퓨터 종료
        private void AllPcPowerOff(object sender, RoutedEventArgs e)
        {
            labManager.AllPcPowerOff();
        }
        //전체 컴퓨터 종료
        private void AllPcPowerOn(object sender, RoutedEventArgs e)
        {
            labManager.AllPcPowerOn();
        }
        private void AllPcPowerReboot(object sender, RoutedEventArgs e)
        {
            labManager.AllPcPowerReboot();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            labManager.setTimeOut(10);
            //랩텍스트 저장
            File.WriteAllText("../../memo/" + this.labNo + "/labMemo.txt", subLabText.Text);

            saveComText();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            subLabText.Text = File.ReadAllText("../../memo/" + this.labNo + "/labMemo.txt");
        }
    }
}
