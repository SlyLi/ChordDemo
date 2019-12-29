using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MetroDemo.lib.Common;

namespace MetroDemo.lib
{
    public class Datagram
    {
        public Datagram()
        {
            Type = DatagramType.chat;
            FromAddress = GetLocalIP().ToString();
            ToAddress = "";
            Message = "";

        }

        public int AllSize
        {
            get
            {
                return ToBytes().Length;
            }
        }
        public DatagramType Type { set; get; }

        public string FromAddress { set; get; }

        public string ToAddress { set; get; }

        public string Message { set; get; }

        public int Length { get { return Message.Length; } }

        public byte[] ToBytes()
        {
            byte[] type = ((int)Type).GetBytes();
            byte[] from = Encoding.UTF8.GetBytes(FromAddress);
            byte[] to = Encoding.UTF8.GetBytes(ToAddress);
            byte[] msg = Encoding.UTF8.GetBytes(Message);

            byte[] all = new byte[msg.Length + 40 * 2 + 4];

            Array.Copy(type, 0, all, 0, type.Length);
            Array.Copy(from, 0, all, 4, from.Length);
            Array.Copy(to, 0, all, 44, to.Length);
            Array.Copy(msg, 0, all, 84, msg.Length);

            return all;
        }
        public static Datagram Convert(byte[] bytes)  //内容格式化
        {

            Datagram data = new Datagram
            {
                Type = (DatagramType)BitConverter.ToInt32(bytes, 0),
                FromAddress = (Encoding.UTF8.GetString(bytes, 4,40)).Replace("\0", ""),
                ToAddress = (Encoding.UTF8.GetString(bytes, 44, 40)).Replace("\0", ""),
                Message = Encoding.UTF8.GetString(bytes, 84, bytes.Length - 84),
            };
            return data;
        }




        /*
        public override string ToString()   //内容序列化
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Type▶{0}◀", this.Type.ToString());

            sb.AppendFormat("FromAddress▶{0}◀", this.FromAddress.ToString());

            sb.AppendFormat("ToAddress▶{0}◀", this.ToAddress.ToString());

            sb.AppendFormat("Message▶{0}", this.Message.ToString());

            return sb.ToString();
        }
        public byte[] ToByte()
        {
            return Encoding.BigEndianUnicode.GetBytes(ToString());
        }
        public static Datagram Convert(string str)  //内容格式化
        {
            Datagram data = new Datagram();
            IDictionary<string, string> idict = new Dictionary<string, string>();

            string[] strlist = str.Split('◀');
            for (int i = 0; i < strlist.Length; i++)
            {
                string[] info = strlist[i].Split('▶');
                if(info.Length==2)
                    idict.Add(info[0], info[1]);

            }

            if (idict.ContainsKey("Type"))
                data.Type = (DatagramType)Enum.Parse(typeof(DatagramType), idict["Type"]);
            if (idict.ContainsKey("FromAddress"))
                data.FromAddress = idict["FromAddress"];
            if (idict.ContainsKey("ToAddress"))
                data.ToAddress = idict["ToAddress"];
            if (idict.ContainsKey("Message"))
                data.Message = idict["Message"];

            return data;
        }
        public static Datagram Convert(byte[] bytes,int nums)  //内容格式化
        {
            string str = Encoding.BigEndianUnicode.GetString(bytes, 0, nums);
            return Convert(str);
        }
        */
    }

    public enum DatagramType
    {
        onChordSuc = 1,                        //在本节点后插入
        onChordPre,                            //在本节点前插入
        offChordSuc,                           //离开环,给后一个节点发送
        offChordPre,                           //离开环，给前一个节点发送
        addNodeFromPre,                        //接受从前一个节点继承来的资源换
        getPre,                                //请求前赴
        returnPre,                             //返回前赴
        getSuc,                                //请求后继
        returnSuc,                             //返回后继
        connect,                               //建立连接
        disconnect,                            //断开连接
        chat,                                  //聊天信息
        chatAll,                               //群聊
        downloadFile,                          //下载文件
        isFileAvailable,                       //判断资源是否有效
        fileAvailable,                         //资源有效
        fileUnAvailable,                       //资源已经失效
        updateSources,                         //更新资源位置信息
        getInfo,                               //获取信息
        giveInfo,                              //给予个人的信息
        insertNode,                            //插入资源
        getNodesList,                          //获取本节点所有资源
        retNodesList,
        getNodePosition,                       //获取节点位置
        freshFinTab,                           //刷新路由信息表
        getFinTab,                             //获取路由信息表
        whoIsActive,                            //广播活动主机
        acticeHost                              //本主机活动
    }
}
