using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Controls;
using System.IO;
using System.Threading;

namespace MetroDemo.lib
{
    public struct SourceIP
    {
        public SourceIP(string str)
        {
            ip = str;
            sha1Code = ip.GetSha1Code();
        }

        public string ip { set; get; }
        public string sha1Code { set; get; }
    }


    public static class Common
    {
        public const int hashBit=160;
        public const int port = 4444;
        public const int intToByteLength = 4;
        public const int replayPort = 4448;
        public const int blockSize = 1024 * 4096;


        public static IPAddress GetLocalIP()
        {
         /*   string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry host;
            host = Dns.GetHostEntry(hostName);

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.ToString().Substring(0, 3) == "172") //学校局域网。测试
                    return ip;
            }

            return host.AddressList[0];*/
           // return IPAddress.Parse("172.19.151.89");
            return IPAddress.Parse("127.0.0.1");
        }

        public static IPAddress GetTargetIP()       //暂时如此 待改进
        {
            /*  string hostName = Dns.GetHostName();   //获取本机名
              IPHostEntry host;
              host = Dns.GetHostEntry(hostName);

              foreach (IPAddress ip in host.AddressList)
              {
                  if (ip.ToString().Substring(0, 3) == "172") //学校局域网。测试
                      return ip;
              }
              return host.AddressList[0];*/
            return IPAddress.Parse("127.0.0.1");
        }


        #region 辅助

        public static void Display(string str,TextBlock msgRecord)
        {
            msgRecord.Dispatcher.Invoke(new Action(() =>
            {
                msgRecord.Text += str + "\n";
            }));
        }


        public static void SendDatagram(string target, Datagram datagram)
        {
            SendDatagram(IPAddress.Parse(target), datagram);
        }

        public static void SendDatagram(IPAddress target, Datagram datagram)
        {
            TcpClient client = new TcpClient();
            client.Connect(target, port);
            NetworkStream stream = client.GetStream();

            byte[] data = datagram.ToByte();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);

            client.Close();
            stream.Close();
        }

        public static IPAddress GetTargetSuc(IPAddress targetIP)
        {
            TcpClient client = new TcpClient();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.getSuc
            };
            client.Connect(targetIP, port);
            NetworkStream stream = client.GetStream();

            byte[] data = datagram.ToByte();

            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);

            if (stream.CanRead)
            {
                int readBytes = 0;
                Byte[] buffer = new Byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int size = BitConverter.ToInt32(buffer, 0);
                buffer = new Byte[size];
                readBytes = stream.Read(buffer, 0, buffer.Length);
                if (readBytes == size)
                {
                    datagram = Datagram.Convert(buffer, size);
                    if (datagram.Type == DatagramType.returnSuc)
                    {
                        client.Close();
                        stream.Close();
                        return IPAddress.Parse(datagram.Message);
                    }
                }
            }
            client.Close();
            stream.Close();

            return null;
        }

        public static string GetTargetSuc(string tar)
        {
            return GetTargetSuc(IPAddress.Parse(tar)).ToString();
        }

        public static IPAddress GetTargetPre(IPAddress targetIP)
        {
            TcpClient client = new TcpClient();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.getPre
            };
            client.Connect(targetIP, port);
            NetworkStream stream = client.GetStream();

            byte[] data = datagram.ToByte();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);
            if (stream.CanRead)
            {
                int readBytes = 0;
                Byte[] buffer = new Byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int size = BitConverter.ToInt32(buffer, 0);
                buffer = new Byte[size];
                readBytes = stream.Read(buffer, 0, buffer.Length);
                if (readBytes == size)
                {
                    datagram = Datagram.Convert(buffer, size);
                    if (datagram.Type == DatagramType.returnPre)
                    {
                        client.Close();
                        stream.Close();
                        return IPAddress.Parse(datagram.Message);
                    }
                }
            }
            client.Close();
            stream.Close();
            return null;
        }
        public static string GetTargetPre(string tar)
        {
            return GetTargetPre(IPAddress.Parse(tar)).ToString();
        }

        #endregion

        public static Nodes GetNodesList(IPAddress target)
        {
            TcpClient client = new TcpClient();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.getNodesList
            };
            client.Connect(target, port);
            NetworkStream stream = client.GetStream();

            byte[] data = datagram.ToByte();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);
            if (stream.CanRead)
            {
                int readBytes = 0;
                Byte[] buffer = new Byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int size = BitConverter.ToInt32(buffer, 0);
                buffer = new Byte[size];
                readBytes = stream.Read(buffer, 0, buffer.Length);
                if (readBytes == size)
                {
                    datagram = Datagram.Convert(buffer, size);
                    if (datagram.Type == DatagramType.retNodesList)
                    {
                        client.Close();
                        stream.Close();
                        return Nodes.Convent(datagram.Message);
                    }
                }
            }
            client.Close();
            stream.Close();
            return new Nodes(target.ToString());
            
           
        }

        public static void ShowNodes(ListView listView,Nodes nodes)
        {
            listView.Items.Clear();
            foreach (var i in nodes)
            {
                listView.Items.Add(i);
            }
        }



       

    }
}
