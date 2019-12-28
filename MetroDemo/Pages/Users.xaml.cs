using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace MetroDemo.Pages
{
    /// <summary>
    /// Users.xaml 的交互逻辑
    /// </summary>
    public partial class Users : UserControl,BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Users()
        {
            InitializeComponent();
        }
        public void InitThis()
        {
            if (chord != null)
            {
                chord.ShowHostList(usersList);
            }
        }

        private void FreshHost_Click(object sender, RoutedEventArgs e)
        {
            if(chord!=null)
            {
                chord.ShowHostList(usersList);
            }
        }

        private void usersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object o = usersList.SelectedItem;
            if (o == null)
                return;
            SourceIP item = (SourceIP)o;

            chord.ShowTargetInfo(IPAddress.Parse(item.ip));
        }
    }
}
