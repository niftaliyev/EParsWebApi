using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UcuzTapAz
{
    public class UcuzTapAzSettlementsNames
    {
        Dictionary<string, int> settlementNames = new Dictionary<string, int>();


        public UcuzTapAzSettlementsNames()
        {
            settlementNames.Add("Ceyranbatan q.", 1);
           
            settlementNames.Add("Digah q.", 3);
            settlementNames.Add("Fatmayı q.", 4);
            settlementNames.Add("Görədil q.", 5);
            settlementNames.Add("Hökməli q.", 6);
            settlementNames.Add("Corat q.", 7);
            settlementNames.Add("Qobu q.", 8);
            settlementNames.Add("Masazır q.", 9);
            settlementNames.Add("Mehdiabad q.", 10);
            settlementNames.Add("Müşviqabad q.", 11);
            settlementNames.Add("Novxanı q.", 12);
            settlementNames.Add("Pirəkəşkül q.", 13);
            settlementNames.Add("Saray q.", 14);
            settlementNames.Add("Yeni Corat q.", 15);
            settlementNames.Add("Zagulba q.", 16);
            settlementNames.Add("Alatava q.", 17);
            settlementNames.Add("28 May", 18);
            //Bineqedi
            settlementNames.Add("6 mikrorayon q.", 19);
            settlementNames.Add("7 mikrorayon q.", 20);
            settlementNames.Add("8 miktorayon q.", 21);
            settlementNames.Add("9 mikrorayon q.", 22);
            settlementNames.Add("Çiçək q.", 2);
            settlementNames.Add("Xocəsən q.", 25);
            settlementNames.Add("Sulutəpə q.", 28);
            settlementNames.Add("Rəsulzadə q.", 27);
            //End Bineqedi

            //Nizami
            settlementNames.Add("8 kilometr q.", 61);
            settlementNames.Add("Keşlə q.", 62);
            settlementNames.Add("NZS q.", 32);
            settlementNames.Add("UPD q.", 181);
            // end nizami

            //nerimanov
         


            settlementNames.Add("Xutor q.", 26);

           
            settlementNames.Add("Biləcəri q.", 23);
            settlementNames.Add("Binəqədi q.", 24);
            settlementNames.Add("Əhmədli q.", 29);
            settlementNames.Add("Həzi Aslanov q.", 30);
            settlementNames.Add("Köhnə Günəşli q.", 31);
           
            settlementNames.Add("Binə q.", 33);
            settlementNames.Add("Buzovna q.", 34);
            settlementNames.Add("Dübəndi q.", 35);
            settlementNames.Add("Gürgən q.", 36);
            settlementNames.Add("Qala q.", 37);
            settlementNames.Add("Mərdəkan q.", 38);
            settlementNames.Add("Şağan q.", 39);
            settlementNames.Add("Şimal Dres q.", 40);
            settlementNames.Add("Şüvəlan q.", 41);
            settlementNames.Add("Türkan q.", 42);
            settlementNames.Add("Zirə q.", 43);
            settlementNames.Add("Bibiheybət q.", 44);
            settlementNames.Add("Ələt q.", 45);
            settlementNames.Add("Qızıldaş q.", 46);
            settlementNames.Add("Qobustan q.", 47);
            settlementNames.Add("Lökbatan q.", 48);
            settlementNames.Add("Puta q.", 49);
            settlementNames.Add("Sahil q.", 50);
            settlementNames.Add("Səngəçal q.", 51);
            settlementNames.Add("Şıxov q.", 52);
            settlementNames.Add("Şubani q.", 53);
            settlementNames.Add("Böyükşor q.", 54);
            settlementNames.Add("1 mikrorayon q.", 55);
            settlementNames.Add("2 mikrorayon q.", 56);
            settlementNames.Add("3 mikrorayon q.", 57);
            settlementNames.Add("4 mikrorayon q.", 58);
            settlementNames.Add("5 mikrorayon q.", 59);
            settlementNames.Add("Kubinka", 60);
            
           
            settlementNames.Add("Bakıxanov q.", 63);
            settlementNames.Add("Balaxanı q.", 64);
            settlementNames.Add("Bilgəh q.", 65);
            settlementNames.Add("Kürdəxanı q.", 66);
            settlementNames.Add("Maştağa q.", 67);
            settlementNames.Add("Məmmədli q.", 68);
            settlementNames.Add("Nardaran q.", 69);
            settlementNames.Add("Pirşağı q.", 70);
            settlementNames.Add("Ramana q.", 71);
            settlementNames.Add("Sabunçu r.", 72);
            settlementNames.Add("Savalan q.", 73);
            settlementNames.Add("Yeni Balaxanı q.", 74);
            settlementNames.Add("Yeni Ramana q.", 75);
            settlementNames.Add("Zabrat q.", 76);
            settlementNames.Add("20 sahə q.", 77);
            settlementNames.Add("Badamdar q.", 78);
            settlementNames.Add("Bayıl q.", 79);
            settlementNames.Add("Bahar q.", 80);
            settlementNames.Add("Bülbülə q.", 81);
            settlementNames.Add("Dədə Qorqud q.", 82);
            settlementNames.Add("Əmircan q.", 83);
            settlementNames.Add("Günəşli q.", 84);
            settlementNames.Add("Hövsan q.", 85);
            settlementNames.Add("Qaraçuxur q.", 86);
            settlementNames.Add("Massiv A", 87);
            settlementNames.Add("Massiv B", 88);
            settlementNames.Add("Massiv D", 89);
            settlementNames.Add("Massiv G", 90);
            settlementNames.Add("Massiv V", 91);
            settlementNames.Add("Suraxanı q.", 92);
            settlementNames.Add("Şərq", 93);
            settlementNames.Add("Yeni Günəşli q.", 94);
            settlementNames.Add("Yeni Suraxanı q.", 95);
            settlementNames.Add("Zığ q.", 96);
            settlementNames.Add("Yasamal q.", 97);
            settlementNames.Add("Yeni Yasamal q.", 98);
            settlementNames.Add("Çermet q.", 99);
        }

        public Dictionary<string, int> GetSettlementsNamesAll()
        {
            return settlementNames;
        }
    }
}
