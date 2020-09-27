#define HEAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static MetroDemo.lib.Common;
using System.IO;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace MetroDemo.lib
{
    
    public class Chord:IDisposable
    {
        public SourceIP local = new SourceIP(GetLocalIP().ToString());
        Nodes nodes = new Nodes();
        List<SourceIP> fingerTable = new List<SourceIP>(hashBit);
        public IPAddress sucIP;   //后节点
        public IPAddress preIP;    //前节点

        TcpListener server = new TcpListener(GetLocalIP(), port);
#if (HEAD)
        UdpClient udpListener = new UdpClient(udp_port);  //默认0.0.0.0 本机上的所有IPV4地址 主机
#else
        UdpClient udpListener = new UdpClient(new IPEndPoint(GetLocalIP(),port));   //从机
#endif
        Thread listenThread =null;  
        TextBlock msgRecord = null;
    

        public Chord()
        {
            ListenStart();
            sucIP = GetLocalIP();
            preIP = GetLocalIP();

            for (int i = 0; i < hashBit; i++)
            {
                fingerTable.Add(local);
            }
            
            FreshFinTab();

        }


       


#region Listen
        /// <summary>
        /// 监听所有连接进来的tcp请求，也就是接受消息并作出响应的反应
        /// </summary>
        /// 
        private void ListenStart()
        {
            server.Start();
            listenThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenThread.Start();

            Thread udpThread = new Thread(UdpListener)
            {
                IsBackground = true
            };
            udpThread.Start(udpListener);

        }

        private void Listen()
        {
            TcpClient client = null;   
            try
            {
                while(true)
                {
                    client = server.AcceptTcpClient();
                    // Display(client.Client.RemoteEndPoint.ToString());
                    Thread thread = new Thread(ReceiveData)
                    {
                        IsBackground = true
                    };
                    thread.Start(client);
                }
                
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        private void ReceiveData(object obj)
        {
            TcpClient client = (TcpClient)obj;

            NetworkStream stream = client.GetStream();
            if (stream.CanRead)
            {
                do
                {
                    int readBytes = 0;
                    byte[] buffer = new Byte[intToByteLength];
                    stream.Read(buffer, 0, buffer.Length);
                    int size = BitConverter.ToInt32(buffer, 0);

                    buffer = new Byte[size];
                    readBytes = stream.Read(buffer, 0, buffer.Length);
                    if (readBytes == size)
                    {
                        Datagram datagram = Datagram.Convert(buffer);
                        Display(datagram.Type + " : " + datagram.Message);
                        Execute(datagram, stream);
                    }
                    else
                    {
                        Display("数据包不完整，已丢弃。");
                    }
                }
                while (stream.DataAvailable);              
            }
        }
#endregion

        /// <summary>
        /// 对消息的执行。
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="stream"></param>
        private void Execute(Datagram datagram, NetworkStream stream)
        {
         //   Display(datagram.Type + " : " + datagram.Message);
            Datagram retDatagram = null;
            string path;
            Byte[] ret;
            switch (datagram.Type)
            {
                case DatagramType.chat:
                    break;
                case DatagramType.getSuc:
                    retDatagram = new Datagram
                    {
                        Type = DatagramType.returnSuc,
                        Message = sucIP.ToString(),
                        
                    };
                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(),0,intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;
                case DatagramType.getPre:
                    retDatagram = new Datagram
                    {
                        Type = DatagramType.returnPre,
                        Message = preIP.ToString(),

                    };
                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(), 0, intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;
                case DatagramType.onChordPre:
                    string joinIP = datagram.Message;
                    if (joinIP.GetSha1Code().CompareTo(preIP.ToString().GetSha1Code()) > 0)
                    {
                        for(int i=nodes.Count-1;i>=0;i--)
                        {
                            if (joinIP.GetSha1Code().CompareTo(nodes[i].sha1Code) >= 0)
                            {
                                nodes.Remove(nodes[i]);
                            }
                        }

                    }
                    else
                    {
                        for (int i = nodes.Count - 1; i >= 0; i--)
                        {
                            if (joinIP.GetSha1Code().CompareTo(nodes[i].sha1Code) >= 0 || nodes[i].sha1Code.CompareTo(preIP.ToString().GetSha1Code()) > 0)
                            {
                                nodes.Remove(nodes[i]);
                            }
                        }
                       
                    }

                    preIP = IPAddress.Parse(joinIP);
                    FreshFinTab();

                    break;
                case DatagramType.onChordSuc:
                    sucIP = IPAddress.Parse(datagram.Message);

                    FreshFinTab();
                    break;

                case DatagramType.offChordPre:
                    preIP = IPAddress.Parse(datagram.Message);
                    FreshFinTab();
                    break;
                case DatagramType.offChordSuc:
                    sucIP = IPAddress.Parse(datagram.Message);
                    FreshFinTab();
                    break;
                case DatagramType.addNodeFromPre:
                    Nodes lists = Nodes.Convent(datagram.Message);
                    foreach(var te in lists)
                    {
                        nodes.Insert(te);
                    }
                    break;
                case DatagramType.chatAll:
                    if(datagram.FromAddress!=GetLocalIP().ToString())
                    {
                        SendDatagram(sucIP, datagram);   
                    }
                    break;

                case DatagramType.insertNode:
                    InsertNode(Node.Convert(datagram.Message));
                    break;

                case DatagramType.getNodePosition:

                    retDatagram = new Datagram
                    {
                        Message = GetNodePostion(Node.Convert(datagram.Message))
                    };
                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(), 0, intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;
                case DatagramType.getNodesList:
                    retDatagram = new Datagram
                    {
                        Type = DatagramType.retNodesList,
                        Message = nodes.ToString()
                    };
                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(), 0, intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;
                case DatagramType.freshFinTab:
                    FreshFinTab();
                    break;


                case DatagramType.downloadFile:
                    int index = int.Parse(datagram.Message.Split(new Char[] { ',' },2)[0]);
                    path = datagram.Message.Split(new Char[] { ',' }, 2)[1];

                    if (File.Exists(path))
                    {
                        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                        fileStream.Seek((long)index * blockSize, SeekOrigin.Begin);
                        int readSize = 0;
                        int fileSize = 0;

                        if(index == fileStream.Length/blockSize)
                        {
                            fileSize = blockSize - (int)fileStream.Length % blockSize;
                        }

                        while (fileSize < blockSize ) 
                        {
                            ret = new byte[4096];
                            readSize = fileStream.Read(ret, 0, ret.Length);
                            stream.Write(ret, 0, readSize);
                            fileSize += readSize;
                        }
                        fileStream.Close();
                        stream.Flush();
                    }
                    break;
                case DatagramType.isFileAvailable:
                    retDatagram = new Datagram()
                    {
                        Type = DatagramType.fileUnAvailable
                    };

                    Node node = Node.Convert(datagram.Message);

                    foreach(var te in node.sources)
                    {
                        if(te.sourceIP==local.ip)
                        {
                            path = te.sourcePath;                      
                            if (File.Exists(path))
                            {
                                FileInfo fileInfo = new FileInfo(path);
                                if (fileInfo.Length ==long.Parse(node.fileSize))
                                {
                                    retDatagram.Message = "嘤嘤嘤嘤嘤嘤嘤嘤";
                                    retDatagram.Type = DatagramType.fileAvailable;
                                    break;
                                }
                               
                            }
                       
                        }
                    }

                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(), 0, intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;

                case DatagramType.updateSources:
                    UpdateNodeSources(Node.Convert(datagram.Message));
                    break;

                case DatagramType.getFinTab:
                    retDatagram = new Datagram()
                    {
                        Message = fingerTable.TabToString(),
                    };
                    ret = retDatagram.ToBytes();
                    stream.Write(retDatagram.AllSize.GetBytes(), 0, intToByteLength);
                    stream.Write(ret, 0, ret.Length);
                    break;
            }
        }

       

        public void Join()
        {
            IPAddress targetIP = GetTargetIP();
            IPAddress local = GetLocalIP();

            while (true)
            {
                if (local.GetSha1Code().CompareTo(targetIP.GetSha1Code()) > 0)
                {
                    IPAddress suc = GetTargetSuc(targetIP);
                    if (suc.GetSha1Code().CompareTo(local.GetSha1Code()) > 0)
                    {
                        JoinChord(targetIP, suc); 
                        break;
                    }
                    else if (!(suc.GetSha1Code().CompareTo(targetIP.GetSha1Code()) > 0))
                    {
                        JoinChord(targetIP, suc);
                        break;
                    }
                    else
                    {
                        targetIP = suc;
                    }
                }
                else if (local.GetSha1Code().CompareTo(targetIP.GetSha1Code()) < 0)
                {
                    IPAddress pre = GetTargetSuc(targetIP);
                    if (pre.GetSha1Code().CompareTo(local.GetSha1Code()) < 0)
                    {
                        JoinChord(pre, targetIP);
                    }
                    else if (!(pre.GetSha1Code().CompareTo(targetIP.GetSha1Code()) < 0))
                    {
                        JoinChord(pre, targetIP);
                    }
                    else
                    {
                        targetIP = pre;
                    }
                }
                else
                {
                    break;
                }
            }


        }

        public void ChatToALL(string str)
        {
            Datagram datagram = new Datagram
            {
                Message = str,
                Type = DatagramType.chatAll
            };
            SendDatagram(sucIP, datagram);
        }

        void UpdateNodeSources(Node node)
        {
            foreach (var i in nodes)
            {
                if (i.sha1Code == node.sha1Code)
                {
                    i.UpdateNodeSources(node.sources);
                }
            }
        }

#region 加入和退出
        void JoinChord(IPAddress pre, IPAddress suc)
        {
            Nodes lists = GetNodesList(suc);

            if (local.sha1Code.CompareTo(pre.ToString().GetSha1Code()) > 0)
            {
                foreach( var temp in lists)
                {
                    if(local.sha1Code.CompareTo(temp.sha1Code)>=0)
                    {
                        nodes.Insert(temp);
                    }
                }
            }
            else
            {
                foreach (var temp in lists)
                {
                    if (local.sha1Code.CompareTo(temp.sha1Code) >= 0 ||temp.sha1Code.CompareTo(pre.ToString().GetSha1Code())>0)
                    {
                        nodes.Insert(temp);
                    }
                }
            }


            Datagram datagram = new Datagram
            {
                Type = DatagramType.onChordSuc,
                Message = GetLocalIP().ToString(),
            };
            SendDatagram(pre, datagram);

            datagram = new Datagram
            {
                Type = DatagramType.onChordPre,
                Message = GetLocalIP().ToString(),
            };
            SendDatagram(suc, datagram);
           
            preIP = pre;
            sucIP = suc;

        }
     

        public void Leave()
        {
            if (preIP.ToString() == GetLocalIP().ToString() && sucIP.ToString() == GetLocalIP().ToString())
            {
                return;
            }
            Datagram datagram = new Datagram
            {
                Type = DatagramType.addNodeFromPre,
                Message = nodes.ToString(),
            };
            SendDatagram(sucIP, datagram);

            datagram = new Datagram
            {
                Type = DatagramType.offChordPre,
                Message =preIP.ToString(),
            };
            SendDatagram(sucIP, datagram);

            datagram = new Datagram
            {
                Type = DatagramType.offChordSuc,
                Message = sucIP.ToString(),
            };
            SendDatagram(preIP, datagram);


            sucIP = GetLocalIP();
            preIP = GetLocalIP();
        }
#endregion

#region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。

                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                Leave();
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~Chord()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
             GC.SuppressFinalize(this);
        }
#endregion
 

        private void FreshFinTab()
        {
            for(int i=0;i<hashBit;i++)
            {
                char[] tempHashArr = local.sha1Code.ToCharArray();
                tempHashArr[hashBit / 4 - 1 - i / 4] += (char)(2 << (i % 4));  //add 1，2，4，8，16，32，64，128。。。

                for (int t = 0; t < hashBit / 4; t++)
                {
                    if (tempHashArr[hashBit / 4 - 1 - t] > '9' && tempHashArr[hashBit / 4 - 1 - t] < '@')
                    {
                        tempHashArr[hashBit / 4 - 1 - t] = (char)(tempHashArr[hashBit / 4 - 1 - t] - ':' + 'a');
                    }
                    else if (tempHashArr[hashBit / 4 - 1 - t] > '?' && tempHashArr[hashBit / 4 - 1 - t] < 'B')
                    {
                        tempHashArr[hashBit / 4 - 1 - t] = (char)(tempHashArr[hashBit / 4 - 1 - t] - 16);
                        if ((hashBit / 4 - 2 - t) >= 0)
                        {
                            tempHashArr[hashBit / 4 - 2 - t] +=(char)1;
                        }
                    }
                    else if (tempHashArr[hashBit / 4 - 1 - t] > 'f')
                    {
                        tempHashArr[hashBit / 4 - 1 - t] = (char)(tempHashArr[hashBit / 4 - 1 - t] - 'g' + '0');
                        if ((hashBit / 4 - 2 - t) >= 0)
                        {
                            tempHashArr[hashBit / 4 - 2 - t] += (char)1;
                        }
                    }
                }   // end add，。
                string tempHash = new string(tempHashArr);
                string destNode = sucIP.ToString();
                
                while(true)
                {
                   if(tempHash.CompareTo(local.sha1Code)>0)
                    {
                        if (destNode.GetSha1Code().CompareTo(tempHash) >= 0)
                        {
                            fingerTable[i] = new SourceIP(destNode);
                            break;
                        }
                        else if(local.sha1Code.CompareTo(destNode.GetSha1Code())>=0)
                        {
                            fingerTable[i] = new SourceIP(destNode);
                            break;
                        }
                        else
                        {
                            destNode = GetTargetSuc(destNode);
                        }
                    }
                    else
                    {
                        while (destNode.GetSha1Code().CompareTo(local.sha1Code) > 0)
                        {
                            destNode = GetTargetSuc(destNode);
                        }
                        if(destNode.GetSha1Code().CompareTo(tempHash)>=0)
                        {
                            fingerTable[i] = new SourceIP(destNode);
                            break;
                        }
                        else
                        {
                            destNode = GetTargetSuc(destNode);
                        }
                    }
                }
            }
        }

#region 节点资源相关

        void InsertNode(string ip,Node node)
        {
            Datagram datagram = new Datagram
            {
                Type = DatagramType.insertNode,
                Message = node.ToString()
            };
            SendDatagram(IPAddress.Parse(ip), datagram);
        }

        void AddNodeIfExist(Node node)
        {
            bool has = false;
            foreach(var te in nodes)
            {
                if(te.sha1Code.CompareTo(node.sha1Code)==0)
                {
                    var exp = node.sources.Where(a => !te.sources.Exists(t => a.sourceIP.Contains(t.sourceIP))).ToList();
                    foreach(var te1 in exp)
                    {
                        te.sources.Add(te1);
                    }
                    has = true;
                    break;
                }
            }
            if(!has)
                nodes.Add(node);
        }

        public void InsertNode(Node node)// 若node已存在，则只添加没有的source
        {
            if(local.sha1Code.CompareTo(sucIP.GetSha1Code())==0)
            {
                // nodes.Add(node);
                AddNodeIfExist(node);
            }
            else if((local.sha1Code.CompareTo(node.sha1Code)>0&&node.sha1Code.CompareTo(preIP.GetSha1Code())>0)||
                (preIP.GetSha1Code().CompareTo(local.sha1Code)>0&&local.sha1Code.CompareTo(node.sha1Code)>0)||
                (node.sha1Code.CompareTo(preIP.GetSha1Code())>0&&preIP.GetSha1Code().CompareTo(local.sha1Code)>0))
            {
                // nodes.Add(node);
                AddNodeIfExist(node);
            }
            else if((sucIP.GetSha1Code().CompareTo(node.sha1Code)>0&&node.sha1Code.CompareTo(local.sha1Code)>0)||
                (node.sha1Code.CompareTo(local.sha1Code)>0&&local.sha1Code.CompareTo(sucIP.GetSha1Code())>0)||
                (sucIP.GetSha1Code().CompareTo(node.sha1Code)>0&&local.sha1Code.CompareTo(sucIP.GetSha1Code())>0))
            {
                InsertNode(sucIP.ToString(), node);
            }
           
            else if (node.sha1Code.CompareTo(local.sha1Code) > 0)
            {
                for (int i = hashBit-1; i >=0; i--)
                {
                    if(fingerTable[i].sha1Code.CompareTo(local.sha1Code)>0&&fingerTable[i].sha1Code.CompareTo(node.sha1Code)<0)
                    {
                        InsertNode(fingerTable[i].ip, node);
                    }
                }
            }
            else
            {
                for (int i = hashBit - 1; i >= 0; i--)
                {
                    if (fingerTable[i].sha1Code.CompareTo(local.sha1Code) > 0 || fingerTable[i].sha1Code.CompareTo(node.sha1Code) < 0)
                    {
                        InsertNode(fingerTable[i].ip, node);
                    }
                }
            }
            
        }
        string GetNodePostion(string ip,Node node)
        {
            string retStr = "";
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(ip), port);
            NetworkStream stream = tcpClient.GetStream();

            Datagram datagram = new Datagram
            {
                Type = DatagramType.getNodePosition,
                Message = node.ToString()
            };
            byte[] vs = datagram.ToBytes();
            stream.Write(datagram.AllSize.GetBytes(), 0, intToByteLength);
            stream.Write(vs, 0, vs.Length);

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
                    datagram = Datagram.Convert(buffer);
                    retStr = datagram.Message;

                }
            }
            tcpClient.Close();
            stream.Close();
            return retStr;
        }

        public string GetNodePostion(Node node)
        {
            if (local.sha1Code.CompareTo(sucIP.GetSha1Code()) == 0)
            {
                return local.ip;
            }
            else if ((local.sha1Code.CompareTo(node.sha1Code) > 0 && node.sha1Code.CompareTo(preIP.GetSha1Code()) > 0) ||
                (preIP.GetSha1Code().CompareTo(local.sha1Code) > 0 && local.sha1Code.CompareTo(node.sha1Code) > 0) ||
                (node.sha1Code.CompareTo(preIP.GetSha1Code()) > 0 && preIP.GetSha1Code().CompareTo(local.sha1Code) > 0))
            {
                return local.ip;
            }
            else if ((sucIP.GetSha1Code().CompareTo(node.sha1Code) > 0 && node.sha1Code.CompareTo(local.sha1Code) > 0) ||
                (node.sha1Code.CompareTo(local.sha1Code) > 0 && local.sha1Code.CompareTo(sucIP.GetSha1Code()) > 0) ||
                (sucIP.GetSha1Code().CompareTo(node.sha1Code) > 0 && local.sha1Code.CompareTo(sucIP.GetSha1Code()) > 0))
            {
                return sucIP.ToString();
            }

            else if (node.sha1Code.CompareTo(local.sha1Code) > 0)
            {
                for (int i = hashBit - 1; i >= 0; i--)
                {
                    if (fingerTable[i].sha1Code.CompareTo(local.sha1Code) > 0 && fingerTable[i].sha1Code.CompareTo(node.sha1Code) < 0)
                    {
                        return GetNodePostion(fingerTable[i].ip, node);
                    }
                }
            }
            else
            {
                for (int i = hashBit - 1; i >= 0; i--)
                {
                    if (fingerTable[i].sha1Code.CompareTo(local.sha1Code) > 0 || fingerTable[i].sha1Code.CompareTo(node.sha1Code) < 0)
                    {
                        return GetNodePostion(fingerTable[i].ip, node);
                    }
                }
            }


            return "";
        }
#endregion


        public void BindDsipBord(TextBlock textBlock)
        {
            msgRecord = textBlock;
        }

        public void Display(string str)
        {
            if(msgRecord!=null)
            {
                msgRecord.Dispatcher.Invoke(new Action(() =>
                {
                    msgRecord.Text += str + "\n";
                }));
            }
        }

#region 前台显示函数
        public void ShowHostList(ListView listView)
        {

            listView.Items.Clear();
            listView.Items.Add(local);
            IPAddress target = sucIP;
            while (target.ToString() != GetLocalIP().ToString())
            {
                SourceIP item = new SourceIP(target.ToString());
                listView.Items.Add(item);
                target = GetTargetSuc(target);
            }
        }
        
        public void ShowSourcesList(ListView listView)
        {
            IPAddress target = sucIP;

            listView.Items.Clear();
            foreach (var i in nodes)
            {
                listView.Items.Add(i);
            }
            while (target.ToString() != GetLocalIP().ToString())
            {
                Nodes temp = GetNodesList(target);
                foreach (var i in temp)
                {
                    listView.Items.Add(i);
                }
                target = GetTargetSuc(target);
            }
        }
        public void ShowTargetInfo(IPAddress target)
        {
            Display("");
            if (target.ToString() == local.ip)
            {
                Display(target.ToString() + "-----------------------------------------------infomation begin");
                Display("IP:" + target.ToString() + "   SHA1：" + target.ToString().GetSha1Code());
                Display("preIP:" + preIP.ToString() + "   SHA1：" + preIP.ToString().GetSha1Code());
                Display("sucIP:" + sucIP.ToString() + "   SHA1：" + sucIP.ToString().GetSha1Code());

                foreach (var va in nodes)
                {
                    Display("标志：" + va.keyName + " 大小:" + va.fileSize + " 类型:" + va.fileType + "位置：" + nodes.location + " 上传者:" + va.sources[0].sourceIP);
                }
                Display("------------------------------------------------------------------------end");
            }
            else
            {
                Display(target.ToString() + "-----------------------------------------------infomation begin");
                Display("IP:" + target.ToString() + "   SHA1：" + target.ToString().GetSha1Code());
                IPAddress pre = GetTargetPre(target);
                Display("preIP:" + pre.ToString() + "   SHA1：" + pre.ToString().GetSha1Code());
                IPAddress suc = GetTargetSuc(target);
                Display("sucIP:" + suc.ToString() + "   SHA1：" + suc.ToString().GetSha1Code());

                Nodes nodes = GetNodesList(target);
                foreach (var va in nodes)
                {
                    Display("标志：" + va.keyName + " 大小:" + va.fileSize + " 类型:" + va.fileType + "位置：" + nodes.location + " 上传者:" + va.sources[0].sourceIP);
                }
                Display("------------------------------------------------------------------------end");
            }
            Display("");
        }


        public void DispPreSuc()
        {
            Display(preIP.ToString());
            Display(sucIP.ToString());
        }
#endregion
    }
}
