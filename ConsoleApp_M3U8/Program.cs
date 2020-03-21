using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;

namespace ConsoleApp_M3U8
{
    class Program
    {
        Queue q = new Queue();
        Queue qThread = new Queue();
        Info info = new Info();
        //Down down = null;
        static int now = -1;
        static void Main(string[] args)
        {
            new Program().init();
            Console.ReadKey();
        }

        private void init()
        {
            if (getM3(""))
            {
                Console.WriteLine("无法获取m3u8信息，按任意键结束");
                return;
            }
            for (int i = 0; i < Info.ThreadNum; i++)
            {
                qThread.Enqueue(i);
                new Thread(new ThreadStart(thVoid)).Start();
            }
            downIng();
        }

        private bool getM3(string url)
        {
            if (url == "")
            {
                Console.WriteLine("请输入m3u8网络地址");
                url = Console.ReadLine();
            }
            string body = "";
            using (WebClient web = new WebClient())
            {
                try
                {
                    body = web.DownloadString(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WebClent发生错误：{0}\n{1}",
                        ex.Message, url);
                }
            }
            if (!body.Contains("#EXT-X-ENDLIST"))
            {
                return true;
            }
            int num = 0;
            foreach (var item in body.Split('\n'))
            {
                if (item.Length > 0)
                {
                    if (item.StartsWith("#"))
                    {
                        info.m3u8 += item + "\n";
                    }
                    else
                    {
                        q.Enqueue(num);
                        info.m3u8 += (num++).ToString() + ".ts\n";
                        info.m3ts.Add(item);
                    }
                }
            }
            info.num = num;
            info.Path = Environment.CurrentDirectory + "\\";
            File.WriteAllText(info.Path + "0m3u8.m3u8", info.m3u8);
            //down = new Down(info);
            return false;
        }

        private void downIng()
        {
            int qNum;
            do
            {
                Thread.Sleep(100);
                qNum = q.Count;
                Console.WriteLine("线程={0} 进度{1}/{2} 当前={3}",
                    qThread.Count, qNum, info.num, now);
            } while (qThread.Count > 0);
        }

        private void thVoid()
        {
            int tNum = 0;
            while (q.Count > 0)
            {
                Thread.Sleep(1);
                tNum = (int)q.Dequeue();
                now = tNum+1;
                //down.down(tNum);
            }
            qThread.Dequeue();
        }
    }
}
