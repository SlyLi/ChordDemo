using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetroDemo.lib.Common;

namespace MetroDemo.lib
{
    public class UserInfo : IDisposable
    {

       

        public string downloadPath;
        public List<string> targetIPs;
        public Nodes DownloadNodes;
        public Nodes UploadNodes;

        public UserInfo()
        {
            string currentPath = Environment.CurrentDirectory;
            string settingPath = currentPath + "\\user.ini";
            if (File.Exists(settingPath))
            {
                StringBuilder readStr = new StringBuilder();
                FileInfo fileInfo = new FileInfo(settingPath);
                FileStream fileStream = new FileStream(settingPath, FileMode.Open, FileAccess.ReadWrite);
                Byte[] bytes = new byte[1024];
                int length = 0;
                while((length = fileStream.Read(bytes, 0, bytes.Length))!=0)
                {
                    readStr.Append(Encoding.BigEndianUnicode.GetString(bytes, 0, length));
                }

                Convert(readStr.ToString());
                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                }
                
            }
            else
            {
                downloadPath = Environment.CurrentDirectory + "\\Download\\";
                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                }
                targetIPs = new List<string>();
                DownloadNodes = new Nodes();
                UploadNodes = new Nodes();

            }
        }


        public void AddDownloadNode(Node node)
        {
            Node localNode = null;
            foreach (var temp in DownloadNodes)
            {
                if (temp.sha1Code == node.sha1Code)
                {
                    localNode = temp;
                    var exp = node.sources.Where(a => !temp.sources.Exists(t => a.sourceIP == t.sourceIP && a.sourcePath == t.sourcePath)).ToList();
                    foreach (var e in exp)
                    {
                        localNode.AddSource(e.sourceIP, e.sourcePath);
                    }
                }
            }

            if (localNode == null)
                DownloadNodes.Add(node);

           /* foreach (var n in node.sources)
            {
                if (n.sourceIP == GetLocalIP().ToString())
                {
                    if(localNode==null)
                    {
                        localNode = node;
                    }
                    else
                    {
                        localNode.AddSource(n.sourceIP, n.sourcePath);

                    }
                }
            }*/
            
           // return localNode;
        }

        public void AddUploadNode(Node node)
        {
            Node localNode = null;
            foreach (var temp in UploadNodes)
            {
                if (temp.sha1Code == node.sha1Code)
                {
                    localNode = temp;

                    var exp = node.sources.Where(a => !temp.sources.Exists(t => a.sourceIP == t.sourceIP && a.sourcePath == t.sourcePath)).ToList();
                    foreach (var e in exp)
                    {
                        localNode.AddSource(e.sourceIP, e.sourcePath);
                    }
                    return;
                }
            }
            foreach (var n in node.sources)
            {
                if (n.sourceIP == GetLocalIP().ToString())
                {
                    if (localNode == null)
                    {
                        localNode = new Node(node.keyName, node.fileSize, n.sourcePath, node.fileType);
                    }
                    else
                    {
                        localNode.AddSource(n.sourceIP, n.sourcePath);

                    }
                }
            }
            if (localNode != null)
            {
                UploadNodes.Add(localNode);
            }
        }

        void Save()
        {
            string currentPath = Environment.CurrentDirectory;
            string path = currentPath + "\\user.ini";
            FileInfo fileInfo = new FileInfo(path);
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] vs = Encoding.BigEndianUnicode.GetBytes(ToSaveString());
            fileStream.Write(vs, 0, vs.Length);
            fileStream.Close();
            File.SetAttributes(path, FileAttributes.Hidden);
        }

        private string ToSaveString()
        {
            StringBuilder saveStr = new StringBuilder();
            saveStr.Append(downloadPath + "\n");
            saveStr.Append(targetIPs.IPsToString() + "\n");
            saveStr.Append(DownloadNodes.ToString()+"\n");
            saveStr.Append(UploadNodes.ToString());
           // saveStr.Append(localNodes.ToString());
            return saveStr.ToString();
        }

        void Convert(string readStr)
        {
            string[] strs = readStr.ToString().Split('\n');
            downloadPath = strs[0];
            targetIPs = strs[1].StrConvertIPs();
            DownloadNodes = Nodes.Convent(strs[2]);
            UploadNodes = Nodes.Convent(strs[3]);
            for(int i=DownloadNodes.Count-1;i>=0;i--)
            {
                for(int j=DownloadNodes[i].sources.Count-1;j>=0;j--)
                {
                    if(DownloadNodes[i].sources[j].sourceIP!=GetLocalIP().ToString())
                    {
                        DownloadNodes[i].sources.RemoveAt(j);
                    }
                    else if(!File.Exists(DownloadNodes[i].sources[j].sourcePath))
                    {
                        DownloadNodes[i].sources.RemoveAt(j);
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(DownloadNodes[i].sources[j].sourcePath);
                        if(fileInfo.Length!=(long.Parse(DownloadNodes[i].fileSize)))
                        {
                            DownloadNodes[i].sources.RemoveAt(j);
                        }
                    }
                }
                if(DownloadNodes[i].sources.Count==0)
                {
                    DownloadNodes.RemoveAt(i);
                }
            }

            for (int i = UploadNodes.Count - 1; i >= 0; i--)
            {
                for (int j = UploadNodes[i].sources.Count - 1; j >= 0; j--)
                {
                    if (UploadNodes[i].sources[j].sourceIP != GetLocalIP().ToString())
                    {
                        UploadNodes[i].sources.RemoveAt(j);
                    }
                    else if (!File.Exists(UploadNodes[i].sources[j].sourcePath))
                    {
                        UploadNodes[i].sources.RemoveAt(j);
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(UploadNodes[i].sources[j].sourcePath);
                        if (fileInfo.Length != (long.Parse(UploadNodes[i].fileSize)))
                        {
                            UploadNodes[i].sources.RemoveAt(j);
                        }
                    }
                }
                if (UploadNodes[i].sources.Count == 0)
                {
                    UploadNodes.RemoveAt(i);
                }
            }
        }
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
                Save();
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~UserInfo()
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

        public void AddNodesToChord(Chord chord)
        {
            foreach (var node in DownloadNodes)
            {
                chord.InsertNode(node);
            }
            foreach (var node in UploadNodes)
            {
                chord.InsertNode(node);
            }
        }
    }


}
