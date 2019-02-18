using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static MetroDemo.lib.Common;

namespace MetroDemo.lib
{
    public static class MyExtensions
    {
        public static string _GetSha1Code(string str)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(str);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        public static byte[] GetBytes(this int i)
        {
            return BitConverter.GetBytes(i);
        }


        public static string GetSha1Code(this String target)
        {
            return _GetSha1Code(target);
        }

        public static string GetSha1Code(this IPAddress ipaddress)
        {
            return _GetSha1Code(ipaddress.ToString());
        }

        public static string TabToString(this List<SourceIP> tab)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var t in tab)
            {
                sb.Append(t.ip+" ");
            }
            return sb.ToString();
        }

        public static List<SourceIP> StrConvertTab(this string str)
        {
            List<SourceIP> finTab = new List<SourceIP>(hashBit);
            string[] strs = str.Split(' ');
            foreach(var temp in strs)
            {
                finTab.Add(new SourceIP(temp));

            }
            return finTab;
        }

        public static string IPsToString(this List<string> ips)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var t in ips)
            {
                sb.Append(t + " ");
            }
            return sb.ToString();
        }

        public static List<string> StrConvertIPs(this string str)
        {
            List<string> ips = new List<string>();
            string[] strs = str.Split(' ');
            foreach(var temp in strs)
            {
                ips.Add(temp);
            }
            return ips;
           
        }
    }
}
