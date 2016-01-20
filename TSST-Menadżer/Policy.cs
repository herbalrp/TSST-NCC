using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST_Menadżer
{
    class Policy
    {
        private struct policy_struct
        {
           public string id_pocz, id_kon, pozwolenie;

        }
        private List<policy_struct> lista = new List<policy_struct>();
        

        public Policy(string path)
        {
            string path1 = path + "_policy.txt";
            string[] lines = System.IO.File.ReadAllLines(@path1);
            string[] split;
            policy_struct tmp;
            for (int i=0; i< lines.Length; i++)
            {
                split = lines[i].Split(new Char[] { '#' });
                tmp = new policy_struct();
                tmp.id_pocz = split[0];
                tmp.id_kon = split[1];
                tmp.pozwolenie = split[2];
                lista.Add(tmp);
            }
        }

        public bool Policy_check(string id_pocz, string id_kon)
        {
          
            foreach(policy_struct tmp in lista)
            {
                if(tmp.id_pocz.Equals(id_pocz) && tmp.id_kon.Equals(id_kon))
                {
                    if (tmp.pozwolenie.Equals("accept"))
                    {
                        return true;
                    }
                    else if (tmp.pozwolenie.Equals("refusal")) 
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
