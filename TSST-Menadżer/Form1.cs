using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSST_Menadzer
{
    public partial class Form1 : Form
    {
        private Menadzer menadzer;
        private String Swezel;
        private int licznik_pojemnosci_stringa = 0;
        private int licznik_pojemnosci2 = 0;
        
        
        public Form1()
        {
            InitializeComponent();

            
            

            
        }

      

        public void dodaj_informacje_o_bledzie(String s)
        {
            if(licznik_pojemnosci_stringa<=100)
            {
                licznik_pojemnosci_stringa++;
            } else
            {
                richTextBox1.Text = "";
                licznik_pojemnosci_stringa = 0;
            }
            try
            {
 richTextBox1.Text += "\r\n" + s;
            }
            catch(Exception e)
            {

            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {

            List<wiersz> tablica = new List<wiersz>();
            foreach (DataGridViewRow a in dataGridView3.Rows)
            {
                if (a.Cells[0].Value != null && a.Cells[1].Value != null && a.Cells[2].Value != null)
                { 
                String x, y, z;
                x = a.Cells[0].Value.ToString();
                y = a.Cells[1].Value.ToString();
                z = a.Cells[2].Value.ToString();
                wiersz wiersz = new wiersz(x, y, z);
                tablica.Add(wiersz);
                }
            }
            menadzer.przeslij_tablice(Swezel, tablica);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pokaz_tablice();
        }

        private void pokaz_tablice()
        {
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            
            Wezel wezel = menadzer.getwezel(comboBox1.Text);
            foreach(wiersz wiersz in wezel.gettabele())
            {
                dataGridView2.Rows.Add(wiersz.Label, wiersz.NextHop, wiersz.NextLabel);
                dataGridView3.Rows.Add(wiersz.Label, wiersz.NextHop, wiersz.NextLabel);
            }
            Swezel = comboBox1.Text;
        }

        public void dodaj_wezel(Wezel wezel)
        {
            Console.WriteLine("JESTEM");
            try {
                dataGridView1.Invoke(new Action(delegate ()
                {
                    dataGridView1.Rows.Add(wezel.getID(), wezel.getstan());
                }));


                comboBox1.Invoke(new Action(delegate ()
                {
                    comboBox1.Items.Add(wezel.getID());
                }));
            }
            catch(Exception)
            {

            }
            
        }

        public void wypisz_log(String s)
        {
            try
            {


                richTextBox2.Invoke(new Action(delegate ()
                {
                    if(licznik_pojemnosci2>100)
                    {
                        richTextBox2.Clear();
                        licznik_pojemnosci2 = 0;
                    } else
                    {
                        richTextBox2.Text += "\r\n" + s;
                    }                    


                }));
            }
            catch (Exception)
            {
                Console.WriteLine("BŁĄD KRYTYCZNY");
            }
        }

        public void zmien_stan_operacyjny(Wezel wezel)
        {
            try
            {


                dataGridView1.Invoke(new Action(delegate ()
                {

                    foreach (DataGridViewRow a in dataGridView1.Rows)
                    {
                        if (a.Cells[0].Value.Equals(wezel.getID()))
                        {

                            a.Cells[1].Value = wezel.getstan();

                            break;
                        }
                    }


                }));
            } catch(Exception)
            {
                Console.WriteLine("BŁĄD KRYTYCZNY");
            }
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Swezel!=null)
            {
                pokaz_tablice();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            menadzer.stworz_kuriera();
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string path = comboBox2.SelectedItem + ".txt";
            
            string[] lines = System.IO.File.ReadAllLines(@path);
            int port = int.Parse(lines[1]);
            string id = lines[3];
            string id_CC = lines[5];
            string domena = lines[7];
            Dictionary<string, string> slownik = new Dictionary<string, string>();
            for(int i=9; i<lines.Length;i++)
            {
                string[] split = lines[i].Split(new char[] { '#' });
                slownik.Add(split[0], split[1]);
            }

             path = comboBox2.SelectedItem + "_elementy.txt";

            lines = System.IO.File.ReadAllLines(@path);
            List<String> elementy = new List<string>();
            foreach(string s in lines)
            {
                elementy.Add(s);
            }
            menadzer = new Menadzer(port, id, domena, id_CC, comboBox2.SelectedItem.ToString(), slownik, this, elementy);
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox2.Invoke(new Action(delegate ()
            {

                richTextBox2.Text = "";


            }));
        }
    }
}
