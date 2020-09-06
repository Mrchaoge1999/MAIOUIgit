﻿using Fleck;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
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
using System.Windows.Threading;
using static MAIO.LoginWindow;

namespace MAIO
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            Cid.Text = Config.cid;
            Cjevent.Text = Config.cjevent;
            discordwebhook.Text = Config.webhook;
            clear.IsChecked = Config.autoclearcookie;
            delay2.Text = Config.delay;
            if (Config.Usemonitor.Contains("True"))
            {
                usemonitor.IsChecked = true;
            }
            else
            {
                usemonitor.IsChecked = false;
            }
            if (Config.UseAdvancemode.Contains("True"))
            {
                advance.IsChecked = true;
            }
            else
            {
                advance.IsChecked = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Writecoookie.write();
            Application.Current.Shutdown();
        }

        private void Deactive_Click(object sender, RoutedEventArgs e)
        {
            string md5checkdoublekey = MD5Helper.EncryptString(Config.Key);
            bool resetstatus = KeyRest(md5checkdoublekey);
            if (resetstatus == true)
            {
                Writecoookie.write();
                Application.Current.Shutdown();
            }
            else
            {
                Writecoookie.write();
                Application.Current.Shutdown();
            }

        }
        public bool KeyRest(string md5key)
        {
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(@"http://49.51.68.105/WebService1.asmx");
            var factory = new ChannelFactory<ServiceReference2.WebService1Soap>(binding, endpoint);
            var callClient = factory.CreateChannel();
            var resetkey = callClient.KeyresetAsync(md5key);
            return resetkey.Result;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.webhook = discordwebhook.Text;
                Config.cid = Cid.Text;
                Config.cjevent = Cjevent.Text;
                //  Config.Usemonitor = Usemonitor.IsChecked.ToString();
                Config.Usemonitor = usemonitor.IsChecked.ToString();
                Config.UseAdvancemode = advance.IsChecked.ToString();
                Config.autoclearcookie =(bool) clear.IsChecked;
                Random ra = new Random();
                if (delay2.Text == "")
                {
                    MessageBox.Show("Please fill all information");
                }
                else
                {
                    Config.delay = delay2.Text;
                }
                string config = "{\"webhook\":\"" + Config.webhook + "\",\"key\":\"" + Config.Key + "\",\"cid\":\"" + Config.cid + "\",\"cjevent\":\"" + Config.cjevent + "\",\"delay\":\"" + Config.delay + "\",\"Usemonitor\":\""+usemonitor.IsChecked.ToString()+"\",\"Advancemode\":\"" + advance.IsChecked.ToString() + "\"," +
                    "\"AutoClearCookie\":\"" + Config.autoclearcookie.ToString()+ "\"}";
                File.WriteAllText(Environment.CurrentDirectory + "\\" + "config.json", config);
                MessageBox.Show("Save success");
            }
            catch
            {
                MessageBox.Show("fail save");
            }
        }
        private void Check_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void gencookie(object sender, RoutedEventArgs e)
        {
            CookieGen cookieGen = new CookieGen();
            cookieGen.Show();
        }
        public void ws(string site)
        {
            string ChromePath = Environment.CurrentDirectory + "\\" + "cookiegen";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + "cookiedata" + "\\" + Guid.NewGuid().ToString();
            Process.Start("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", "\"--load-extension=\"" + ChromePath + "\"\" \"--user-data-dir=\"" + path + "\"");
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://127.0.0.1:64526");
            server.Start(socket =>
            {
                socket.OnOpen = () => //当建立Socket链接时执行此方法
                {
                    var data = socket.ConnectionInfo; //通过data可以获得这个链接传递过来的Cookie信息，用来区分各个链接和用户之间的关系（如果需要后台主动推送信息到某个客户的时候，可以使用Cookie）
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>// 当关闭Socket链接十执行此方法
                {
                    //  Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>// 接收客户端发送过来的信息
                {
                    if (message.Contains("abck"))
                    {
                        string bmsz = "";
                        string geoc = "";
                        string bm_sv = "";
                        JObject jo = JObject.Parse(message);
                        var chao = jo.ToString();
                        if (jo["value"].ToString().Contains("==") == false)
                        {
                            JArray ja = JArray.Parse(jo["value1"].ToString());
                            foreach (var i in ja)
                            {
                                JObject jo2 = JObject.Parse(i.ToString());
                                if (jo2["name"].ToString() == "bm_sz")
                                {
                                    bmsz = jo2["value"].ToString();
                                }
                                if (jo2["name"].ToString() == "ak_bmsc")
                                {
                                    geoc = jo2["value"].ToString();
                                }
                                if (jo2["name"].ToString() == "bm_sv")
                                {
                                    bm_sv = jo2["value"].ToString();
                                }
                                if (bmsz != "" && geoc != "")
                                {
                                    break;
                                }
                            }
                            string cookie = "bm_sz=" + bmsz + "; _abck=" + jo["value"].ToString();
                            long time2 = (long)(DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalMilliseconds;
                            string cookiewtime = "[{\"cookie\":\"" + cookie + "\",\"time\":\"" + time2.ToString() + "\",\"site\":\"" + site + "\"}]";
                            if (site == "NIKE")
                            {
                                Mainwindow.lines.Add(cookie);
                                Mainwindow.cookiewtime.Add(time2, cookie);
                                Mainwindow.iscookielistnull = false;
                            }
                          //  Main.updatelable(cookie, true);
                            FileInfo fi = new FileInfo(Environment.CurrentDirectory + "\\" + "cookie.json");
                            if (fi.Length == 0)
                            {
                                FileStream fs1 = new FileStream(Environment.CurrentDirectory + "\\" + "cookie.json", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                                JArray ja2 = JArray.Parse(cookiewtime);
                                StreamWriter sw = new StreamWriter(fs1);
                                sw.Write(ja2.ToString().Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", ""));
                                sw.Close();
                                fs1.Close();
                            }
                            else
                            {
                                using (FileStream fs2 = new FileStream(Environment.CurrentDirectory + "\\" + "cookie.json", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                {
                                    StreamReader sr = new StreamReader(fs2);
                                    string read = sr.ReadToEnd();
                                    sr.Close();
                                    JArray ja3 = JArray.Parse(read);//bug
                                    ja3.Add(JObject.Parse(cookiewtime.Replace("[", "").Replace("]", "")));
                                    FileStream fs3 = new FileStream(Environment.CurrentDirectory + "\\" + "cookie.json", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                                    fs3.SetLength(0);
                                    StreamWriter sw = new StreamWriter(fs3);
                                    sw.Write(ja3.ToString().Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", ""));
                                    sw.Close();
                                    fs3.Close();
                                }
                            }
                        }
                    }
                    socket.Send(message);
                    if (message.Contains("{\"type\":\"proxy\"}"))
                    {
                        Random ra = new Random();
                        int index = ra.Next(0, Mainwindow.proxypool.Count);
                        string proxy = "";
                        try
                        {
                            proxy = "Proxy: " + Mainwindow.proxypool[index] + "";
                        }
                        catch
                        {
                            proxy = "Proxy: ";
                        }
                        allSockets.ToList().ForEach(s => s.Send(proxy));
                    }
                    else
                    {
                    }

                };
            });
        }
        private static DateTime timeStampStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private void DelAllCookie_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory + "\\" + "cookie.json";
            FileStream fs0 = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs0.SetLength(0);
            fs0.Close();
            Mainwindow.lines.Clear();
            Mainwindow.cookiewtime.Clear();
            Mainwindow.iscookielistnull = true;
          /*  Config.mn.cookienum.Dispatcher.Invoke(new Action(
                  delegate
                  {
                      Config.mn.cookienum.Content = Mainwindow.lines.Count;
                  }));*/
        }

        private void ToggleButton_Initialized(object sender, EventArgs e)
        {
            string resourceStr = "pack://application:,,,/Resource/Style/Primary/Magenta.xaml";
            UpdataResourceDictionary(resourceStr, 1);
        }
        private void UpdataResourceDictionary(string resourceStr, int pos)
        {
            if (pos < 0 || pos > 2)
            {
                return;
            }
            ResourceDictionary resource = new ResourceDictionary
            {
                Source = new Uri(resourceStr)
            };
           // Resources.MergedDictionaries.RemoveAt(pos);
            Resources.MergedDictionaries.Insert(0, resource);
        }

    }
}
