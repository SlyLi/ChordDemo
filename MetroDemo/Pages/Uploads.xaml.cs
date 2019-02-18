using System;
using System.Collections.Generic;
using System.IO;
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
using MahApps.Metro.Controls.Dialogs;
using MetroDemo.lib;
using Microsoft.Win32;
namespace MetroDemo.Pages
{
    /// <summary>
    /// Uploads.xaml 的交互逻辑
    /// </summary>
    public partial class Uploads : UserControl,BaseInterface
    {
        public Chord chord { set; get; }
        public UserInfo userInfo { set; get; }

        public Uploads()
        {
            InitializeComponent();
        }
        public void InitThis()
        {
           if(userInfo!=null)
            {
                Common.ShowNodes(uploadList, userInfo.UploadNodes);
            }
        }

        private async void UpLoad_ClickAsync(object sender, RoutedEventArgs e)
        {
            string filePath = SelectFile();

            if (File.Exists(filePath))
            {
                string[] keyArr = filePath.Split('\\');
                string[] typeBefore = keyArr[keyArr.Length - 1].Split('.');
                string type = typeBefore[typeBefore.Length - 1];

                string key = keyArr[keyArr.Length - 1].Substring(0, keyArr[keyArr.Length - 1].Length - type.Length-1);

                MetroWindow metroWindow = Window.GetWindow(this) as MetroWindow;

                var mySettings = new MetroDialogSettings()
                {
                    DefaultText=key,
                    AffirmativeButtonText = "确定",
                    NegativeButtonText="取消",
                    ColorScheme = metroWindow.MetroDialogOptions.ColorScheme
                };
                key = await metroWindow.ShowInputAsync("", "请输入文件标志：", mySettings);

                if(key!=null)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    Node node = new Node(key, fileInfo.Length.ToString(), filePath, type);
                    chord.InsertNode(node);
                    userInfo.AddUploadNode(node);
                    Common.ShowNodes(uploadList, userInfo.UploadNodes);
                }
                else
                {

                }
                

            }    
            
        }

        public string SelectFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "请选择文件夹",
                Filter = "所有文件(*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return "";
        }

    }

}
