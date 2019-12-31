using MahApps.Metro;
using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : UserControl, BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Home()
        {
            InitializeComponent();
        }
        public void InitThis()
        {

        }

        private void ChordJoinLeave_Click(object sender, RoutedEventArgs e)
        {
            if (chord != null)
            {
                var button = sender as ToggleButton;
                if ((string)button.Content == "加入")
                {
                    chord.Join();
                    button.Content = "断开";
                    /*
                    /// 修改主题，样式。待完成
                    Accent expectedAccent = ThemeManager.Accents.First(x => x.Name == "Green"); //"Blue"
                    AppTheme expectedTheme = ThemeManager.GetAppTheme("BaseDark");  //"BaseDark"
                    ThemeManager.ChangeAppStyle(Application.Current, expectedAccent, expectedTheme);
                    */
                }
                else
                {
                    chord.Leave();
                    button.Content = "加入";
                }


            }
        }
    }
}
