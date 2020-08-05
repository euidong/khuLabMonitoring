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
      aTimer.Elapsed += CheckStatus;
      aTimer.Enabled = true;
      string ip;
      pc_num = 0;
      pc = new UdpClient[100];
      pc_status = new int[100];
      file = new StreamReader("../../ip/lab" + lab_num + ".txt");

      while ((ip = file.ReadLine()) != null) {
        pc[pc_num] = new UdpClient(ip, port);
        pc[pc_num++].Client.ReceiveTimeout = 10;
      }
      file.Close();
    }
    public void setButtonList(List<Button> list) {
      buttonList = list;
      CheckStatus(null, null);
      setTimeOut(1000);
    }
    // 상태확인하는 타이머 파라미터 (ms마다 등록한 함수 호출)
    private System.Timers.Timer aTimer = new System.Timers.Timer(5000);

    private void CheckStatus(Object source, System.Timers.ElapsedEventArgs e) {
      if (buttonList != null) {
        // pc상태에따라 0:꺼짐 1:켜짐 저장
        for (int i = 0; i < pc_num; i++) {
          int state = isAlive(i);
          if (state == (int)State.ON)
            pc_status[i] = state;
          else if (pc_status[i] != (int)State.SUSPEND)
            pc_status[i] = state;
        }

        for (int i = 0; i < pc_num; i++) {
          buttonList.ElementAt(i).Dispatcher.Invoke(
              () => {
                //꺼져있으면 버튼 회색으로
                if (pc_status[i] == (int)State.OFF) {
                  buttonList.ElementAt(i).Background = Brushes.Gray;

                }
                else if (pc_status[i] == (int)State.SUSPEND) {
                  buttonList.ElementAt(i).Background = Brushes.OrangeRed;
                }
                //켜져있으면 버튼 밝은 회색
                else {
                  buttonList.ElementAt(i).Background = Brushes.LightGray;
                }
              }
              );
        }
      }
    }


    public void setTimeOut(int time) {
      for (int i = 0; i < pc_num; i++)
        pc[i].Client.ReceiveTimeout = time;
    }


    // TODO : 스레드로 바꾼다. SEND, RECEIVE
    public void SendMessage(int target_pc, string message) {
      if (pc[target_pc] != null) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        pc[target_pc].Send(data, data.Length);
      }

    }

    public string ReceiveMessage(int target_pc) {
      try {
        byte[] data = pc[target_pc].Receive(ref sender);
        return Encoding.UTF8.GetString(data);
      }
      catch (SocketException e) {
        return null;
      }
    }

    public int isAlive(int target_pc) {

      SendMessage(target_pc, "check");

      if (ReceiveMessage(target_pc) == null)
        return (int)State.OFF;
      else if (ReceiveMessage(target_pc) == "suspend")
        return (int)State.SUSPEND;
      else
        return (int)State.ON;
    }


    public string[] GetAllData(int target_pc) {
      SendMessage(target_pc, "all");
      string result = ReceiveMessage(target_pc);
      if (result != null)
        return result.Split(' ');
      else
        return null;
    }


    public string GetIPAddress(int target_pc) {
      SendMessage(target_pc, "ip");
      return ReceiveMessage(target_pc);
    }

    public string GetMacAddress(int target_pc) {
      SendMessage(target_pc, "mac");
      return ReceiveMessage(target_pc);
    }

    public string GetCpuUsage(int target_pc) {
      SendMessage(target_pc, "cpu");
      return ReceiveMessage(target_pc);
    }
    public string GetRamRemain(int target_pc) {
      SendMessage(target_pc, "ram");
      return ReceiveMessage(target_pc);
    }
    public string GetHddUsage(int target_pc) {
      SendMessage(target_pc, "hdd");
      return ReceiveMessage(target_pc);
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
      SendMessage(target_pc, "off");
    }

    public void PcPowerReboot(int target_pc) {
      SendMessage(target_pc, "reboot");
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
