using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Audioplayer
{
    class findAudio
    {
        public void finder(List<string> path, List<string> songs)
        {
            Regex rg = new Regex("(.mp3)|(.avi)|(.wav)", RegexOptions.IgnoreCase);
            string[] files = new string[4096];
            string[] dirs = new string[4096];
            List<string> dirList = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                if (Directory.Exists(path[i]))
                {
                    files = Directory.GetFiles(path[i]);
                    dirs = Directory.GetDirectories(path[i]);
                }

                foreach (string file in files)
                {
                    if (rg.Match(file).Length > 0)
                    {
                        songs.Add(file);
                    }
                }

                foreach (string dir in dirs)
                {
                    dirList.Add(dir);
                    finder(dirList, songs);
                }
            }
        }
    }
}
