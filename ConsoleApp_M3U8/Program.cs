using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
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
                    return false;
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
            info.Path = Environment.CurrentDirectory + "\\" + GetMD5HashString(Encoding.UTF8, url) + "\\";
            Directory.CreateDirectory(info.Path);
            File.WriteAllText(info.Path + "0index.m3u8", info.m3u8);
            string bat = @"D:\RuanJian\0xia\xdown\ffmpeg.exe " +
                "-allowed_extensions ALL -protocol_whitelist " +
                "\"file,http,crypto,tcp\" -i 0index.m3u8 -c copy \"" +
                Environment.CurrentDirectory + "\\" + "0out.mp4\"\n" +
                "explorer /e,/select,\"" + Environment.CurrentDirectory + "\\" + "0out.mp4\"";
            File.WriteAllText(info.Path + "0all.bat", bat);
            //down = new Down(info);
            Console.WriteLine("path={0}\n", info.Path);
            int left = url.LastIndexOf('/');
            info.UrlLeft = url.Substring(0, left + 1);
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
                    qThread.Count, info.num - qNum, info.num, now);
            } while (qThread.Count > 0);
        }

        private void thVoid()
        {
            int tNum = -1;
            while (q.Count > 0)
            {
                Thread.Sleep(1);
                try
                {
                    tNum = (int)q.Dequeue();
                }
                catch (Exception)
                {
                }
                if (tNum == -1)
                {
                    Thread.Sleep(5);
                    continue;
                }
                now = tNum+1;
                //down.down(tNum);
                string filename = info.Path + tNum.ToString();
                using (WebClient web = new WebClient())
                {
                    try
                    {
                        web.DownloadFile(info.UrlLeft + info.m3ts[tNum], filename + ".tmp");
                        File.Move(filename + ".tmp", filename + ".ts");
                    }
                    catch (Exception)
                    {
                        q.Enqueue(tNum);
                        
                    }
                }
                tNum = -1;
            }
            qThread.Dequeue();
        }

        private static System.Security.Cryptography.MD5 p_md5 = System.Security.Cryptography.MD5.Create();
        //使用指定编码将字符串散列
        public static string GetMD5HashString(Encoding encode, string sourceStr)
        {
            StringBuilder sb = new StringBuilder();
            byte[] source = p_md5.ComputeHash(encode.GetBytes(sourceStr));
            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
