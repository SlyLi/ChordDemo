﻿using MetroDemo.lib;
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
            Thread thread = new Thread(() => DownloadFile(chord,userInfo, node))
            {
                IsBackground = true
            };
            thread.Start();
        }


        public static void DownloadFile(Chord chord,UserInfo userInfo, Node node)
        {
            string filePath = userInfo.downloadPath + node.keyName + "." + node.fileType;
            node.PrepareSources();
            while (node.saveSize != node.fileSize && node.status==Node.NodeStatus.downloading)
            {
                for (int i = 0; i < node.blockNum; i++)
                {
                    if (!node.blocks[i])
                    {
                        if (IsFileAvailable(node, node.blockSource[i]))
                        {
                            SaveFile(i, node, filePath);
                            node.blocks[i] = true;
                            node.OnPropertyChanged("progressValue");
                        }

                    }
                    if (node.status != Node.NodeStatus.downloading)
                        break;
                }
                
            }
            

        }

        public static bool IsFileAvailable(Node node,Source source)
        {
            TcpClient client = new TcpClient();
            client.Connect(source.sourceIP, port);
            NetworkStream stream = client.GetStream();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.isFileAvailable,
                Message = node.ToString()
            };
            byte[] data = datagram.ToBytes();
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
                    datagram = Datagram.Convert(buffer);

                    if (datagram.Type == DatagramType.fileAvailable)
                    {
                        return true;
                    }
                }
            }
            client.Close();
            stream.Close();
            return false;
        }

        public static void SaveFile(int i,Node node, string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            TcpClient client = new TcpClient();
            client.Connect(node.blockSource[i].sourceIP, port);
            NetworkStream stream = client.GetStream();
            Datagram datagram = new Datagram
            {
                Type = DatagramType.downloadFile,
                Message = i.ToString() + "," + node.blockSource[i].sourcePath 
            };
            byte[] data = datagram.ToBytes();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(data, 0, data.Length);
            byte[] buffer = new byte[1024];

            int length = 0;
            int readSize = 0;
            fileStream.Seek((long)i * blockSize, SeekOrigin.Begin);
            if(i==node.blockNum-1)
            {
                readSize = blockSize - (int)long.Parse(node.fileSize) % blockSize;
            }
            while (readSize < blockSize)
            {
                length = stream.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer, 0, length); 
                readSize += length;

            }
            fileStream.Flush();
            fileStream.Close();
            client.Close();
            stream.Close();


        }
        #endregion
        public static void DownloadPause(Node node)
        {
            node.DownloadStatusPause();
        }

        public static void DownloadContinue(Chord chord, UserInfo userInfo, Node node)
        {
            node.DownloadStatusContinue();
            DownloadBegin(chord, userInfo, node);
        }
    }
}
