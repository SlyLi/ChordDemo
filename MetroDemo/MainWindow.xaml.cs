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
            InitPages();
        }

        private void InitPages()
        {
            HamburgerMenuItemCollection pages = HamburgerMenuControl.ItemsSource as HamburgerMenuItemCollection;
           
            foreach(var page in pages)
            {
                BaseInterface page_tag = page.Tag as BaseInterface;
                if (page_tag != null)
                {
                    if (page_tag.chord == null)
                        page_tag.chord = chord;
                    if (page_tag.userInfo == null)
                        page_tag.userInfo = userInfo;
                    page_tag.InitThis();
                }
            }
                
        }


        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            HamburgerMenuControl.Content = e.InvokedItem;
            BaseInterface page = ((HamburgerMenuIconItem)e.InvokedItem).Tag as BaseInterface;
            if(page != null )
            {      
                page.InitThis();
            }
       
        }

    }
}
