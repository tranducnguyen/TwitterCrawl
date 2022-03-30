using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Pwpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Type = "";
        public static string Month = "";
        public static string Year = "";
        public static int numThreads;
        public static List<Info> listData;
        public static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();
            main = this;

        }

        public void btn_nhap_Click(object sender, RoutedEventArgs e)
        {
            grid1.ItemsSource = LoadCollectionData();

        }

        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {

            Thread t = new Thread(() =>
            {

                Dispatcher.Invoke(() =>
                {
                    btnDownLoad.Content = "Đang tải";
                });
                //string[] listCookie = File.ReadAllLines("link.txt");
                //List<string> listCook = new List<string>();
                //foreach (string item in listCookie)
                //{
                //    listCook.Add(item);
                //}


                //Info info = new Info
                //{
                //    _CURSOR = "",
                //    _LINK = "",
                //    _LIST_COOKIES=listCook
                //};
                //TwitterController discord = new TwitterController();
                //discord.GetListUserName(info);
                DownloadMulti();

                Dispatcher.Invoke(() =>
                {
                    btnDownLoad.Content = "Tải..";
                });
            })
            {
                IsBackground = true
            };
            t.Start();


        }

        void DownloadMulti()
        {
            try
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\output"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\output");
                }

                if (listData==null || listData.Count()==0)
                {
                    MessageBox.Show("Vui lòng nạp list");
                    return;
                }
                foreach (Info item in listData)
                {
                    item._STATUS = "Đang lấy link...";
                    Refresh();
                    TwitterController discord = new TwitterController();
                    discord.GetListUserName(item);
                    item._STATUS = "Đã lấy xong " + item._COUNT_LINK + " links";
                    Refresh();
                    Thread.Sleep(1000);
                }

                Dispatcher.Invoke(() =>
                {
                    btnDownLoad.Content = "Tải...";
                });
            }
            catch { }
            
        }
        public void udpateList()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                //listData[vitri]._STATUS = status;
                grid1.Items.Refresh();
            }));

        }
        public void Refresh()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                grid1.Items.Refresh();
            }));

        }

        private void LoadInfo()
        {

        }

        private List<Info> LoadCollectionData()
        {
            List<Info> listAccount = new List<Info>();
            try
            {
                string path = Directory.GetCurrentDirectory() + "\\link.txt";
                string[] listImport = File.ReadAllLines(path);
                int test = 0;

                foreach (var value in listImport)
                {
                    string[] acsplit = value.Split("||");
                    if (acsplit.Count() > 2)
                    {
                        MessageBox.Show("Sai định dạng, xin vui lòng nhập lại file");
                        return null;
                    }
                    test++;
                    Info info = new Info()
                    {
                        _ID = test.ToString(),
                        _LINK = acsplit[1],
                        _COOKIE = acsplit[0]
                    };
                   
                    if (!string.IsNullOrEmpty(info._LINK) && !string.IsNullOrEmpty(info._COOKIE))
                    {
                        listAccount.Add(info);
                    }
                    else
                    {
                        MessageBox.Show(value + " không hợp lệ");
                        return null;
                    }
                }

                listData = listAccount;
                return listAccount;
            }
            catch
            {
                return null;
            }

        }

        private void txbDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            Thread t = new Thread(()=> {
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        CONSTVAL.TimeDelay = int.Parse(this.txbDelay.Text);
                    }));
                }
                catch {
                    CONSTVAL.TimeDelay = 10;
                }
                
            });
            t.IsBackground = true;
            t.Start();
        }
    }
    public static class CONSTVAL
    {
        public static int TimeDelay = 10;
    }

    public class Info
    {
        public string _ID { get; set; }
        public string _LINK { get; set; }
        public string _STATUS { get; set; }
        public int _COUNT_LINK { get; set; }
        public string _COOKIE { get; set; }
        public int _COUNT_CUR { get; set; }
        public string _CURSOR { get; set; }
    }
}
