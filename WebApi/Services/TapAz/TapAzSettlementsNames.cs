using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.TapAz
{
    public class TapAzSettlementsNames
    {
        Dictionary<string, int> settlementNames = new Dictionary<string, int>();


        public TapAzSettlementsNames()
        {
            settlementNames.Add("Ceyranbatan qəs.",1);
            settlementNames.Add("Çiçək qəs.",2);
            settlementNames.Add("Digah qəs.",3);
            settlementNames.Add("Fatmayı qəs.", 4);
            settlementNames.Add("Görədil qəs.", 5);
            settlementNames.Add("Hökməli qəs.", 6);
            settlementNames.Add("Corat qəs.", 7);
            settlementNames.Add("Qobu qəs.", 8);
            settlementNames.Add("Masazır qəs.", 9);
            settlementNames.Add("Mehdiabad qəs.", 10);
            settlementNames.Add("Müşviqabad qəs.",11);
            settlementNames.Add("Novxanı qəs.", 12);
            settlementNames.Add("Pirəkəşkül qəs.", 13);
            settlementNames.Add("Saray qəs.", 14);
            settlementNames.Add("Yeni Corat qəs.", 15);
            settlementNames.Add("Zagulba qəs.", 16);
            settlementNames.Add("Alatava 2 qəs.", 17);
            settlementNames.Add("28 May", 18);
            settlementNames.Add("6-cı mkr.", 19);
            settlementNames.Add("7-ci mkr.", 20);
            settlementNames.Add("8-ci mkr.", 21);
            settlementNames.Add("9-cu mkr.", 22);
            settlementNames.Add("9 Mkr.", 22);
            settlementNames.Add("Biləcəri qəs.", 23);
            settlementNames.Add("Binəqədi qəs.", 24);
            settlementNames.Add("Xocəsən qəs.", 25);
            settlementNames.Add("Xutor qəs.", 26);
            settlementNames.Add("M.Ə.Rəsulzadə qəs.", 27);
            settlementNames.Add("Sulutəpə qəs.", 28);
            settlementNames.Add("Əhmədli qəs.", 29);
            settlementNames.Add("Həzi Aslanov qəs.", 30);
            settlementNames.Add("Köhnə Günəşli qəs.", 31);
            settlementNames.Add("NZS qəs.", 32);
            settlementNames.Add("Binə qəs.", 33);
            settlementNames.Add("Buzovna qəs.", 34);
            settlementNames.Add("Dübəndi qəs.", 35);
            settlementNames.Add("Gürgən qəs.", 36);
            settlementNames.Add("Qala qəs.", 37);
            settlementNames.Add("Mərdəkan qəs.", 38);
            settlementNames.Add("Şağan qəs.", 39);
            settlementNames.Add("Şimal Dres qəs.", 40);
            settlementNames.Add("Şüvəlan qəs.", 41);
            settlementNames.Add("Türkan qəs.", 42);
            settlementNames.Add("Zirə qəs.", 43);
            settlementNames.Add("Bibi Heybət", 44);
            settlementNames.Add("Ələt qəs.", 45);
            settlementNames.Add("Qızıldaş qəs.", 46);
            settlementNames.Add("Qobustan qəs.", 47);
            settlementNames.Add("Lökbatan qəs.", 48);
            settlementNames.Add("Puta qəs.", 49);
            settlementNames.Add("Sahil qəs.", 50);
            settlementNames.Add("Səngəçal qəs.", 51);
            settlementNames.Add("Şıxov qəs.", 52);
            settlementNames.Add("Şubani qəs.", 53);
            settlementNames.Add("Böyükşor qəs.", 54);
            settlementNames.Add("1-ci mkr.", 55);
            settlementNames.Add("2-ci mkr.", 56);
            settlementNames.Add("3-ci mkr.", 57);
            settlementNames.Add("4-ci mkr.", 58);
            settlementNames.Add("5-ci mkr.", 59);
            settlementNames.Add("Kubinka", 60);
            settlementNames.Add("8-ci kilometr", 61);
            settlementNames.Add("Keşlə qəs.", 62);
            settlementNames.Add("Bakıxanov qəs.", 63);
            settlementNames.Add("Balaxanı qəs.", 64);
            settlementNames.Add("Bilgəh qəs.", 65);
            settlementNames.Add("Kürdəxanı qəs.", 66);
            settlementNames.Add("Maştağa qəs.", 67);
            settlementNames.Add("Məmmədli qəs.", 68);
            settlementNames.Add("Nardaran qəs.", 69);
            settlementNames.Add("Pirşağı qəs.", 70);
            settlementNames.Add("Ramana qəs.", 71);
            settlementNames.Add("Sabunçu r.", 72);
            settlementNames.Add("Savalan qəs.", 73);
            settlementNames.Add("Yeni Balaxanı qəs.", 74);
            settlementNames.Add("Yeni Ramana qəs.", 75);
            settlementNames.Add("Zabrat qəs.", 76);
            settlementNames.Add("20-ci sahə qəs.", 77);
            settlementNames.Add("Badamdar qəs.", 78);
            settlementNames.Add("Bayıl qəs.", 79);
            settlementNames.Add("Bahar qəs.", 80);
            settlementNames.Add("Bülbülə qəs.", 81);
            settlementNames.Add("Dədə Qorqud qəs.", 82);
            settlementNames.Add("Əmircan qəs.", 83);
            settlementNames.Add("Günəşli qəs.", 84);
            settlementNames.Add("Hövsan qəs.", 85);
            settlementNames.Add("Qaraçuxur qəs.", 86);
            settlementNames.Add("Massiv A", 87);
            settlementNames.Add("Massiv B", 88);
            settlementNames.Add("Massiv D", 89);
            settlementNames.Add("Massiv G", 90);
            settlementNames.Add("Massiv V", 91);
            settlementNames.Add("Suraxanı qəs.", 92);
            settlementNames.Add("Şərq", 93);
            settlementNames.Add("Yeni Günəşli qəs.", 94);
            settlementNames.Add("Yeni Suraxanı qəs.", 95);
            settlementNames.Add("Zığ qəs.", 96);
            settlementNames.Add("Yasamal qəs.", 97);
            settlementNames.Add("Yeni Yasamal qəs.", 98);
            settlementNames.Add("Çermet qəs.", 99);
        }

        public Dictionary<string, int> GetSettlementsNamesAll()
        {
            return settlementNames;
        }
    }
}
