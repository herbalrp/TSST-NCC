using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TSST_Menadżer;

namespace TSST_Menadzer
{
    public struct wiersz
    {
        public String Label;
        public String NextHop;
        public String NextLabel;

        public wiersz(String Label, String NextHop, String NextLabel)
        {
            this.Label = Label;
            this.NextHop = NextHop;
            this.NextLabel = NextLabel;
        }
      
    }

    class Menadzer
    {
        
        private List<Wezel> lista = new List<Wezel>();
        Form1 form;
        private kurier kurier;
       public Directory directory;
        public Policy policy;
        private int port;
        public string id;
        public string id_CC;
        private string path;
        public Dictionary<string, string> slownik;
        public List<String> elementy;
        public string domena;

        public Menadzer(int port, string id, string domena, string id_CC, string path, Dictionary<string, string> slownik, Form1 form, List<String> elementy)
        {
            this.port = port;
            this.id = id;
            this.id_CC = id_CC;
            this.path = path;
            this.slownik = slownik;
            this.domena = domena;
            this.elementy = elementy;
            directory = new Directory(path);
            policy = new Policy(path);
            stworz_kuriera();
            this.form = form;
            ThreadStart r = new ThreadStart(sprawdzacz);
            Thread newThread = new Thread(r);
            newThread.Start();
        }

        public void stworz_kuriera()
        {
            if(kurier!=null)
            {
                kurier.zerwij_polaczenie();
            }
            kurier = new kurier("192.168.61.25", port, this);
            
        }

        public bool czy_jest_w_mojej_podsieci(string s)
        {
            bool v = false;
            foreach(string g in elementy)
            {
                if(g.Equals(s))
                {
                    v = true;
                    break;
                }
            }
            return v;
        }

        public void dodaj_wezel(Wezel wezel)
        {
            lock(this)
                {
                bool jest = false;
                foreach (Wezel wez in lista)
                {
                    if (wez.getID().Equals(wezel.getID()))
                    {
                        wez.setstan(true);
                        form.zmien_stan_operacyjny(wez);
                        jest = true;
                        break;
                    }
                }
                if (!jest)
                {
                    lista.Add(wezel);
                    form.dodaj_wezel(wezel);

                }

                
            }
        }

        private void sprawdzacz()
        {
            while (true)
            {
                Thread.Sleep(5000);
                foreach (Wezel wez in lista)
                {
                    TimeSpan data = DateTime.Now - wez.getdate();
                    Console.WriteLine("Sprawdzam dla " + wez.getID() + " " + wez.getdate() + " " + data);
                    if (data.TotalSeconds > 10)
                    {
                        Console.WriteLine("Zmieniam na nieaktywny");
                        wez.setstan(false);
                        form.zmien_stan_operacyjny(wez);
                    }
                }
            }
        }

        internal void dodaj_informacje_o_bledzie(string v)
        {
            form.dodaj_informacje_o_bledzie(v);
        }

        public Wezel getwezel(String ID)
        {
            foreach(Wezel a in lista)
            {
                if(a.getID().Equals(ID))
                {
                    return a;
                }
            }
            return null;
        }

       

        public void przeslij_tablice(String wezel, List<wiersz> tablica)
        {
           foreach(Wezel wez in lista)
            {
                if(wez.getID().Equals(wezel))
                {
                    List<wiersz> tablica_tmp = wez.gettabele();
                    wez.settabele(tablica);
                   if(!kurier.przeslij_tablice(wez))
                    {
                        wez.settabele(tablica_tmp);
                    }
                    
                    break;
                }
            }
        }

        public void dodaj_log(string s)
        {
            form.wypisz_log(s);
        }
       

     


    }
}
