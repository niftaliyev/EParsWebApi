using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzSettlementNames
    {
        Dictionary<string, int> settlementsNames = new Dictionary<string, int>();

        public EmlakciAzSettlementNames()
        {
            settlementsNames.Add("1 mkr", 55);
            settlementsNames.Add("2 mkr", 56);
            settlementsNames.Add("3 mkr", 57);
            settlementsNames.Add("4 mkr", 58);
            settlementsNames.Add("5 mkr", 59);
            settlementsNames.Add("6 mkr", 19);
            settlementsNames.Add("7 mkr", 20);
            settlementsNames.Add("8 mkr", 21);
            settlementsNames.Add("9 mkr", 22);
            settlementsNames.Add("8 km", 61);
            settlementsNames.Add("28 may", 18);
            settlementsNames.Add("20-ci sahə", 77);
            settlementsNames.Add("Alatava 2", 17);
            settlementsNames.Add("Badamdar", 78);
            settlementsNames.Add("Bahar", 80);
            settlementsNames.Add("Bakıxanov", 63);
            settlementsNames.Add("Balaxanı", 64);
            settlementsNames.Add("Bayıl", 79);
            settlementsNames.Add("Bibiheybət", 44);
            settlementsNames.Add("Biləcəri", 23);
            settlementsNames.Add("Bilgəh", 65);
            settlementsNames.Add("Binə", 33);
            settlementsNames.Add("Binəqədi", 24);
            settlementsNames.Add("Böyükşor", 54);
            settlementsNames.Add("Bülbülə", 81);
            settlementsNames.Add("Buzovna", 34);
            settlementsNames.Add("Ceyranbatan", 1);
            settlementsNames.Add("Çiçək", 2);
            settlementsNames.Add("Dədə Qorqud", 82);
            settlementsNames.Add("Digah", 3);
            settlementsNames.Add("Dübəndi bağları", 35);
            settlementsNames.Add("Əhmədli", 29);
            settlementsNames.Add("Ələt q", 45);
            settlementsNames.Add("Əmircan", 83);
            settlementsNames.Add("Fatmayı", 4);
            settlementsNames.Add("Görədil", 5);
            settlementsNames.Add("Həzi Aslanov", 30);
            settlementsNames.Add("Hökməli", 6);
            settlementsNames.Add("Hövsan", 85);
            settlementsNames.Add("Keşlə", 62);
            settlementsNames.Add("K.Günəşli", 31);
            settlementsNames.Add("Kürdəxanı", 66);
            settlementsNames.Add("Lökbatan", 48);
            settlementsNames.Add("Rəsulzadə", 27);
            settlementsNames.Add("Masazır", 9);
            settlementsNames.Add("Maştağa", 67);
            settlementsNames.Add("Mehdiabad", 10);
            settlementsNames.Add("Məhəmmədli", 68);
            settlementsNames.Add("Mərdəkan", 38);
            settlementsNames.Add("Müşfiqabad", 11);
            settlementsNames.Add("Nardaran", 69);
            settlementsNames.Add("Novxanı", 12);
            settlementsNames.Add("NZS", 32);
            settlementsNames.Add("Perekeşkül", 13);
            settlementsNames.Add("Pirşağı", 70);
            settlementsNames.Add("Puta", 49);
            settlementsNames.Add("Qala", 37);
            settlementsNames.Add("Qaraçuxur", 86);
            settlementsNames.Add("Qızıldaş", 46);
            settlementsNames.Add("Qobu", 8);
            settlementsNames.Add("Qobustan", 47);
            settlementsNames.Add("Ramana", 71);
            settlementsNames.Add("Şağan", 39);
            settlementsNames.Add("Sahil", 50);
            settlementsNames.Add("Səngəçal", 51);
            settlementsNames.Add("Saray", 14);
            settlementsNames.Add("Savalan", 73);
            settlementsNames.Add("Sulutəpə", 28);
            settlementsNames.Add("Şıxov", 52);
            settlementsNames.Add("Şubanı", 53);
            settlementsNames.Add("Şüvəlan", 41);
            settlementsNames.Add("Türkan", 42);
            settlementsNames.Add("Xocəsən", 25);
            settlementsNames.Add("Y.Günəşli", 94);
            settlementsNames.Add("Y.Ramana", 75);
            settlementsNames.Add("Y.Suraxanı", 95);
            settlementsNames.Add("Yeni Yasamal", 98);
            settlementsNames.Add("Zabrat 1", 10);
            settlementsNames.Add("Zabrat 2", 10);
            settlementsNames.Add("Zağulba", 16);
            settlementsNames.Add("Zığ", 96);
            settlementsNames.Add("Zirə", 43);
            settlementsNames.Add("Çermet", 99);
            settlementsNames.Add("Sabunçu", 72);


            settlementsNames.Add("Ağ şəhər",103 );
            settlementsNames.Add("Alatava 1",104 );
            settlementsNames.Add("Albalı",105 );
            settlementsNames.Add("Atyalı",106 );
            settlementsNames.Add("Corat", 107);
            settlementsNames.Add("Güzdək",108 );
            settlementsNames.Add("Montin",109 );
            settlementsNames.Add("Nasosnu",110 );
            settlementsNames.Add("Nübar", 111);
            settlementsNames.Add("Papanin",112 );
            settlementsNames.Add("Pirallahı", 113);
            settlementsNames.Add("Qara şəhər",114 );
            settlementsNames.Add("Şuşa", 115);
            settlementsNames.Add("Ümid", 116);
            settlementsNames.Add("Xaşaxuna",117 );
            settlementsNames.Add("Xutor",118 );
            settlementsNames.Add("Y.Yasamal", 119);
            settlementsNames.Add("Zuğulba",120 );
            settlementsNames.Add("Xırdalan", 26 );
        }
        public Dictionary<string, int> GetSettlementsNamesAll()
        {
            return settlementsNames;
        }
    }
}
