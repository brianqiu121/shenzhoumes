using log4net;
using Shenzhou.Bll;
using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shenzhou_Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 变量定义
        //ILog log = LogManager.GetLogger("Shenzhou.Mes");

        /// <summary>
        /// 保存每个富文本框当前的行数，key为富文本框名称，value为行数
        /// </summary>
        private Hashtable htLines = new Hashtable();

        /// <summary>
        /// 富文本框最多显示行数，超过则清空重新计数
        /// </summary>
        int maxRichTextLine = 30;

        Socket socketPdaWatch;
        Thread threadPdaWatch;
        /// <summary>
        /// 保存了服务器端所有负责和PLC客户端通信发套接字  
        /// </summary>
        Dictionary<string, Socket> dictSocketPda = new Dictionary<string, Socket>();
        /// <summary>
        /// 保存了服务器端所有负责调用通信套接字.Receive方法的线程 
        /// </summary>
        Dictionary<string, Thread> dictThreadPda = new Dictionary<string, Thread>();

        Socket socketAgvWatch;
        Thread threadAgvWatch;
        Thread threadAgvSend;
        /// <summary>
        /// 保存了服务器端所有负责和PLC客户端通信发套接字  
        /// </summary>
        Dictionary<string, Socket> dictSocketAgv = new Dictionary<string, Socket>();
        /// <summary>
        /// 保存了服务器端所有负责调用通信套接字.Receive方法的线程 
        /// </summary>
        Dictionary<string, Thread> dictThreadAgv = new Dictionary<string, Thread>();
        #endregion

        #region 系统方法
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //CutBll bll = new CutBll();
            //List<BP_ORDER> model = bll.getBdBpOrder("410100420161200340");
            //开启PLC监听
            socketPdaWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(txtIP.Text.Trim());
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(txtPdaPort.Text.Trim()));
            socketPdaWatch.Bind(endpoint);

            socketPdaWatch.Listen(10);

            threadPdaWatch = new Thread(WatchingPdaConn);
            threadPdaWatch.IsBackground = true;
            threadPdaWatch.Start();

            Dispatcher.Invoke(new Action(() =>
            {
                setRichTxt(txtPdaMsg, "开始对PDA客户端进行监听！" + "\r\n");
                //txtPlcMsg.AppendText("开始对PLC客户端进行监听！" + "\r\n");
            }
                ));

            //开启Agv监听
            socketAgvWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endpointAgv = new IPEndPoint(ipaddress, int.Parse(txtAGVPort.Text.Trim()));
            socketAgvWatch.Bind(endpointAgv);

            socketAgvWatch.Listen(10);

            threadAgvWatch = new Thread(WatchingAgvConn);
            threadAgvWatch.IsBackground = true;
            threadAgvWatch.Start();

            threadAgvSend = new Thread(SendAgvMsg);
            threadAgvSend.IsBackground = true;
            threadAgvSend.Start();

            Dispatcher.Invoke(new Action(() =>
            {
                setRichTxt(txtAgvMsg, "开始对Agv客户端进行监听！" + "\r\n");
            }
                ));

            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;
            btnStop.Foreground = Brushes.White;
            btnStart.Foreground = Brushes.Gray;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            //关闭所有进程
            foreach (var item in dictThreadPda)
            {
                Thread tPda = item.Value;
                try
                {
                    tPda.Abort();
                }
                catch { }
            }
            foreach (var item in dictThreadAgv)
            {
                Thread tAgv = item.Value;
                try
                {
                    tAgv.Abort();
                }
                catch { }
            }

            try
            {
                threadPdaWatch.Abort();
                threadAgvWatch.Abort();
                threadAgvSend.Abort();
                socketPdaWatch.Close();
                socketAgvWatch.Close();
            }
            catch { }

            //清空所有客户端记录
            dictSocketPda.Clear();
            dictThreadPda.Clear();

            dictSocketAgv.Clear();
            dictThreadAgv.Clear();

            setRichTxt(txtAgvMsg, "停止对AGV客户端进行监听！" + "\r\n");

            setRichTxt(txtPdaMsg, "停止对移动客户端进行监听！" + "\r\n");

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnStop.Foreground = Brushes.Gray;
            btnStart.Foreground = Brushes.White;
        }
        #endregion

        #region Pda socket通信
        /// <summary>
        /// 监听Pda客户端连接，当有新连接进入时，将新连接的socket添加进在dictSocketPda中,
        /// 同时新开一个线程监听此新连接，将新开的线程添加进dictThreadPda中
        /// </summary>
        private void WatchingPdaConn()
        {

            while (true)
            {
                Socket socConn = socketPdaWatch.Accept();

                //将与客户端通信的套接字对象sokConn添加到键值对集合中，并以客户端IP端口作为键  
                dictSocketPda.Add(socConn.RemoteEndPoint.ToString(), socConn);

                //新开一个线程监听此连接
                ParameterizedThreadStart pts = new ParameterizedThreadStart(RecPdaMsg);
                Thread trd = new Thread(pts);
                trd.IsBackground = true;
                trd.Start(socConn);

                dictThreadPda.Add(socConn.RemoteEndPoint.ToString(), trd);//保存新开的线程
                //在PLC Log文本框中添加信息
                Dispatcher.Invoke(new Action(() =>
                {
                    setRichTxt(txtPdaMsg, "客户端" + socConn.RemoteEndPoint.ToString() + "连接成功！" + "\r\n");

                }
                    ));

            }

        }

        /// <summary>
        /// 处理收到的Pda信息
        /// </summary>
        /// <param name="socketClientPara">对应Pda客户端连接的socket对象</param>
        private void RecPdaMsg(object socketClientPara)
        {
            Socket socketRec = socketClientPara as Socket;

            while (true)
            {
                byte[] arrRecMsg = new byte[1024 * 1024];

                int length = -1;
                try
                {
                    length = socketRec.Receive(arrRecMsg);
                }
                catch (SocketException ex)
                {
                    GlobalData.log.Error("RecPlcMsg>>SocketException>>" + CommonHelper.GetException(ex));
                    //从通信套接字集合中删除被中断连接的通信套接字对象  
                    dictSocketPda.Remove(socketRec.RemoteEndPoint.ToString());
                    //从通信线程结合中删除被终端连接的通信线程对象  
                    dictThreadPda.Remove(socketRec.RemoteEndPoint.ToString());

                    break;
                }
                catch (Exception ex)
                {
                    GlobalData.log.Error("RecPdaMsg>>Exception>>" + CommonHelper.GetException(ex));
                    break;
                }

                string str = DatacomHelper.BytesToString(Encoding.UTF8, arrRecMsg, 0, length);// Encoding.UTF8.GetString(arrRecMsg, 0, length);

                PdaBll pdaBll = new PdaBll();
                string strResult = pdaBll.ProcessMsg(str);
                if (strResult == null)
                {
                    strResult = "ERROR,0";
                }
                byte[] strSendMsg = Encoding.UTF8.GetBytes(strResult);
                socketRec.Send(strSendMsg);

                //将收到的信息显示到界面
                Dispatcher.Invoke(new Action(() =>
                {
                    setRichTxt(txtPdaMsg, "" + GetTime() + ":\r\n" + socketRec.RemoteEndPoint + ":" + str + "\r\n");

                }));


                Thread.Sleep(10);
            }
        }


        #endregion

        #region Agv socket 通信
        /// <summary>
        /// 监听Agv客户端连接，当有新连接进入时，将新连接的socket添加进在dictSocketAgv中,
        /// </summary>
        private void WatchingAgvConn()
        {
            while (true)
            {
                Socket socConn = socketAgvWatch.Accept();

                //将与客户端通信的套接字对象sokConn添加到键值对集合中，并以客户端IP端口作为键  
                dictSocketAgv.Add(socConn.RemoteEndPoint.ToString(), socConn);

                //新开一个线程监听此连接
                ParameterizedThreadStart pts = new ParameterizedThreadStart(RecAgvMsg);
                Thread trd = new Thread(pts);
                trd.IsBackground = true;
                trd.Start(socConn);

                dictThreadAgv.Add(socConn.RemoteEndPoint.ToString(), trd);//保存新开的线程
                //在PLC Log文本框中添加信息
                Dispatcher.Invoke(new Action(() =>
                {
                    setRichTxt(txtAgvMsg, "客户端" + socConn.RemoteEndPoint.ToString() + "连接成功！" + "\r\n");

                }
                    ));

            }

        }

        /// <summary>
        /// 处理收到的Agv信息
        /// </summary>
        /// <param name="socketClientPara">对应Agv客户端连接的socket对象</param>
        private void RecAgvMsg(object socketClientPara)
        {
            Socket socketRec = socketClientPara as Socket;

            while (true)
            {
                byte[] arrRecMsg = new byte[1024 * 1024];

                int length = -1;
                try
                {
                    length = socketRec.Receive(arrRecMsg);
                }
                catch (SocketException ex)
                {
                    GlobalData.log.Error("RecPlcMsg>>SocketException>>" + CommonHelper.GetException(ex));
                    //从通信套接字集合中删除被中断连接的通信套接字对象  
                    dictSocketAgv.Remove(socketRec.RemoteEndPoint.ToString());
                    //从通信线程结合中删除被终端连接的通信线程对象  
                    dictThreadAgv.Remove(socketRec.RemoteEndPoint.ToString());

                    break;
                }
                catch (Exception ex)
                {
                    GlobalData.log.Error("RecAgvMsg>>Exception>>" + CommonHelper.GetException(ex));
                    break;
                }

                string str = DatacomHelper.BytesToString(Encoding.UTF8, arrRecMsg, 0, length);// Encoding.UTF8.GetString(arrRecMsg, 0, length);

                AgvBll agvBll = new AgvBll();
                //string strResult = agvBll.ProcessMsg(str);
                //if (strResult == null)
                //{
                //    strResult = "ERROR,0";
                //}
                byte[] akMsg = agvBll.ProcessMsg(str);
                if (akMsg != null && akMsg.Length > 0)
                {
                    socketRec.Send(akMsg);
                }

                //将收到的信息显示到界面
                Dispatcher.Invoke(new Action(() =>
                {
                    setRichTxt(txtAgvMsg, "" + GetTime() + ":\r\n" + socketRec.RemoteEndPoint + ":" + str + "\r\n");

                }));

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 发送指令给agv
        /// </summary>
        private void SendAgvMsg()
        {
            AgvBll agvBll = new AgvBll();
            while (true)
            {
                try
                {
                    if (dictSocketAgv.Count() > 0)
                    {
                        List<byte[]> lstSend = agvBll.SendMsg();
                        foreach (var msg in lstSend)
                        {
                            foreach (var soc in dictSocketAgv)
                            {
                                soc.Value.Send(msg);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string strMsg = CommonHelper.GetException(e);
                    GlobalData.log.Error(strMsg);
                    //将错误信息显示到界面
                    Dispatcher.Invoke(new Action(() =>
                    {
                        setRichTxt(txtAgvMsg, "" + GetTime() + ":\r\n" + strMsg + "\r\n");

                    }));
                }
                Thread.Sleep(5000);
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 将信息追加到富文本框中，分行显示，最多显示30行
        /// </summary>
        /// <param name="richTxt">富文本框对象</param>
        /// <param name="txt">要显示的内容</param>
        private void setRichTxt(System.Windows.Controls.RichTextBox richTxt, string txt)
        {
            //累计文本框行数
            int lineCnt = 0;
            if (htLines.Contains(richTxt.Name))
            {
                lineCnt = (int)htLines[richTxt.Name];
                if (lineCnt >= maxRichTextLine)//最多保留指定行数据
                {
                    lineCnt = 0;
                    richTxt.Document.Blocks.Clear();
                }

                htLines[richTxt.Name] = lineCnt + 1;

            }
            else
            {
                htLines.Add(richTxt.Name, 1);
            }

            //赋值
            richTxt.AppendText(txt);

            GlobalData.log.Info(txt);
        }

        /// <summary>
        /// 取得当前时间
        /// </summary>
        /// <returns>当前时间</returns>
        private DateTime GetTime()
        {
            DateTime getTime = new DateTime();
            getTime = DateTime.Now;
            return getTime;
        }
        #endregion
    }
}
