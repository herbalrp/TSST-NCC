using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace TSST_Menadzer
{
    class kurier
    {
        private struct chec_nawiazania_polaczenia
        {
            public string pocz, kon, zwr, nas, id_unk, przep;
            
        }

        private String IP;
        private int port;
        private StreamWriter writer;
        private StreamReader reader;
        private TcpClient client;
        private Menadzer menadzer;
        List<chec_nawiazania_polaczenia> lista_checi = new List<chec_nawiazania_polaczenia>();

        public kurier(String IP, int port, Menadzer menadzer)
        {
            this.IP = IP;
            this.port = port;
            this.menadzer = menadzer;
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress.Parse(IP), port);


                NetworkStream stream = client.GetStream();
                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);

                writer.WriteLine("JESTEM#"+menadzer.id+"#"+getIPAdress());
                writer.Flush();
                ThreadStart r = new ThreadStart(run);
                Thread newThread = new Thread(r);
                newThread.Start();
            }catch(Exception)
            {
                string data = DateTime.Now.ToString();
                menadzer.dodaj_informacje_o_bledzie("błąd#" + data + "#nie można połączyć się z kablami");
            }

        }

    

        private chec_nawiazania_polaczenia czy_nadal_jest_chec(string id_unk)
        {
            

            foreach (chec_nawiazania_polaczenia tmp in lista_checi)
            {
                

                if (tmp.id_unk.Equals(id_unk) )
                {
                    return tmp;

                }
            }


            return new chec_nawiazania_polaczenia();
        }

  

        private void release_chec(string id_unik)
        {

            string tmpS;
            foreach (chec_nawiazania_polaczenia tmp in lista_checi)
            {

                if (tmp.id_unk.Equals(id_unik))
                {

                    lista_checi.Remove(tmp);
                    break;

                }


                //foreach (chec_nawiazania_polaczenia tmp in lista_checi)
                //{
                //    if (tmp.pocz.Equals(kon) && tmp.kon.Equals(pocz))
                //    {
                //        lista_checi.Remove(tmp);
                //        break;

                //    }
                //}
            }
        }



        public static string getIPAdress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        internal bool przeslij_tablice(Wezel wez)
        {
           
                
                string s = stworz_przekaz(wez);
                return przeslij_wiadomosc(s, wez.getID(), wez.getIP());

           

        }

        public void zerwij_polaczenie()
        {
            if(writer!=null)
            writer.Close();
            if(reader!=null)
            reader.Close();
            if(client!=null)
            client.Close();
        }

        private string stworz_przekaz(Wezel wez)
        {
            string s = "TABLICA" + "#";
            foreach (wiersz a in wez.gettabele())
            {
                s += a.Label + "#" + a.NextHop + "#" + a.NextLabel + "#";
            }
            s += "KONIEC";
            return s;
        }

        private void run()
        {
            try {
                while (true)
                {

                    string s = reader.ReadLine();
                    przyjmij_przekaz(s);
                }
            }catch(Exception e)
            {
                menadzer.dodaj_informacje_o_bledzie("błąd#nie można odczytać wiadomości z kabli - sprawdź kable");
            }
        }

        private void wyslij(string s)
        {
            writer.WriteLine(s);
            writer.Flush();
        }

        private bool przeslij_wiadomosc(String s, String ID, String IP)
        {
            lock (this)
            {
                try
                {
                    String x = "TABLICA#"+ID+"#" + s;
                    if(client.Connected)
                    {
                    writer.WriteLine(x);
                    writer.Flush();
                    string data = DateTime.Now.ToString();
                    menadzer.dodaj_informacje_o_bledzie("OK#"+data+"#wyslano wiadomość poprawnie");
                    }
                   else
                    {
                        string data = DateTime.Now.ToString();
                        menadzer.dodaj_informacje_o_bledzie("ERROR#" + data + "#nie można przesłać wiadomości - sprawdź kable");
                    }
                   
                }catch(Exception e)
                {
                    string data = DateTime.Now.ToString();
                    menadzer.dodaj_informacje_o_bledzie("błąd#" + data + "#nie można przesłać wiadomości - sprawdź kable");
                    return false;
                }
                return true;
            }

        }

        private void przyjmij_przekaz(string s)
        {
            String[] przekaz = s.Split(new Char[] { '#' });

            switch(przekaz[0])
            {
                case "JESTEM": przyjmij_Jestem(przekaz); break;
                case "CallRequest": CallRequest1(przekaz);  break;
                case "CallRelease": callrelease(przekaz) ;break;
                case "CallCoordination": callcoordination(przekaz);break;
                case "ConnectionRequest": connectionrequest(przekaz); break;
                case "CallAccept": callaccept(przekaz); break;

            }
        }

        private void callrelease(string[] przekaz) //DOKOŃCZYĆ
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From " + przekaz[1] + "# CallRelease(" + przekaz[1] + "," + przekaz[2] + "," + przekaz[3]+")");

            bool potwierdzenie = true;
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[1] + ")");
            string adres1 = menadzer.directory.DIrectoryRequest(przekaz[1]);
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
            if (adres1 == null)
            {
                potwierdzenie = false;
            }
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
            string adres2 = menadzer.directory.DIrectoryRequest(przekaz[2]);
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
            if (adres2 == null)
            {
                potwierdzenie = false;
            }
            if (potwierdzenie)
            {
                menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[1] + ", " + adres1 + ", " + przekaz[2] + ", " + adres2 + ", " +przekaz[3] +", " + "RELEASE" + ")");
                wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[1] + "#" + adres1 + "#" + przekaz[2] + "#" + adres2 + "#0#"+  przekaz[3] + "#" + "RELEASE");


                chec_nawiazania_polaczenia tmp = new chec_nawiazania_polaczenia();
                tmp = czy_nadal_jest_chec(przekaz[3]);
                if (tmp.nas != null)
                {

                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.nas + "# CallCoordination(" + przekaz[1] + "," + przekaz[2] + ", " + przekaz[3]+", " + "RELEASE" + ")");
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.nas + "#CallCoordination#" + menadzer.id+"#" + przekaz[1] + "#" + przekaz[2] + "#0#" +przekaz[3] +"#" + "RELEASE" );

                }
                else {

                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + przekaz[2] + "# CallRelease(" + przekaz[1] + ", " + przekaz[2]+ ", " +przekaz[3]+ ")");
                    string[] split_id = przekaz[1].Split(new char[] { '@' });
                    if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRelease#" + przekaz[1] + "#" + przekaz[2] + "#" +przekaz[3]);
                }
                if (tmp.zwr != null)
                {

                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.zwr + "# CallCoordination(" + przekaz[1] + "," + przekaz[2] + ", " + "RELEASE" + ")");
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.zwr + "#CallCoordination#" + menadzer.id + "#" + przekaz[1] + "#" + przekaz[2] + "#0#" + przekaz[3] + "#" + "RELEASE");

                }

            }
            release_chec(przekaz[3]);
           
        }

        private void callcoordination(string[] przekaz)
        {
          if(przekaz.Length==7)
            {
                przyjecie_odpowiedzi_callcoordination(przekaz);
            }  else
            {//żądanie callcoordination
                przyjecie_callcoordination(przekaz);
            }
            
        }

        private void przyjecie_odpowiedzi_callcoordination(string[] przekaz)
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From NCC" + przekaz[1] + "# CallCoordination(" + przekaz[2] + "," + przekaz[3] + ", " +przekaz[4]+ ", " + przekaz[5] +", " + przekaz[6] + ")");
            chec_nawiazania_polaczenia tmp = czy_nadal_jest_chec( przekaz[5]);
            if (tmp.kon != null)
            {

                if (tmp.zwr != null)
                {//gdy węzeł nadawczy nie jest w domenie
                    if (przekaz[6].Equals("CONFIRMATION"))
                    {
                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[3] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[3]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }
                        if (potwierdzenie)
                        {
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[2] + ", " + adres1 + ", " + przekaz[3] + ", " + adres2 + ", "+tmp.przep+ ", "+przekaz[5]+ ")");
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[2] + "#" + adres1 + "#" + przekaz[3] + "#" + adres2 + "#" + tmp.przep + "#" +przekaz[5]);
                           
                        }
                       

                    }
                    else if (przekaz[6].Equals("REJECTION"))
                    {

                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.zwr + "# CallCoordination(" + przekaz[2] + ", " + przekaz[3] +", " + przekaz[4] + ", " + przekaz[5] + ", REJECTION)");

                        wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.zwr + "#CallCoordination#"+menadzer.id+"#" + przekaz[2] + "#" + przekaz[3] + "#" + przekaz[4] + "#" + przekaz[5] +"#REJECTION#");
                        release_chec(przekaz[5]);
                    }
                    else if (przekaz[6].Equals("RELEASE"))
                    {
                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[3] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[3]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }
                        if (potwierdzenie)
                        {
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[2] + ", " + adres1 + ", " + przekaz[3] + ", " + adres2 + ", "+przekaz[4] + ", "+przekaz[5]+ ", " + "RELEASE" + ")");
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[2] + "#" + adres1 + "#" + przekaz[3] + "#" + adres2 + "#0" + "#" +przekaz[5]+ "#" + "RELEASE");
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + przekaz[3] + "# CallRelease(" + przekaz[2] + ", " + przekaz[3] + ", "  +przekaz[5]+ ")");
                            string[] split_id = przekaz[2].Split(new char[] { '@' });
                            if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                                wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRelease#" + przekaz[2] + "#" + przekaz[3] + "#"  +przekaz[5]);
                        }
                        release_chec(przekaz[5]);
                    }
                }
                else
                {//gdy węzeł nadawczy jest w domenie
                    if (przekaz[6].Equals("CONFIRMATION"))
                    {
                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[3] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[3]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }
                        if(potwierdzenie)
                        {
 menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[2] + ", " +adres1 + ", " + przekaz[3] +", " +adres2+ ", " + tmp.przep + ", " + przekaz[5] + ")");
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[2] + "#" +adres1 + "#" + przekaz[3] + "#" +adres2+"#" + tmp.przep + "#" + przekaz[5]);
                        }
                       
                    }
                    else if (przekaz[6].Equals("REJECTION"))
                    {
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + tmp.pocz + "# CallRequest(" + przekaz[2] + ", " + przekaz[3] + ", "+przekaz[4]+ ", " + przekaz[5]+", " + ", REJECTION)");
                        string[] split_id = przekaz[2].Split(new char[] { '@' });
                        if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRequest#" + przekaz[2] + "#" + przekaz[3] + "#" +przekaz[4] + "#" +przekaz[5] + "#REJECTION");
                        release_chec(przekaz[5]);
                    }
                    else if (przekaz[6].Equals("RELEASE"))
                    {
                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[3] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[3]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }
                        if (potwierdzenie)
                        {
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[2] + ", " + adres1 + ", " + przekaz[3] + ", " + adres2 + ", " +tmp.przep + ", " + przekaz[5] + "RELEASE" +")");
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[2] + "#" + adres1 + "#" + przekaz[3] + "#" + adres2 + "#" + tmp.przep + "#" +  przekaz[5] + "#RELEASE");
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + przekaz[3] + "# CallRelease(" + przekaz[2] + ", "  + przekaz[3] + ", " +przekaz[5]+ ")");
                            string[] split_id = przekaz[2].Split(new char[] { '@' });
                            if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                                wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRelease#" + przekaz[2] + "#" + przekaz[3] + "#" +przekaz[5]);
                        }
                        release_chec( przekaz[5]);
                    }
                }

            }
        }

        private void przyjecie_callcoordination(string[] przekaz)
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From NCC" + przekaz[1] + "# CallCoordination(" + przekaz[2] + "," + przekaz[3]+ "," + przekaz[4] + "," +przekaz[5]+ ")");
            bool potwierdzenie = true;
            //menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[3] + ")");

            //string adres = menadzer.directory.DIrectoryRequest(przekaz[3]);
            //menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres + ")");
            //if (adres == null)
            //{
            //    potwierdzenie = false;
            //}
         
                menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Policy S.# Policy(" + przekaz[2] + ", " + przekaz[3] + ")");
                if (menadzer.policy.Policy_check(przekaz[2], przekaz[3]))
                {
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Policy S.# Policy(CONFIRMATION)");
                }
                else
                {
                    potwierdzenie = false;
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Policy S.# Policy(REJECTION)");
                }

            
            if (potwierdzenie)
            {
                string[] split_domena = przekaz[3].Split(new char[] { '@' });
                if (!split_domena[1].Equals(menadzer.domena))
                {
                    //DOPISAĆ DO JAKIEGO NCC WYSYŁA SIĘ!!!
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC#CallCoordination(" + przekaz[2] + ", " + przekaz[3] + "," + przekaz[4] + "," + przekaz[5] + ")");
                    chec_nawiazania_polaczenia chec_tmp = new chec_nawiazania_polaczenia();
                    chec_tmp.pocz = przekaz[2];
                    chec_tmp.kon = przekaz[3];
                    chec_tmp.przep = przekaz[4];
                    chec_tmp.id_unk = przekaz[5];
                    chec_tmp.zwr = przekaz[1];
                    string id_NCC;
                   if( menadzer.slownik.TryGetValue(split_domena[1],out id_NCC))
                    {
chec_tmp.nas = id_NCC;
                    lista_checi.Add(chec_tmp);

                    wyslij("Sygnalizuj#" + menadzer.id + "#" + id_NCC + "#CallCoordination#" + menadzer.id + "#" + przekaz[2] + "#" + przekaz[3] + "#" + przekaz[4] + "#"+ przekaz[5]);

                    }
                    
                }
                else
                {
                   
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + przekaz[3] + "# CallAccept(" + przekaz[2] + ", " + przekaz[3] + ", " + przekaz[4] + ", " + przekaz[5]+")");
                        chec_nawiazania_polaczenia chec_tmp = new chec_nawiazania_polaczenia();
                        chec_tmp.pocz = przekaz[2];
                        chec_tmp.kon = przekaz[3];
                    chec_tmp.przep = przekaz[4];
                    chec_tmp.id_unk = przekaz[5];
                    chec_tmp.zwr = przekaz[1];
                        lista_checi.Add(chec_tmp);
                    string[] split_id = przekaz[3].Split(new char[] { '@' });
                    if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallAccept#" + przekaz[2] + "#" + przekaz[3] + "#"+przekaz[4] + "#" + przekaz[5]);
                    
                    


                }


            }
            else
            {
                menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + przekaz[1] + "# CallCoordination(" + przekaz[2] + ", " + przekaz[3] + ", "+przekaz[4] + ", "+przekaz[5]+ ", REJECTION)");
                wyslij("Sygnalizuj#" + menadzer.id + "#" + przekaz[1] + "#CallCoordination#" + menadzer.id+"#"+przekaz[2] + "#" + przekaz[3] + "#" + przekaz[4] + "#" + przekaz[5] + "#" + "#REJECTION#");

            }

        }

        private void connectionrequest(string[] przekaz)
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From CC"+menadzer.id_CC + "# ConnectionRequest(" + przekaz[1] + "," + przekaz[2]+")");
            chec_nawiazania_polaczenia tmp = czy_nadal_jest_chec(przekaz[1]);
            if (tmp.pocz != null)
            {
                

                if (przekaz[2].Equals("REJECTION"))
                {
                    if (tmp.nas != null)
                    {
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.nas + "# CallCoordination(" + tmp.pocz + "," + tmp.kon + ", " + tmp.przep +", "+tmp.id_unk + "RELEASE" + ")");
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.nas + "#CallCoordination#" +menadzer.id +"#" + tmp.pocz + "#" + tmp.kon+ "#" + " #" + tmp.id_unk + "#" + "RELEASE");
                    }
                    else
                    {
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + tmp.kon + "# CallRelease(" + tmp.pocz+ ", " + tmp.kon + ", " +tmp.id_unk + ")");
                    
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.kon + "#CallRelease#" + tmp.pocz + "#" + tmp.kon + "#" + tmp.id_unk);
                    }

                   
                }
                
                
                if (tmp.zwr != null)
                {//gdy węzeł nadawczy nie jest w domenie
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.zwr + "# CallCoordination(" + tmp.pocz + "," + tmp.kon + ", " + tmp.id_unk + ", " + przekaz[2] + ")");
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.zwr + "#CallCoordination#" + menadzer.id + "#" + tmp.pocz + "#" + tmp.kon + "#" + " #" + tmp.id_unk + "#" + przekaz[2]);
                   
                    
                }
                else
                {//gdy węzeł nadawczy jest w domenie
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + tmp.pocz + "# CallRequest(" + tmp.pocz + "," + tmp.kon + ", " +tmp.przep + ", " +tmp.id_unk +", " + przekaz[2] + ")");
                    string[] split_id = przekaz[1].Split(new char[] { '@' });
                    if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRequest#" + tmp.pocz + "#" + tmp.kon + "#" + tmp.przep + "#" + tmp.id_unk+ "#" + przekaz[2] + "#");
                     
                }
                }
                
            
            if (przekaz[2].Equals("REJECTION"))
            {
                release_chec(przekaz[1]);
            }

        }

        private void callaccept(string[] przekaz)
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From " + przekaz[2] + "# CallAccept(" + przekaz[1] + "," + przekaz[2] +", "+przekaz[3] + ", " + przekaz[4] + ", " + przekaz[5] + ")");
            chec_nawiazania_polaczenia tmp = czy_nadal_jest_chec( przekaz[4]);
            if (tmp.kon != null)
            {

                if (tmp.zwr != null)
                {//gdy węzeł nadawczy nie jest w domenie
                    if (przekaz[5].Equals("CONFIRMATION"))
                    {
                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[1] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[1]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }
                        if(potwierdzenie)
                        {
                            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC" + menadzer.id_CC + "# ConnectionRequest(" + przekaz[1] + ", " + adres1 + ", " + przekaz[2] + ", " + adres2 + ", " +przekaz[3] + ", " +przekaz[4]+ ")");
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[1] + "#" + adres1 + "#" + przekaz[2] + "#" + adres2 + "#" + przekaz[3] + "#" +  przekaz[4]);
                        }
                     

                    }
                    else if (przekaz[5].Equals("REJECTION"))
                    {

                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC " + tmp.zwr+ "# CallCoordination(" + przekaz[1] + ", " + przekaz[2] + ", " + przekaz[4] + ", REJECTION)");
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + tmp.zwr + "#CallCoordination#"+menadzer.id+"#" + przekaz[1] + "#" + przekaz[2] + "# #" + przekaz[4] + "#REJECTION");
                        release_chec(przekaz[4]);
                    }
                } else
                {//gdy węzeł nadawczy jest w domenie
                    if (przekaz[5].Equals("CONFIRMATION"))
                     {

                        bool potwierdzenie = true;
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[1] + ")");
                        string adres1 = menadzer.directory.DIrectoryRequest(przekaz[1]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres1 + ")");
                        if (adres1 == null)
                        {
                            potwierdzenie = false;
                        }
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2] + ")");
                        string adres2 = menadzer.directory.DIrectoryRequest(przekaz[2]);
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres2 + ")");
                        if (adres2 == null)
                        {
                            potwierdzenie = false;
                        }


                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To CC "+menadzer.id_CC + "# ConnectionRequest(" + przekaz[1] + ", " + adres1 + ", " + przekaz[2] + ", " + adres2 + ", " + przekaz[3] + ", " + przekaz[4] + ")");
                        wyslij("Sygnalizuj#" + menadzer.id + "#" + menadzer.id_CC + "#ConnectionRequest#" + przekaz[1] + "#" +adres1 + "#" + przekaz[2] + "#" +adres2 + "#" + przekaz[3] + "#" +  przekaz[4]);
                    }
                    else if(przekaz[5].Equals("REJECTION"))
                      {
                        menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " + tmp.pocz+ "# CallRequest(" + przekaz[1] + ", " + przekaz[2] + ", " + przekaz[3] + ", " + przekaz[4] + ", REJECTION)");
                        string[] split_id = przekaz[1].Split(new char[] { '@' });
                        if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                            wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRequest#" + przekaz[1] + "#" + przekaz[2] + "#" + przekaz[3] + "#" + przekaz[4] + "#REJECTION#");
                        release_chec(przekaz[4]);
                    }
                }

            }
           
        }

        private void CallRequest1(string[] przekaz)
        {
            menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From "+przekaz[1]+"# CallRequest(" + przekaz[1] +","+przekaz[2]+"," +przekaz[3] +  ")"); //3-przepływnosc
            bool potwierdzenie = true;
            //menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Directory# DirectoryRequest(" + przekaz[2]+")");
            
            //string adres = menadzer.directory.DIrectoryRequest(przekaz[2]);
            //menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Directory S.# DirectoryRequest(" + adres + ")");
            //if (adres==null)
            //{
            //    potwierdzenie = false;
            //} else
            
                menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To Policy S.# Policy(" + przekaz[1]+", "+przekaz[2] + ")");
                if(menadzer.policy.Policy_check(przekaz[1], przekaz[2]))
                {
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Policy S.# Policy(CONFIRMATION)");
                }
                else
                {
                    potwierdzenie = false;
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#From Policy S.# Policy(REJECTION)");
                }

            Random rand = new Random();
            int r = rand.Next();
            if(potwierdzenie)
            {
                string[] split_domena = przekaz[2].Split(new char[] { '@' });
                if(!split_domena[1].Equals(menadzer.domena))
                {
                    //DOPISAĆ DO JAKIEGO NCC WYSYŁA SIĘ!!!
                    menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To NCC#CallCoordination(" + przekaz[1] + ", " + przekaz[2]+"," + przekaz[3]+"," +przekaz[1]+przekaz[2]+r+ ")");
                    chec_nawiazania_polaczenia chec_tmp = new chec_nawiazania_polaczenia();
                    chec_tmp.pocz = przekaz[1];
                    chec_tmp.kon = przekaz[2];
                    chec_tmp.id_unk = przekaz[1] + przekaz[2] + r;
                    chec_tmp.przep = przekaz[3];
                    string NCC_id;
                    if(menadzer.slownik.TryGetValue(split_domena[1], out NCC_id))
                    {
                    chec_tmp.nas = NCC_id;               
                    lista_checi.Add(chec_tmp);
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + chec_tmp.nas + "#CallCoordination#"+menadzer.id+"#" + przekaz[1] + "#" + przekaz[2] + "#" +przekaz[3] + "#" + przekaz[1] + przekaz[2] + r);

                    }
                  
                }
                else
                {
                    
                      menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " +przekaz[2] +"# CallAccept(" + przekaz[1] + ", " + przekaz[2] + ", " +przekaz[3] + ", " + przekaz[1] + przekaz[2] + r + ")");
                    chec_nawiazania_polaczenia chec_tmp = new chec_nawiazania_polaczenia();
                    chec_tmp.pocz = przekaz[1];
                    chec_tmp.kon = przekaz[2];
                    chec_tmp.przep = przekaz[3];
                    chec_tmp.id_unk = przekaz[1] + przekaz[2] + r;
                    lista_checi.Add(chec_tmp);
                    string[] split_id = przekaz[2].Split(new char[] { '@' });
                    if(menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallAccept#" + przekaz[1] + "#" + przekaz[2] + "#" + przekaz[3] + "#" + przekaz[1] + przekaz[2] + r);
                                      
                   

                }


            }
            else
            {
                menadzer.dodaj_log(DateTime.Now.ToLongTimeString() + "#To " +przekaz[1]+"# CallRequest(" + przekaz[1] + ", " + przekaz[2] +", REJECTION)") ;
                string[] split_id = przekaz[1].Split(new char[] { '@' });
                if (menadzer.czy_jest_w_mojej_podsieci(split_id[0]))
                    wyslij("Sygnalizuj#" + menadzer.id + "#" + split_id[0] + "#CallRequest#" + przekaz[1] + "#" + przekaz[2] + "#" + przekaz[3] + "#" + przekaz[1] + przekaz[2] + r + "#REJECTION#");
                
            }
            
            

        }

        private void przyjmij_Jestem(String[] przekaz)
        {
           
                try
                {
                    string nazwa = przekaz[1];
                    string IP = przekaz[2];                    
                    
                    Wezel wezel = new Wezel(nazwa, IP);
                    menadzer.dodaj_wezel(wezel);
                

                }
                catch (Exception e)
                {
                    string data = DateTime.Now.ToString();
                    menadzer.dodaj_informacje_o_bledzie("błąd#" + data + "#otrzymano błędną wiadomość");
                }
            } 
        }
    }

