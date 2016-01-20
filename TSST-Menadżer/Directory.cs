using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST_Menadżer
{
    class Directory
    {

        private Dictionary<string, string> slownik = new Dictionary<string, string>();

        public Directory(string path)
        {
            string path1 = path + "_directory.txt";
            string[] lines = System.IO.File.ReadAllLines(@path1);
            string[] split;
          
            for (int i = 0; i < lines.Length; i++)
            {
                split = lines[i].Split(new Char[] { '#' });
                slownik.Add(split[0], split[1]);
            }
        }

        public string DIrectoryRequest(string name)
    {
        string adres;
            bool powodzenie = slownik.TryGetValue(name, out adres);
            if(powodzenie)
            {
                return adres;
            } else
            {
                return null;
            }
    }


    }

    
}
