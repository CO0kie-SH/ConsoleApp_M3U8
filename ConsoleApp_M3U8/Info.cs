using System.Collections.Generic;

namespace ConsoleApp_M3U8
{
    class Info
    {
        public List<string> m3ts = new List<string>();
        public static int ThreadNum = 1;
        public string m3u8 { get; internal set; }
        public string UrlLeft { get; internal set; }
        public string Path { get; internal set; }
        public int num { get; internal set; }
    }
}
