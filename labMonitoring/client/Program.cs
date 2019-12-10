using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Management;
using System.IO;
using Microsoft.Win32;

namespace client
{

    class Program
    {
        private static IPEndPoint serverEndPoint;
        private static UdpClient server;
        private static byte[] data;
        private static byte[] sendData;
        private static IPEndPoint messageSender;
        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        private static PerformanceCounter hddCounter;
        

        private static void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    break;
                case PowerModes.Suspend:
                    sendData = Encoding.UTF8.GetBytes("suspend");
                    server.Send(sendData, sendData.Length, messageSender);
                    break;
            }
        }
        static void Main(string[] args)
        {
            SystemEvents.PowerModeChanged += OnPowerChange;

            // booting 시 자동 실행
            try
            {
                String runKey = @"SOFTWARE\Microsoft\\Windows\CurrentVersion\Run";
                RegistryKey strUpKey = Registry.LocalMachine.OpenSubKey(runKey);
                if (strUpKey.GetValue("StartupTest") == null)
                {
                    strUpKey.Close();
                    strUpKey = Registry.LocalMachine.OpenSubKey(runKey, true);
                    strUpKey.SetValue("client.exe", System.IO.Directory.GetCurrentDirectory() + "\\client.exe");
                }
                Console.WriteLine("Add Startup Success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine(GetMacAddress());
            Console.WriteLine(GetIPAddress());
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            hddCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            cpuCounter.NextValue();
            hddCounter.NextValue();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(GetIPAddress()), 10000);
            server = new UdpClient(serverEndPoint);
            data = new byte[100];
            sendData = new byte[100];
            Receive();
        }


        private static void Receive()
        {
            while (true)
            {
                data = server.Receive(ref messageSender);
                string msg = Encoding.UTF8.GetString(data);
                // 전원 off
                if (msg.Contains("off"))
                {
                    SendAck(msg);
                    //프로그램을 모두 정리하고 전원 off
                    System.Diagnostics.Process.Start("shutdown.exe", "-s");
                }
                // 재부팅
                else if (msg.Contains("reboot"))
                {
                    SendAck(msg);
                    Process.Start("shutdown.exe", "-r");
                }
                else if (msg.Contains("all"))
                {
                    sendData = Encoding.UTF8.GetBytes(GetIPAddress() + " " +
                                                      GetMacAddress() + " " +
                                                      getCurrentCpuUsage() + " " +
                                                      getAvailableRAM() + " " +
                                                      getCurrentHddUsage() + " ");
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }
                // send IP
                else if (msg.Contains("ip"))
                {
                    sendData = Encoding.UTF8.GetBytes(GetIPAddress());
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }
                // send MAC
                else if (msg.Contains("mac"))
                {
                    sendData = Encoding.UTF8.GetBytes(GetMacAddress());
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }

                // send CPU %
                else if (msg.Contains("cpu"))
                {
                    sendData = Encoding.UTF8.GetBytes(getCurrentCpuUsage());
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }

                // send RAM %
                else if (msg.Contains("ram"))
                {
                    sendData = Encoding.UTF8.GetBytes(getAvailableRAM());
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }
                // send HDD %
                else if (msg.Contains("hdd"))
                {
                    sendData = Encoding.UTF8.GetBytes(getCurrentHddUsage());
                    Console.WriteLine("get data");
                    server.Send(sendData, sendData.Length, messageSender);
                }
                else if (msg.Contains("check"))
                {
                    sendData = Encoding.UTF8.GetBytes("alive");
                    server.Send(sendData, sendData.Length, messageSender);
                }
                // send program shutdown
                else if (msg.Contains("end"))
                    break;
            }
        }

        private static void SendAck(string msg)
        {
            sendData = Encoding.UTF8.GetBytes("accepted");
            server.Send(sendData, sendData.Length, messageSender);
        }

        
        //mac주소 얻기
        private static string GetMacAddress()
        {
            return Macformat(NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString());
        }

        private static string Macformat(string str)
        {
            string mac = "";

            char[] chrArr = str.ToCharArray();
            for (int i = 0; i < chrArr.Length; i++)
            {
                if (i % 2 == 0)
                {
                    mac += chrArr[i].ToString();
                }
                else
                {
                    mac += chrArr[i].ToString();
                    if (i != chrArr.Length - 1)
                        mac += ":";
                }
            }
            return mac;
        }
        //ip주소 얻기
        private static string GetIPAddress()
        {
            string ip = "";
            IPAddress[] host = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var item in host)
            {
                if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {

                    ip = item.ToString();
                    if (!ip.Contains("192. 168"))
                    {
                        return ip;
                    }
                }
            }
            return ip;
        }

        public static string getCurrentHddUsage()
        {
            DriveInfo drv = new DriveInfo("C");
            int totalFree = Convert.ToInt32(drv.TotalFreeSpace / 1024 / 1024 / 1024);
            return totalFree.ToString() + "GB";
        }
        public static string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue().ToString() + "%";
        }

        public static string getAvailableRAM()
        {
            ObjectQuery winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);
            ManagementObjectCollection queryCollection = searcher.Get();

            ulong memory = 0;
            foreach (ManagementObject item in queryCollection)
            {
                memory = ulong.Parse(item["TotalVisibleMemorySize"].ToString());
            }
            int maxMem = (int)(memory / 1024);
            float percent = maxMem / ramCounter.NextValue();
            return percent.ToString() + "%";
        }

    }





}
