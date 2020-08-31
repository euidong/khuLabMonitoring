using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace server {
  enum State { OFF = 0, ON, SUSPEND };
  public class Management {
    private UdpClient[] pc;
    private int pc_num;
    private const int port = 10000;
    StreamReader file;
    IPEndPoint sender;
    private delegate void RunDelegate(int i);
    private int[] pc_status;

    List<Button> buttonList = null;

    public Management(string lab_num) {
      //타이머 함수 등록
      rerenderTimer.Elapsed += renderView;
      rerenderTimer.Enabled = true;
      string ip;
      pc_num = 0;
      pc = new UdpClient[100];
      pc_status = new int[100];
      file = new StreamReader("../../ip/lab" + lab_num + ".txt");

      while ((ip = file.ReadLine()) != null) {
        pc[pc_num] = new UdpClient(ip, port);
        pc[pc_num++].Client.ReceiveTimeout = 10; // * 최초 시행 시에는 timeOut을 짧게하고, 그 다음부터는 느리게 수행
      }
      file.Close();
    }

    public void setButtonList(List<Button> list) {
      buttonList = list;
      renderView(null, null);
      setAllPcTimeOut(1000); // * 전체 화면 잡은 후로는 pc time out을 1초로 변경.
    }
    // 상태확인하는 타이머 파라미터 (ms마다 등록한 함수 호출)
    private System.Timers.Timer rerenderTimer = new System.Timers.Timer(5000);

    public void renderView(Object source, System.Timers.ElapsedEventArgs e) {
      checkStatus();
      drawButtonList();
    }

    private void checkStatus() {
      for (int i = 0; i < pc_num; i++) {
        int state = isAlive(i);
        if (state == (int)State.ON)
          pc_status[i] = state;
        else if (pc_status[i] != (int)State.SUSPEND)
          pc_status[i] = state;
      }
    }

    private void drawButtonList() {
      if (buttonList != null) {
        for (int i = 0; i < pc_num; i++) {
          buttonList.ElementAt(i).Dispatcher.Invoke(
            () => {
              switch (pc_status[i]) {
                case (int)State.OFF:
                  buttonList.ElementAt(i).Background = Brushes.Gray;
                  break;
                case (int)State.SUSPEND:
                  buttonList.ElementAt(i).Background = Brushes.OrangeRed;
                  break;
                default:
                  buttonList.ElementAt(i).Background = Brushes.LightGray;
                  break;
              }
            }
          );
        }
      }
    }


    public void setAllPcTimeOut(int time) {
      for (int i = 0; i < pc_num; i++)
        pc[i].Client.ReceiveTimeout = time;
    }


    public int isAlive(int target_pc) {
      sendMessage(target_pc, "check");
      if (receiveMessage(target_pc) == null)
        return (int)State.OFF;
      else if (receiveMessage(target_pc) == "suspend")
        return (int)State.SUSPEND;
      else
        return (int)State.ON;
    }


    public string[] GetAllData(int target_pc) {
      sendMessage(target_pc, "all");
      string result = receiveMessage(target_pc);
      if (result != null)
        return result.Split(' ');
      else
        return null;
    }


    public string GetIPAddress(int target_pc) {
      sendMessage(target_pc, "ip");
      return receiveMessage(target_pc);
    }

    public string GetMacAddress(int target_pc) {
      sendMessage(target_pc, "mac");
      return receiveMessage(target_pc);
    }

    public string GetCpuUsage(int target_pc) {
      sendMessage(target_pc, "cpu");
      return receiveMessage(target_pc);
    }
    public string GetRamRemain(int target_pc) {
      sendMessage(target_pc, "ram");
      return receiveMessage(target_pc);
    }
    public string GetHddUsage(int target_pc) {
      sendMessage(target_pc, "hdd");
      return receiveMessage(target_pc);
    }

    public string receiveMessage(int target_pc) {
      try {
        byte[] data = pc[target_pc].Receive(ref sender);
        return Encoding.UTF8.GetString(data);
      }
      catch (SocketException e) {
        return null;
      }
    }

    public void sendMessage(int target_pc, string message) {
      if (pc[target_pc] != null) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        pc[target_pc].Send(data, data.Length);
      }
    }



    private static void WakeUp(byte[] mac) {
      UdpClient client = new UdpClient();
      client.Connect(IPAddress.Broadcast, 9);

      byte[] packet = new byte[17 * 6];

      for (int i = 0; i < 6; i++) {
        packet[i] = 0xFF;
      }

      for (int i = 1; i <= 16; i++) {
        for (int j = 0; j < 6; j++) {
          packet[i * 6 + j] = mac[j];
        }
      }
      client.Send(packet, packet.Length);
    }

    public string PcPowerOn(int target_pc) {
      byte[] mac = new byte[] { 0x60, 0x45, 0xCB, 0x9E, 0x6F, 0x48 };
      WakeUp(mac);
      return " ";
    }
    public void PcPowerOff(int target_pc) {
      sendMessage(target_pc, "off");
    }

    public void PcPowerReboot(int target_pc) {
      sendMessage(target_pc, "reboot");
    }

    public void AllPcPowerOn() {
      for (int i = 0; i < pc_num; i++) {
        PcPowerOn(i);
      }
    }

    public void AllPcPowerOff() {
      for (int i = 0; i < pc_num; i++) {
        PcPowerOff(i);
      }
    }
    public void AllPcPowerReboot() {
      for (int i = 0; i < pc_num; i++) {
        PcPowerReboot(i);
      }
    }
  }
}
