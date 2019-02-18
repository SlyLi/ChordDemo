using MetroDemo.lib;
using System;
using System.Collections.Generic;
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
            while(true)
            {
                Thread.Sleep(100);
                DownloadList.Dispatcher.Invoke(() =>
                {
                    DownloadList.Items.Clear();
                    foreach(var i in userInfo.DownloadNodes)
                    {
                        DownloadList.Items.Add(i);
                    }
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


    }
}
