using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MetroDemo.lib.Common;

namespace MetroDemo.Lib
{
    public static class Download
    {
        #region 文件资源下载相关
        public static void DownloadBegin(Chord chord,UserInfo userInfo, Node node)
        {
            chord.InsertNode(node);

            Thread thread = new Thread(() => DownloadFile(chord,userInfo, node))
            {
                IsBackground = true
            };
            thread.Start();
        }

        public static void DownloadFile(Chord chord,UserInfo userInfo, Node node)
        {
            string filePath = userInfo.downloadPath + node.keyName + "." + node.fileType;
            if (File.Exists(filePath))
            {
                // Display("文件已下载");
                return;
            }

            foreach (var source in node.sources)
            {
                TcpClient client = new TcpClient();
                client.Connect(source.sourceIP, port);
                NetworkStream stream = client.GetStream();
                Datagram datagram = new Datagram
                {
                    Type = DatagramType.isFileAvailable,
                    Message = node.ToString()
                };
                byte[] data = datagram.ToByte();
                stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
                stream.Write(data, 0, data.Length);

                if (stream.CanRead)
                {
                    int readBytes = 0;
                    Byte[] buffer = new Byte[intToByteLength];
                    stream.Read(buffer, 0, buffer.Length);
                    int size = BitConverter.ToInt32(buffer, 0);
                    buffer = new Byte[size];
                    readBytes = stream.Read(buffer, 0, buffer.Length);
                    if (readBytes == size)
                    {
                        datagram = Datagram.Convert(buffer, size);

                        if (datagram.Type == DatagramType.fileAvailable)
                        {
                            node.AddSource(chord.local.ip, filePath);
                            Node localNode = userInfo.AddDownloadNode(node);

                            Datagram datagram1 = new Datagram
                            {
                                Type = DatagramType.updateSources,
                                Message = node.ToString(),
                            };
                            SendDatagram(chord.GetNodePostion(node), datagram1);
                            SaveFile(localNode, source, filePath);

                            client.Close();
                            stream.Close();
                            return;

                        }
                        else if (datagram.Type == DatagramType.fileUnAvailable)
                        {
                            node.RemoveSource(source);
                            Datagram datagram1 = new Datagram
                            {
                                Type = DatagramType.updateSources,
                                Message = node.ToString(),
                            };
                            SendDatagram(chord.GetNodePostion(node), datagram1);
                        }
                    }

                }
                client.Close();
                stream.Close();


            }
        }

        public static void SaveFile(Node local, Source source, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);

            TcpClient client = new TcpClient();
            client.Connect(source.sourceIP, port);
            NetworkStream stream = client.GetStream();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.downloadFile,
                Message = source.sourcePath
            };
            byte[] data = datagram.ToByte();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);
            byte[] buffer = new byte[1024];

            int length = 0;
            long readSize = 0;
            long fileSize = long.Parse(local.fileSize);
            while (readSize < fileSize)
            {
                length = stream.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer, 0, length);
                fileStream.Flush();
                readSize += length;
                local.saveSize = readSize.ToString();

            }
            fileStream.Close();
            local.saveSize = readSize.ToString();
            client.Close();
            stream.Close();



        }
        #endregion

    }
}
