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
using MahApps.Metro.Controls;
using MetroDemo.Pages;
using MetroDemo.lib;
using MahApps.Metro.Controls.Dialogs;

namespace MetroDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }
        public MainWindow()
        {
            chord = new Chord();
            userInfo = new UserInfo();
            userInfo.AddNodesToChord(chord);
           

            InitializeComponent();
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            HamburgerMenuControl.Content = e.InvokedItem;
            BaseInterface page = ((HamburgerMenuIconItem)e.InvokedItem).Tag as BaseInterface;
            if(page != null )
            {
                if (page.chord == null)
                    page.chord = chord;
                if (page.userInfo == null)
                    page.userInfo = userInfo;
                page.InitThis();
            }

            Downloads downloads = page as Downloads;
            if (downloads != null)
            {
                downloads.FreshDownload();
            }
        }


      
    }
}
