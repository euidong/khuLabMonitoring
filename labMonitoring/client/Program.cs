using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;

namespace client {

  class Program {

    private const string serverIp = "192.168.10.103"; // server ip를 입력해야합니다, 
    private const int serverPingPort = 44484;
    private const int serverMessagePort = 44485;

    private const int myPingPort = 47214;
    private const int myMessagePort = 47215;

    // * TODO: 해당 위치에 서버 IP를 삽입해야합니다.
    private static IPEndPoint serverPingEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPingPort);
    private static IPEndPoint serverMessageEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverMessagePort);

    public class PingThread {
      private static IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
      public static void DoWork() {
        byte[] receiveData;
        byte[] sendData;
        string msg;
        while (true) {
          // 1. ping이 올 때까지 대기
          UdpClient client = new UdpClient(myPingPort);
          receiveData = client.Receive(ref remoteIpEndPoint);
          msg = Encoding.UTF8.GetString(receiveData);
          Console.WriteLine("get ping");
          if (msg.Contains("check")) {
            // 2. alive 신호 전송
            sendData = Encoding.UTF8.GetBytes("alive");
            client.Connect(serverPingEndPoint);
            client.Send(sendData, sendData.Length);
            client.Close();
            Console.WriteLine("send ack");
          }
        }
      }
    }

    public class MessageThread {
      private static IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
      public static void DoWork() {
        byte[] receiveData;
        byte[] sendData;
        string msg;
        while (true) {
          UdpClient client = new UdpClient(myMessagePort);
          receiveData = client.Receive(ref remoteIpEndPoint);
          msg = Encoding.UTF8.GetString(receiveData);
          client.Connect(serverMessageEndPoint);
          // 전원 off
          if (msg.Contains("off")) {
            sendData = Encoding.UTF8.GetBytes("accepted");
            client.Send(sendData, sendData.Length);
            //프로그램을 모두 정리하고 전원 off
            System.Diagnostics.Process.Start("shutdown.exe", "-s");
          }
          // 재부팅
          else if (msg.Contains("reboot")) {
            sendData = Encoding.UTF8.GetBytes("accepted");
            client.Send(sendData, sendData.Length);
            Process.Start("shutdown.exe", "-r");
          }
          else if (msg.Contains("all")) {
            sendData = Encoding.UTF8.GetBytes(GetIPAddress() + " " +
                                              GetMacAddress() + " " +
                                              getCurrentCpuUsage() + " " +
                                              getAvailableRAM() + " " +
                                              getCurrentHddUsage() + " ");
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }
          // send IP
          else if (msg.Contains("ip")) {
            sendData = Encoding.UTF8.GetBytes(GetIPAddress());
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }
          // send MAC
          else if (msg.Contains("mac")) {
            sendData = Encoding.UTF8.GetBytes(GetMacAddress());
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }

          // send CPU %
          else if (msg.Contains("cpu")) {
            sendData = Encoding.UTF8.GetBytes(getCurrentCpuUsage());
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }

          // send RAM %
          else if (msg.Contains("ram")) {
            sendData = Encoding.UTF8.GetBytes(getAvailableRAM());
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }
          // send HDD %
          else if (msg.Contains("hdd")) {
            sendData = Encoding.UTF8.GetBytes(getCurrentHddUsage());
            Console.WriteLine("get data");
            client.Send(sendData, sendData.Length);
          }
          else if (msg.Contains("check")) {
            sendData = Encoding.UTF8.GetBytes("alive");
            client.Send(sendData, sendData.Length);
          }
          // send program shutdown
          else if (msg.Contains("end"))
            break;
          Console.WriteLine("send data");
          client.Close();
        }
      }
    }

    private static PerformanceCounter cpuCounter;
    private static PerformanceCounter ramCounter;
    private static PerformanceCounter hddCounter;


    static void Main(string[] args) {

      // booting 시 자동 실행
      try {
        String runKey = @"SOFTWARE\Microsoft\\Windows\CurrentVersion\Run";
        RegistryKey strUpKey = Registry.LocalMachine.OpenSubKey(runKey);
        if (strUpKey.GetValue("StartupTest") == null) {
          strUpKey.Close();
          strUpKey = Registry.LocalMachine.OpenSubKey(runKey, true);
          strUpKey.SetValue("client.exe", System.IO.Directory.GetCurrentDirectory() + "\\client.exe");
        }
        Console.WriteLine("Add Startup Success");
      }
      catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
      Console.WriteLine(GetMacAddress());
      Console.WriteLine(GetIPAddress());
      cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
      ramCounter = new PerformanceCounter("Memory", "Available MBytes");
      hddCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
      cpuCounter.NextValue();
      hddCounter.NextValue();

      Receive();
    }


    private static void Receive() {
      Thread pingThread = new Thread(PingThread.DoWork);
      Thread messageThread = new Thread(MessageThread.DoWork);
      pingThread.Start();
      messageThread.Start();
      pingThread.Join();
      messageThread.Join();
    }


    //mac주소 얻기
    private static string GetMacAddress() {
      return Macformat(NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString());
    }

    private static string Macformat(string str) {
      string mac = "";

      char[] chrArr = str.ToCharArray();
      for (int i = 0; i < chrArr.Length; i++) {
        if (i % 2 == 0) {
          mac += chrArr[i].ToString();
        }
        else {
          mac += chrArr[i].ToString();
          if (i != chrArr.Length - 1)
            mac += ":";
        }
      }
      return mac;
    }
    //ip주소 얻기
    private static string GetIPAddress() {
      string ip = "";
      IPAddress[] host = Dns.GetHostAddresses(Dns.GetHostName());
      foreach (var item in host) {
        if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {

          ip = item.ToString();
          if (!ip.Contains("192. 168")) {
            return ip;
          }
        }
      }
      return ip;
    }

    public static string getCurrentHddUsage() {
      DriveInfo drv = new DriveInfo("C");
      int totalFree = Convert.ToInt32(drv.TotalFreeSpace / 1024 / 1024 / 1024);
      return totalFree.ToString() + "GB";
    }
    public static string getCurrentCpuUsage() {
      return cpuCounter.NextValue().ToString() + "%";
    }

    public static string getAvailableRAM() {
      ObjectQuery winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
      ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);
      ManagementObjectCollection queryCollection = searcher.Get();

      ulong memory = 0;
      foreach (ManagementObject item in queryCollection) {
        memory = ulong.Parse(item["TotalVisibleMemorySize"].ToString());
      }
      int maxMem = (int)(memory / 1024);
      float percent = maxMem / ramCounter.NextValue();
      return percent.ToString() + "%";
    }
    }
  }
