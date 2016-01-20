using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSST_Menadzer
{
    public class Wezel
    {


        private String ID;
        private String IP;
    
        private bool stan_operacyjny = true;
        private DateTime last_alive;
        List<wiersz> tablica = new List<wiersz>();

        public Wezel(String ID, String IP)
        {
            this.ID = ID;
            this.IP = IP;
            

        }

        public String getID()
        {
            return ID;
        }

        public String getIP()
        {
            return IP;
        }
      
        public bool getstan()
        {
            return stan_operacyjny;
        }
        public void setstan(bool stan)
        {
            this.stan_operacyjny = stan;
            if(stan)
            {
                last_alive = DateTime.Now;
                Console.WriteLine("wpisano last_alive dla: " + ID + " " + last_alive);
            }
        }
        public DateTime getdate()
        {
            return last_alive;
        }
        public List<wiersz> gettabele()
        {
            if(tablica==null)
            {
                return null;
            }
            return tablica;
        }
        public void settabele(List<wiersz> tabela)
        {

            this.tablica = tabela;
        }
    }
}
