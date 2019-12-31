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
using MahApps.Metro;

namespace MetroDemo
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }
        private readonly MainWindowViewModel _viewModel;
        public MainWindow()
        {
            chord = new Chord();
            userInfo = new UserInfo();
            userInfo.AddNodesToChord(chord);
  
            InitializeComponent();
            InitPages();
            _viewModel = new MainWindowViewModel(DialogCoordinator.Instance);
            DataContext = _viewModel;

        }

        public MainWindow(int i)
        {
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
            HamburgerMenuControl.Content = pages[0];

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
        
        private void SlyLi_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://blog.slyli.cn");
        }

        private MetroWindow about_window;
        private void About_Click(object sender, RoutedEventArgs e)
        {
            if (about_window != null)
            {
                about_window.Close();
            }
            about_window = new MetroWindow() { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, Title = "About this!", Width = 500, Height = 300 };
            about_window.Closed += (o, args) => about_window = null;
            about_window.Content = new TextBlock() { Text = "MetroWindow with Border", FontSize = 28, FontWeight = FontWeights.Light, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            
            //边框阴影效果
            about_window.SetCurrentValue(BorderThicknessProperty, new Thickness(0));
            about_window.SetCurrentValue(BorderBrushProperty, null);
            about_window.SetCurrentValue(GlowBrushProperty, Brushes.Black);

           // about_window.BorderThickness = new Thickness(0, 0, 0, 0);// 无边框
            //  about_window = new MainWindow(1) { Title = "About this!", Width = 500, Height = 300 };  //俄罗斯套娃

            about_window.Show();

         
        }
    }
}
