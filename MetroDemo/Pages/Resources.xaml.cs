using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Resources.xaml 的交互逻辑
    /// </summary>
    public partial class Resources : UserControl, BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Resources()
        {
            InitializeComponent();
        }
        public void InitThis()
        {
            if (chord != null)
            {
                chord.ShowSourcesList(sourcesList);
            }
        }

        private void FreshResource_Click(object sender, RoutedEventArgs e)
        {
            if (chord != null)
            {
                chord.ShowSourcesList(sourcesList);
            }
        }
        private void DownloadSource_Click(object sender, RoutedEventArgs e)
        {
            object o = sourcesList.SelectedItem;
            if (o == null)
                return;
            Node node = o as Node;
            DownloadBegin(chord,userInfo,node);
            //  chord.DownloadFile(node);
        }

        private void sourcesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object o = sourcesList.SelectedItem;
            if (o == null)
                return;
            Node node = o as Node;

            chord.Display("");
            chord.Display(node.keyName + "-----------node begin");
            chord.Display("size : " + node.fileSize + " 类型:" + node.fileType + " SHA1:" + node.sha1Code);
            foreach (var source in node.sources)
            {
                chord.Display("Sources IP:" + source.sourceIP + " Path:" + source.sourcePath);
            }
            chord.Display("------------------------------------------------------------------------end");
        }
    }
}