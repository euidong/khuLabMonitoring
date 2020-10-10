using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;


namespace server {
  enum PowerState { OFF = 0, ON, SUSPEND };
  
  public class Management {  
    private const int pingServerPort = 44484;
    private const int messageServerPort = 44485;
    private const int pingClientPort = 47214;
    private const int messageClientPort = 47215;

    private IPEndPoint[] pingClient;
    private IPEndPoint[] messageClient;
    private int clientNum;
    
    StreamReader file;
    private delegate void RunDelegate(int i);
    private int[] clientPowerState;

    List<Button> buttonList = null;

    public Management(string lab_num) {      
      clientNum = 0;
      pingClient = new IPEndPoint[100];
      messageClient = new IPEndPoint[100];
      clientPowerState = new int[100];
      for (int i = 0; i < 100; i++) {
        clientPowerState[i] = (int)PowerState.OFF;
      }

      // ip table에서 데이터를 얻어온다.
      file = new StreamReader("../../ip/lab" + lab_num + ".txt");
      string ip;
      while ((ip = file.ReadLine()) != null) {
        pingClient[clientNum] = new IPEndPoint(IPAddress.Parse(ip), pingClientPort);
        messageClient[clientNum++] = new IPEndPoint(IPAddress.Parse(ip), messageClientPort);
      }
      file.Close();

      //타이머 함수 등록
      rerenderTimer.Elapsed += RenderView;
      rerenderTimer.Enabled = true;
    }

    public void SetButtonList(List<Button> list) {
      buttonList = list;
      DrawButtonList();
    }

    // 상태확인하는 타이머 파라미터 (ms마다 등록한 함수 호출)
    public System.Timers.Timer rerenderTimer = new System.Timers.Timer(5000);

    private void RenderView(Object source, System.Timers.ElapsedEventArgs e) {
      DrawButtonList();
      CheckStatus().Wait(1000);
    }

    private Task CheckStatus() {
      for (int i = 0; i < clientNum; i++) {
        Console.WriteLine("send ping" + i);
        IsAlive(i);
        Console.WriteLine("get ack" + i);
      }
      return null;
    }
    private void DrawButtonList() {
      if (buttonList != null) {
        for (int i = 0; i < clientNum; i++) {
          buttonList.ElementAt(i).Dispatcher.Invoke(
            () => {
              switch (clientPowerState[i]) {
                case (int)PowerState.OFF:
                  buttonList.ElementAt(i).Background = Brushes.Gray;
                  break;
                case (int)PowerState.SUSPEND:
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

    public void IsAlive(int targetPc) {
      SendPing(targetPc);
      string receviedMessage = ReceivePing(targetPc);
      if (receviedMessage == null)
        clientPowerState[targetPc] = (int)PowerState.OFF;
      else if (receviedMessage == "suspend")
        clientPowerState[targetPc] = (int)PowerState.SUSPEND;
      else
        clientPowerState[targetPc] = (int)PowerState.ON;
    }

    public string[] GetAllData(int targetPc) {
      SendMessage(targetPc, "all");
      string result = ReceiveMessage(targetPc);
      if (result != null)
        return result.Split(' ');
      else
        return null;
    }

    public string GetIPAddress(int targetPc) {
      SendMessage(targetPc, "ip");
      return ReceiveMessage(targetPc);
    }
    public string GetMacAddress(int targetPc) {
      SendMessage(targetPc, "mac");
      return ReceiveMessage(targetPc);
    }
    public async Task<string> GetCpuUsageAsync(int targetPc) {
      SendMessage(targetPc, "cpu");
      
      return await Task.Run(() => ReceiveMessage(targetPc));
    }
    public async Task<string> GetRamRemainAsync(int targetPc) {
      SendMessage(targetPc, "ram");
      return await Task.Run(() => ReceiveMessage(targetPc));
    }
    public string GetHddUsage(int targetPc) {
      SendMessage(targetPc, "hdd");
      return ReceiveMessage(targetPc);
    }

    public string ReceivePing(int targetPc) {
      UdpClient pingServer = new UdpClient(pingServerPort);
      try {
        pingServer.Client.ReceiveTimeout = 1000;
        pingServer.Connect(pingClient[targetPc]);
        byte[] bytes = pingServer.Receive(ref pingClient[targetPc]);
        return Encoding.UTF8.GetString(bytes);
      } catch (ObjectDisposedException e) {
        Console.WriteLine(e);
        Console.WriteLine("연결이 종료되었습니다.");
        return null;
      } catch (SocketException e) {
        Console.WriteLine(e);
        Console.WriteLine("연결이 끊겼습니다.");
        return null;
      } finally {
        pingServer.Close();
      }
    }

    public string ReceiveMessage(int targetPc) {
      UdpClient messageServer = new UdpClient(messageServerPort);
      try {
        messageServer.Client.ReceiveTimeout = 1000;
        messageServer.Connect(messageClient[targetPc]);
        byte[] bytes = messageServer.Receive(ref messageClient[targetPc]);
        Console.WriteLine("get message");
        return Encoding.UTF8.GetString(bytes);
      } catch (ObjectDisposedException) {
        Console.WriteLine("연결이 종료되었습니다.");
        return null;
      } catch (SocketException) {
        Console.WriteLine("연결이 끊겼습니다.");
        return null;
      } finally {
        messageServer.Close();
      }
    }

    public void SendPing(int targetPc) {
      if (pingClient[targetPc] != null) {
        UdpClient server = new UdpClient();
        byte[] data = Encoding.UTF8.GetBytes("check");
        try {
          server.Connect(pingClient[targetPc]);
          server.Send(data, data.Length);  
        } catch (ObjectDisposedException) {
          Console.WriteLine("연결이 종료되었습니다.");
        } catch (SocketException) {
          Console.WriteLine("연결이 끊겼습니다.");
        } finally {
          server.Close();
        }
      }
    }

    public void SendMessage(int targetPc, string message) {
      if (messageClient[targetPc] != null) {
        UdpClient server = new UdpClient();
        byte[] data = Encoding.UTF8.GetBytes(message);
        try {
          server.Connect(messageClient[targetPc]);
          server.Send(data, data.Length);
        } catch (ObjectDisposedException) {
          Console.WriteLine("연결이 종료되었습니다.");
        } catch (SocketException) {
          Console.WriteLine("연결이 끊겼습니다.");
        } finally {
          server.Close();
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
      for (int i = 0; i < clientNum; i++) {
        PcPowerOn(i);
      }
    }
    // ! -----------------------------

    public void PcPowerOff(int targetPc) {
      SendMessage(targetPc, "off");
    }

    // TODO: 모두 끝날 때까지 대기하는 로직 추가.
    public void AllPcPowerOff() {
      for (int i = 0; i < clientNum; i++) {
        PcPowerOff(i);
      }
    }

    public void PcPowerReboot(int target_pc) {
      SendMessage(target_pc, "reboot");
    }

    // TODO: 모두 끝날 때까지 대기하는 로직 추가.    
    public void AllPcPowerReboot() {
      for (int i = 0; i < clientNum; i++) {
        PcPowerReboot(i);
      }
    }
  }
}
