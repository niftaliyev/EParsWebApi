using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UnvanAz
{
    public class UnvanAzSettlementNames
    {
        Dictionary<string, int> settlementNames = new Dictionary<string, int>();


        public UnvanAzSettlementNames()
        {
            settlementNames.Add("Ceyranbatan qəsəbəsi", 1);
            settlementNames.Add("Çiçək qəsəbəsi", 2);
            settlementNames.Add("Digah qəsəbəsi", 3);
            settlementNames.Add("Fatmayı qəsəbəsi", 4);
            settlementNames.Add("Görədil qəsəbəsi", 5);
            settlementNames.Add("Hökməli qəsəbəsi", 6);
            settlementNames.Add("Corat qəsəbəsi", 7);
            settlementNames.Add("Qobu qəsəbəsi", 8);
            settlementNames.Add("Masazır qəsəbəsi", 9);
            settlementNames.Add("Mehdiabad qəsəbəsi", 10);
            settlementNames.Add("Müşviqabad qəsəbəsi", 11);
            settlementNames.Add("Novxanı qəsəbəsi", 12);
            settlementNames.Add("Pirəkəşkül qəsəbəsi", 13);
            settlementNames.Add("Saray qəsəbəsi", 14);
            settlementNames.Add("Yeni Corat qəsəbəsi", 15);
            settlementNames.Add("Zagulba qəsəbəsi", 16);
            settlementNames.Add("2-ci Alatava qəsəbəsi", 17);
            settlementNames.Add("28 May", 18);
            settlementNames.Add("6-cı mikrorayon qəsəbəsi", 19);
            settlementNames.Add("7-ci mikrorayon qəsəbəsi", 20);
            settlementNames.Add("8-ci mikrorayon qəsəbəsi", 21);
            settlementNames.Add("9-cu mikrorayon qəsəbəsi", 22);
            settlementNames.Add("9 Mkr.", 22);
            settlementNames.Add("Biləcəri qəsəbəsi", 23);
            settlementNames.Add("Binəqədi qəsəbəsi", 24);
            settlementNames.Add("Xocəsən qəsəbəsi", 25);

            settlementNames.Add("Xutor qəsəbəsi", 26);
            settlementNames.Add("M.Ə.Rəsulzadə qəsəbəsi", 27);
            settlementNames.Add("Sulutəpə qəsəbəsi", 28);
            settlementNames.Add("Əhmədli qəsəbəsi", 29);
            settlementNames.Add("Həzi Aslanov qəsəbəsi", 30);
            settlementNames.Add("Köhnə Günəşli qəsəbəsi", 31);
            settlementNames.Add("NZS qəsəbəsi", 32);
            settlementNames.Add("Binə qəsəbəsi", 33);
            settlementNames.Add("Buzovna qəsəbəsi", 34);
            settlementNames.Add("Dübəndi qəsəbəsi", 35);
            settlementNames.Add("Gürgən qəsəbəsi", 36);
            settlementNames.Add("Qala qəsəbəsi", 37);
            settlementNames.Add("Mərdəkan qəsəbəsi", 38);
            settlementNames.Add("Şağan qəsəbəsi", 39);
            settlementNames.Add("Şimal DRES qəsəbəsi", 40);
            settlementNames.Add("Şüvəlan qəsəbəsi", 41);
            settlementNames.Add("Türkan qəsəbəsi", 42);
            settlementNames.Add("Zirə qəsəbəsi", 43);
            settlementNames.Add("Bibi Heybət qəsəbəsi", 44);
            settlementNames.Add("Ələt qəsəbəsi", 45);
            settlementNames.Add("Qızıldaş qəsəbəsi", 46);
            settlementNames.Add("Qobustan qəsəbəsi", 47);
            settlementNames.Add("Lökbatan qəsəbəsi", 48);
            settlementNames.Add("Puta qəsəbəsi", 49);
            settlementNames.Add("Sahil qəsəbəsi", 50);
            settlementNames.Add("Səngəçal qəsəbəsi", 51);
            settlementNames.Add("Şıxov qəsəbəsi", 52);
            settlementNames.Add("Şubani qəsəbəsi", 53);
            settlementNames.Add("Böyükşor qəsəbəsi", 54);
            settlementNames.Add("1-ci mikrorayon qəsəbəsi", 55);
            settlementNames.Add("2-ci mikrorayon qəsəbəsi", 56);
            settlementNames.Add("3-ci mikrorayon qəsəbəsi", 57);
            settlementNames.Add("4-ci mikrorayon qəsəbəsi", 58);
            settlementNames.Add("5-ci mikrorayon qəsəbəsi", 59);
            settlementNames.Add("Kubinka", 60);
            settlementNames.Add("8-ci kilometr qəsəbəsi", 61);
            settlementNames.Add("Keşlə qəsəbəsi", 62);
            settlementNames.Add("Bakıxanov qəsəbəsi", 63);
            settlementNames.Add("Balaxanı qəsəbəsi", 64);
            settlementNames.Add("Bilgəh qəsəbəsi", 65);
            settlementNames.Add("Kürdəxanı qəsəbəsi", 66);
            settlementNames.Add("Maştağa qəsəbəsi", 67);
            settlementNames.Add("Məmmədli qəsəbəsi", 68);
            settlementNames.Add("Nardaran qəsəbəsi", 69);
            settlementNames.Add("Pirşağı qəsəbəsi", 70);
            settlementNames.Add("Ramana qəsəbəsi", 71);
            settlementNames.Add("Sabunçu r.", 72);
            settlementNames.Add("Savalan qəsəbəsi", 73);
            settlementNames.Add("Yeni Balaxanı qəsəbəsi", 74);
            settlementNames.Add("Yeni Ramana qəsəbəsi", 75);
            settlementNames.Add("Zabrat qəsəbəsi", 76);
            settlementNames.Add("20-ci sahə qəsəbəsi", 77);
            settlementNames.Add("Badamdar qəsəbəsi", 78);
            settlementNames.Add("Bayıl qəsəbəsi", 79);
            settlementNames.Add("Bahar", 80);
            settlementNames.Add("Bülbülə qəsəbəsi", 81);
            settlementNames.Add("Dədə Qorqud", 82);
            settlementNames.Add("Əmircan qəsəbəsi", 83);
            settlementNames.Add("Günəşli qəsəbəsi", 84);
            settlementNames.Add("Hövsan qəsəbəsi", 85);
            settlementNames.Add("Qaraçuxur qəsəbəsi", 86);
            settlementNames.Add("Massiv A", 87);
            settlementNames.Add("Massiv B", 88);
            settlementNames.Add("Massiv D", 89);
            settlementNames.Add("Massiv G", 90);
            settlementNames.Add("Massiv V", 91);
            settlementNames.Add("Suraxanı qəsəbəsi", 92);
            settlementNames.Add("Şərq", 93);
            settlementNames.Add("Yeni Günəşli qəsəbəsi", 94);
            settlementNames.Add("Yeni Suraxanı qəsəbəsi", 95);
            settlementNames.Add("Zığ qəsəbəsi", 96);
            settlementNames.Add("Yasamal qəsəbəsi", 97);
            settlementNames.Add("Yeni Yasamal qəsəbəsi", 98);
            settlementNames.Add("Çermet qəsəbəsi", 99);
        }

        public Dictionary<string, int> GetSettlementsNamesAll()
        {
            return settlementNames;
        }
    }
}

