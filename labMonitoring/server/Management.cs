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
      rerenderTimer.Elapsed += RenderView;
      rerenderTimer.Enabled = true;
      string ip;
      pc_num = 0;
      pc = new UdpClient[100];
      pc_status = new int[100]; // TODO: 처음에 전부 꺼진 걸로 초기화
      file = new StreamReader("../../ip/lab" + lab_num + ".txt");

      while ((ip = file.ReadLine()) != null) {
        pc[pc_num] = new UdpClient(ip, port);
        pc[pc_num++].Client.ReceiveTimeout = 1000; // * 최초 시행 시에는 timeOut을 짧게하고, 그 다음부터는 느리게 수행
      }
      file.Close();
    }
    
    public async Task<void> SetButtonList(List<Button> list) {
      buttonList = list;
      await RenderView(null, null);
    }
    // 상태확인하는 타이머 파라미터 (ms마다 등록한 함수 호출)
    private System.Timers.Timer rerenderTimer = new System.Timers.Timer(5000);

    private async Task<void> RenderView(Object source, System.Timers.ElapsedEventArgs e) {
      await CheckStatus();
      DrawButtonList();
    }

    private async Task<void> CheckStatus() {
      for (int i = 0; i < pc_num; i++) {
        int state = await IsAlive(i);
        if (state == (int)State.ON)
          pc_status[i] = state;
        else if (pc_status[i] != (int)State.SUSPEND)
          pc_status[i] = state;
      }
    }

    private void DrawButtonList() {
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


    public void SetAllPcTimeOut(int time) {
      for (int i = 0; i < pc_num; i++)
        pc[i].Client.ReceiveTimeout = time;
    }


    public async Task<int> IsAlive(int target_pc) {
      await SendMessageAsync(target_pc, "check");
      string receviedMessage = await receiveAsync(target_pc);

      if (receviedMessage == null)
        return (int)State.OFF;
      else if (receviedMessage == "suspend")
        return (int)State.SUSPEND;
      else
        return (int)State.ON;
    }


    public async Task<string[]> GetAllData(int target_pc) {
      await SendMessageAsync(target_pc, "all");
      string result = await ReceiveMessageAsync(target_pc);
      if (result != null)
        return result.Split(' ');
      else
        return null;
    }


    public async Task<string> GetIPAddress(int target_pc) {
      await SendMessageAsync(target_pc, "ip");
      return await ReceiveMessageAsync(target_pc);
    }

    public async Task<string> GetMacAddress(int target_pc) {
      await SendMessageAsync(target_pc, "mac");
      return await ReceiveMessageAsync(target_pc);
    }

    public async Task<string> GetCpuUsage(int target_pc) {
      await SendMessageAsync(target_pc, "cpu");
      return await ReceiveMessageAsync(target_pc);
    }
    public async Task<string> GetRamRemain(int target_pc) {
      await SendMessageAsync(target_pc, "ram");
      return await ReceiveMessageAsync(target_pc);
    }
    public async Task<string> GetHddUsage(int target_pc) {
      await SendMessageAsync(target_pc, "hdd");
      return await ReceiveMessageAsync(target_pc);
    }

    public async Task<string> ReceiveMessageAsync(int target_pc) {
      try {
        byte[] data = await pc[target_pc].ReceiveAsync(ref sender);
        return Encoding.UTF8.GetString(data);
      } catch (ObjectDisposedException e) {
        Console.WriteLine("연결이 종료되었습니다.");
        return null;
      } catch (SocketException e) {
        Console.WriteLine("연결이 끊겼습니다.");
        return null;
      }
    }

    public async Task<void> SendMessageAsync(int target_pc, string message) {
      if (pc[target_pc] != null) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        try {
          await pc[target_pc].SendAsync(data, data.Length);
        } catch (ObjectDisposedException e) {
          Console.WriteLine("연결이 종료되었습니다.");
        } catch (SocketException e) {
          Console.WriteLine("연결이 끊겼습니다.");
        }
      }
    }

    // ! ------------------------------ 
    // ! 해당 기능은 port forwarding이 불가능한, 교내 사정상 불가능한 기능입니다. (PC ON)
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

    public void AllPcPowerOn() {
      for (int i = 0; i < pc_num; i++) {
        PcPowerOn(i);
      }
    }
    // ! -----------------------------

    public async Task<void> PcPowerOffAsync(int target_pc) {
      await SendMessageAsync(target_pc, "off");
    }

    // TODO: 모두 끝날 때까지 대기하는 로직 추가.
    public async Task<void> AllPcPowerOffAsync() {
      for (int i = 0; i < pc_num; i++) {
        await PcPowerOffAsync(i);
      }
    }

    public async Task<void> PcPowerRebootAsync(int target_pc) {
      await SendMessageAsync(target_pc, "reboot");
    }

    // TODO: 모두 끝날 때까지 대기하는 로직 추가.    
    public async Task<void> AllPcPowerRebootAsync() {
      for (int i = 0; i < pc_num; i++) {
        await PcPowerRebootAsync(i);
      }
    }
  }
}
