using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static MetroDemo.lib.Common;
using Newtonsoft.Json;

namespace MetroDemo.lib
{
    public struct Source
    {
        public Source(string ip,string path)
        {
            sourceIP = ip;
            sourcePath = path;
        }
        public string sourceIP { set; get; }
        public string sourcePath { set; get; }
    }

    public class Nodes :List<Node>
    {
        public string location { set; get; }
        public Nodes(string s)
        {
            location = s;
        }
        public Nodes()
        {
            location = GetLocalIP().ToString();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}",location.ToString());
            foreach (var i in this)
            {
                sb.AppendFormat("○{0}", i.ToString());
            }
            return sb.ToString();
        }

        public static Nodes Convent(string str)
        {
            if (str == "")
                return null;
            else
                return JsonConvert.DeserializeObject<Nodes>(str);

            string[] strs = str.Split('○');
            Nodes nodes = new Nodes(strs[0]);
            
            foreach(var s in strs)
            {
                if (s == strs[0])
                    continue;
                nodes.Add(Node.Convert(s));
            }
            return nodes;
        }
    }

    
    public class Node
    {
        public enum NodeStatus
        {
            noSource,   //没有可用的上传者
            downloaded, //下载完成  
            downloading,    //正在下载  
            pause,  //暂停

        }


        public Node(string key,string size, string path,string type)
        {       
            keyName = key;
            fileSize = size;
            fileType = type;
            sha1Code = key.GetSha1Code();
            AddSource(GetLocalIP().ToString(), path);


        }
        public Node(string key, string size,string type)
        {
            keyName = key;
            sha1Code = key.GetSha1Code();
            fileSize = size;
            fileType = type;
        }

        public Node(string key, string size, string type,string blockStr,int i) //i的值没有用处，只用于区分功能
        {
            keyName = key;
            sha1Code = key.GetSha1Code();
            fileSize = size;
            fileType = type;
            blocks = blockStr.StringToBools();
        }
        [JsonConstructor]
        public Node(List<Source> s, string key, string size,string code, string type ) 
        {
            sources = s;
            keyName = key;
            fileSize = size;
            sha1Code = code;
            fileType = type;
            
        }

        public string keyName { set; get; }
        public string fileSize { set; get; }
        public string sha1Code { set; get; }
        public string fileType { set; get; }

        public List<Source> sources = null;
        [JsonIgnoreAttribute]
        public NodeStatus status { private set; get; }

        [JsonIgnoreAttribute]
        public Source[] blockSource = null;

        [JsonIgnoreAttribute]
        public bool[] blocks = null;

        

        [JsonIgnoreAttribute]
        public int blockNum => (int)Math.Ceiling((double)long.Parse(fileSize) / blockSize);

        [JsonIgnoreAttribute]
        public string saveSize {
            get
            {
                if (blocks == null)
                    return "0";
                int done = 0;
                for(int i=0;i<blockNum;i++)
                {
                    if (blocks[i] == true)
                        done++;
                }
                if (done == blockNum)
                {
                    status = NodeStatus.downloaded;
                    return fileSize;
                }
                else
                    return ((long)done * blockSize).ToString();
                 
            }
        }

        [JsonIgnoreAttribute]
        public double progressValue
        {
            get
            {
                return long.Parse(saveSize) * 100 / long.Parse(fileSize);
            }
        }
       
        public void DownloadInit()
        {
            blockSource = new Source[blockNum];
            blocks = new bool[blockNum];
            for (int i = 0; i < blockNum; i++)
            {
                blocks[i] = false;
             //   blockSource[i] = sources[0];
            }
            status = NodeStatus.downloading;
        }


        public void PrepareSources()
        {
            if(blockSource==null)
            {
                blockSource = new Source[blockNum];
            }
            List<Source> soc = getUsefulSources();
            int count = soc.Count;
            int j = 0;
            if(count==0)
            {
                status = NodeStatus.noSource;
            }
            else
            {
                for(int i=0;i<blockNum;i++)
                {
                    blockSource[i] = soc[j++];
                    if (j == count)
                        j = 0;
                }
            }

        }

        public void DownloadStatusContinue()
        {
            status = NodeStatus.downloading;
        }
        public void DownloadStatusPause()
        {
            status = NodeStatus.pause;
        }


        public List<Source> getUsefulSources()
        {
            List<Source> soc = new List<Source>();
            foreach(var source in sources)
            {
                
                TcpClient client = new TcpClient();
                try
                {
                    client.Connect(source.sourceIP, port);
                }
                catch (Exception)
                {
                //    sources.Remove(source);
                }
                if(client.Connected)
                {
                    NetworkStream stream = client.GetStream();
                    Datagram datagram = new Datagram
                    {
                        Type = DatagramType.isFileAvailable,
                        Message = this.ToString()
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
                                soc.Add(source);
                            }
                        }
                    }
                    client.Close();
                    stream.Close();
                }
            }
            return soc;

        }





        public void AddSource(string ip,string path)
        {
            if (sources == null)
                sources = new List<Source>();
            Source source = new Source
            {
                sourceIP = ip,
                sourcePath = path
            };
            sources.Add(source);
        }

        public void UpdateNodeSources(List<Source> s)
        {
            sources = new List<Source>(s);
        }

        public void RemoveSource(Source s)
        {
            sources.Remove(s);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);

        }

        public static Node Convert(string str)
        {
            if (str == "")
                return null;
            else
                return JsonConvert.DeserializeObject<Node>(str );
            
        }
    }
}
