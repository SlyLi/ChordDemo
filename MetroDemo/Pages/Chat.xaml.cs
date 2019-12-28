using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MetroDemo.lib;


namespace MetroDemo.Pages
{
    /// <summary>
    /// Chat.xaml 的交互逻辑
    /// </summary>
    public partial class Chat : UserControl,BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Chat()
        {
            InitializeComponent();
            
        }

        public void InitThis()
        {
            if (chord != null )
            {
                chord.BindDsipBord(messageRecord);
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            chord.ChatToALL(inputMessage.Text);
           // messageRecord.Text += inputMessage.Text + "\r\n";
            inputMessage.Text = "";
            
        }

        
    }
  
}
