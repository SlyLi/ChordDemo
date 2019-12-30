using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MetroDemo.Lib.Download;

namespace MetroDemo.Pages
{
    /// <summary>
    /// Downloads.xaml 的交互逻辑
    /// </summary>
    public partial class Downloads : UserControl, BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Downloads()
        {
            InitializeComponent();
            Thread thread = new Thread(FreshInterface)
            {
                IsBackground = true
            };
            thread.Start();

        }

        

        public void InitThis()
        {
            if(chord!=null)
            {
                FreshDownload();

            }
        }
        void FreshInterface()
        {
            while (chord == null); 
            while(false)          //更改刷新方法，改进
            {
                Thread.Sleep(100);
                DownloadList.Dispatcher.Invoke(() =>
                {
                    var o= DownloadList.SelectedItem;
                    DownloadList.Items.Clear();
                    foreach(var i in userInfo.DownloadNodes)
                    {
                        DownloadList.Items.Add(i);
                    }
                    DownloadList.SelectedItem = o;
                });


            }
           
        }

        private void FreshDownload_Click(object sender, RoutedEventArgs e)
        {
            FreshDownload();
        }

        public void FreshDownload()
        {
            Common.ShowNodes(DownloadList, userInfo.DownloadNodes);
        }

        private void PauseDownload_Click(object sender, RoutedEventArgs e)
        {
            object o = DownloadList.SelectedItem;
            if (o == null)
                return;
            Node node = o as Node;
            DownloadPause(node);
            


        }

        private void ContinueDownload_Click(object sender, RoutedEventArgs e)
        {
            object o = DownloadList.SelectedItem;
            if (o == null)
                return;
            Node node = o as Node;
            DownloadContinue(chord, userInfo, node);


        }


    }
}
