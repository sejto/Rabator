using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Rabator
{
    public partial class Form1 : Form
    {
        public string htmlString = "http://www.orlen.pl/PL/DlaBiznesu/HurtoweCenyPaliw/Strony/default.aspx";
        string file = "parametry.xml";
        public string Dataval = "";
        public string ONName = "";
        public string ONval = "";
        public string pb95Name = "";
        public string pb95val = "";
        public string ONAName = "";
        public string ONAval = "";
        public decimal Vat = 1.23m;
        public decimal pb95Detal;
        public decimal ADBlueDetal;
        public decimal ONADetal;
        public decimal ONDetal;
        public int grupa = 0;
        public int grupy = 12;

        public Form1()
        {
            InitializeComponent();
            Data_lbl.Text = "...";
        }

        private void Check_btn_Click(object sender, EventArgs e)
        {/// 1 - ON
         /// 2 - pb95
         /// 5 - ONA
         /// 2069 - AD BLUE
            grupa = 1;
            CenyOrlen();
            pb95Detal = Convert.ToDecimal(CenySlupek(2));
            pb95slupek_lbl.Text = pb95Detal.ToString();
            ADBlueDetal = Convert.ToDecimal(CenySlupek(2069));
            adbSlupek_lbl.Text = ADBlueDetal.ToString();
            ONADetal = Convert.ToDecimal(CenySlupek(5));
            ONAslupek_lbl.Text = ONADetal.ToString();
            ONDetal = Convert.ToDecimal(CenySlupek(1));
            ONslupek_lbl.Text = ONDetal.ToString();
            ObliczRabat(grupa);
        }

        public void CenyOrlen()
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = hw.Load(htmlString);

            HtmlNode DatavalN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblDate']");

            HtmlNode pb95N = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblFuelNamePb95']");
            HtmlNode pb95valN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblPb95Price']");

            HtmlNode ONN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblFuelNameONEkodisel']");
            HtmlNode ONvalN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblONEkodiselPrice']");

            HtmlNode ONAN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblFuelNameONArctic2']");
            HtmlNode ONANvalN = doc.DocumentNode.SelectSingleNode("//span [@id='ctl00_ctl00_SPWebPartManager1_g_753cafe9_2be0_414b_aa26_6f746d63d018_ctl00_lblONArctic2Price']");

            Dataval = DatavalN.InnerText.Replace("&nbsp;", " ");

            ONName = ONN.InnerText.Replace("&nbsp;", " ");
            ONval = ONvalN.InnerText.Replace("&nbsp;", " ");

            pb95Name = pb95N.InnerText.Replace("&nbsp;", " ");
            pb95val = pb95valN.InnerText.Replace("&nbsp;", " ");

            ONAName = ONAN.InnerText.Replace("&nbsp;", " ");
            ONAval = ONANvalN.InnerText.Replace("&nbsp;", " ");

            Data_lbl.Text = Dataval;
            pb95_lbl.Text = pb95val;
            ON_lbl.Text = ONval;
            ONA_lbl.Text = ONAval;
            //adb_lbl.Text = Adblueval.ToString();
         
        }
        public double CenySlupek(int paliwo)
        {
            string sql = "select cast(ROUND(Cenadet*(1+(CAST(Stawka AS DECIMAL))/10000),2 )AS DECIMAL(5,2)) as cena from towar where towid=" + paliwo;
            string keyname = "HKEY_CURRENT_USER\\MARKET\\serwerLokal";
            rejestrIO rejestr = new rejestrIO();
            string klucz = rejestr.czytajklucz(keyname, "SQLconnect", true); //parametry połączenia do bazy SQL zapisane w rejestrze
            SqlConnection cnn;
            SqlDataReader dataReader;
            SqlCommand command;
            cnn = new SqlConnection(klucz);
            try
            {
                cnn.Open();
                command = new SqlCommand(sql, cnn);
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    return Convert.ToDouble( dataReader.GetValue(0));
                    // MessageBox.Show(dataReader.GetValue(0).ToString());
                }
                dataReader.Close();
                command.Dispose();
                cnn.Close();
                return paliwo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie mogę połączyć się z bazą danych PC-Market ! " + ex);
                return paliwo;
            }


        }

        public void ObliczRabat(int grupa)
        {
            grupa_lbl.Text = grupa.ToString();
            string grupaData = DateTime.Now.ToString("yy.MM.dd"); ;
            decimal onr = Convert.ToDecimal(parametry("/Rabat/Grupa" + grupa + "/ON"), new System.Globalization.NumberFormatInfo());
            decimal pb95r = Convert.ToDecimal(parametry("/Rabat/Grupa" + grupa + "/PB95"), new System.Globalization.NumberFormatInfo());
            decimal adbr = Convert.ToDecimal(parametry("/Rabat/Grupa" + grupa + "/ADB"), new System.Globalization.NumberFormatInfo());
            decimal ONAr = Convert.ToDecimal(parametry("/Rabat/Grupa" + grupa + "/ONA"), new System.Globalization.NumberFormatInfo());

            if (onr.ToString() != "0,00")
            {
                grupaName_lbl.Text = "ORLEN" + grupaData + " - " + grupa;
            }
            else
                grupaName_lbl.Text = "Słupek";
            decimal ON = Convert.ToDecimal(ONval.Replace('.', ','));

            decimal ONrG = ((ON / 1000)-onr)*Vat;

            if (ONrG >= ONDetal && grupa == 5)
            {
                ONrG = ONDetal - 0.05m;
                MessageBox.Show("Cena hurt Orlenu jest wyższa niż słupek," + Environment.NewLine + "dla E100 obliczamy wtedy (cena_słupek - 0,05).", "Uwaga - sprawdź");
            }
            else
            {
                if (onr.ToString() != "0,00")
                    ONG_lbl.Text = String.Format("{0:0.00}", ONrG);
                else
                    ONG_lbl.Text = "Brak rabatu";
            }
           

            //ONG_lbl.Text = "Brak rabatu";


            if ( pb95r.ToString() != "0,00")
                {
                decimal pb95rG = pb95Detal - pb95r;
                pb95G_lbl.Text = pb95rG.ToString();
                }
                else
                pb95G_lbl.Text = "Brak rabatu";

            if (adbr.ToString() != "0,00")
            {
                decimal adbrG = ADBlueDetal - adbr;
                ADBG_lbl.Text = adbrG.ToString();
            }
            else
                ADBG_lbl.Text = "Brak rabatu";


            if (ONAr.ToString() != "0,00")
            {
                decimal ONArG = ONADetal - ONAr;
                ONAG_lbl.Text = ONArG.ToString();
            }
            else
                ONAG_lbl.Text = "Brak rabatu";
        }

        private void next_btn_Click(object sender, EventArgs e)
        {
            if (grupa == 0) { return; }
            else
            if (grupa < grupy)
            {
                grupa = grupa + 1;
                ObliczRabat(grupa);
            }
            else return;
        }

        private void prev_btn_Click(object sender, EventArgs e)
        {
            if (grupa == 0) { return; }
            else
            if (grupa >= 2)
            {
                grupa = grupa - 1;
                ObliczRabat(grupa);
            }
            else return;
        }
        public string parametry(string param)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("parametry.xml");
            XmlNode dane = xmlDoc.DocumentElement.SelectSingleNode(param);
            return dane.InnerText;
        }  //odczytuje gałąź konfiguracji z xml

        public string test1(string htmlString)
        {
            Data_lbl.Text = "Łączenie....";
            WebRequest request = WebRequest.Create(htmlString);
            WebResponse response = request.GetResponse();
            Stream dane = response.GetResponseStream();
            string html = String.Empty;

            using (StreamReader sr = new StreamReader(dane))
            {
                html = sr.ReadToEnd();
            }
            Data_lbl.Text = "OK";
            //'ctl00_lblDate'
            string data = "";
            // MessageBox.Show(html);
            return html;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
    class rejestrIO
    {
        const string salt = "f$4e9$#n!#98iaf542";
        public void zapiszklucz(string keyName, string valuename, string valuedata, bool crypt)
        {
            if (crypt != true)
            {
                Registry.SetValue(keyName, valuename, valuedata);
            }
            else
            {
                string valdatacrypt = Cipher.Encrypt(valuedata, salt);
                Registry.SetValue(keyName, valuename, valdatacrypt);
            }
        }
        public string czytajklucz(string keyName, string valuename, bool crypt)
        {
            if (crypt != true)
            {
                string klucz = (string)Registry.GetValue(keyName, valuename, "Value does not exist.");
                return klucz;
            }
            else
            {
                string kluczcrypt = (string)Registry.GetValue(keyName, valuename, "Value does not exist.");
                string klucz = Cipher.Decrypt(kluczcrypt, salt);
                return klucz;
            }
        }

    }
    public static class Cipher
    {
        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="password">Password</param>
        public static string Encrypt(string plainText, string password)
        {
            if (plainText == null)
            {
                return null;
            }

            if (password == null)
            {
                password = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            var bytesEncrypted = Cipher.Encrypt(bytesToBeEncrypted, passwordBytes);
            return Convert.ToBase64String(bytesEncrypted);

        }

        /// <summary>
        /// Decrypt a string.
        /// </summary>
        /// <param name="encryptedText">String to be decrypted</param>
        /// <param name="password">Password used during encryption</param>
        /// <exception cref="FormatException"></exception>
        public static string Decrypt(string encryptedText, string password)
        {
            if (encryptedText == null)
            {
                return null;
            }

            if (password == null)
            {
                password = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Cipher.Decrypt(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 5, 3, 8, 5, 6, 1, 3 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 5, 3, 8, 5, 6, 1, 3 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;

        }
    }

}
