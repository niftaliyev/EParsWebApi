using System.Collections.Generic;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzSettlementNames
    {
        Dictionary<string, int> settlementNames = new Dictionary<string, int>();

        public YeniEmlakAzSettlementNames()
        {
            settlementNames.Add("28 may qəs.",18);
            settlementNames.Add("6-cı mikrorayon",19);
            settlementNames.Add("7-ci mikrorayon",20);
            settlementNames.Add("8-ci mikrorayon",21);
            settlementNames.Add("9-cu mikrorayon",22);
            settlementNames.Add("Biləcəri qəs.",23);
            settlementNames.Add("Binəqədi qəs.",24);
            settlementNames.Add("Çiçək qəs.",2);
            settlementNames.Add("Rəsulzadə qəs.",100);
            settlementNames.Add("Sulutəpə qəs.",28);
            settlementNames.Add("Xocəsən qəs.",25);
            settlementNames.Add("Dərnəgül",101);
            settlementNames.Add("8-ci kilometr",61);
            settlementNames.Add("Keşlə qəs.",8);
            settlementNames.Add("UPD",181);
            settlementNames.Add("Böyükşor qəs.",54);
            settlementNames.Add("Montin qəs.",109);
            settlementNames.Add("1-ci mikrorayon", 55);
            settlementNames.Add("2-ci mikrorayon", 56);
            settlementNames.Add("3-cü mikrorayon", 57);
            settlementNames.Add("4-cü mikrorayon", 58);
            settlementNames.Add("5-ci mikrorayon", 59);
            settlementNames.Add("Kubinka", 60);
            settlementNames.Add("Ceyildağ qəs.", 128);
            settlementNames.Add("Ələt qəs.", 45);
            settlementNames.Add("Heybət qəs.", 44);
            settlementNames.Add("Lökbatan qəs.", 48);
            settlementNames.Add("Müşfiqabad qəs.", 22);
            settlementNames.Add("Pirsaat qəs.", 22);
            settlementNames.Add("Puta qəs.", 49);
            settlementNames.Add("Qaradağ qəs.", 179);
            settlementNames.Add("Qızıldaş qəs.", 46);
            settlementNames.Add("Qobustan qəs.", 47);
            settlementNames.Add("Sahil qəs.", 50);
            settlementNames.Add("Səngəçal qəs.", 51);
            settlementNames.Add("Şubani qəs.", 53);
            settlementNames.Add("Ümid qəs.", 116);
            settlementNames.Add("Bakıxanov qəs.", 63);
            settlementNames.Add("Balaxanı qəs.", 64);
            settlementNames.Add("Bilgəh qəs.", 65);
            settlementNames.Add("Kürdəxanı qəs.", 66);
            settlementNames.Add("Məştağa qəs.", 67);
            settlementNames.Add("Nardaran qəs.", 69);
            settlementNames.Add("Pirşağı qəs.", 70);
            settlementNames.Add("Ramana qəs.", 71);
            settlementNames.Add("Sabunçu qəs.", 72);
            settlementNames.Add("Savalan qəs.", 73);
            settlementNames.Add("Yeni Ramana qəs.", 75);
            settlementNames.Add("Zabrat qəs.", 76);
            settlementNames.Add("Albalı qəs.", 168);
            settlementNames.Add("Bahar qəs.", 80);
            settlementNames.Add("Bülbülə qəs.", 81);
            settlementNames.Add("Dədə Qorqud qəs.", 82);
            settlementNames.Add("Əmircan qəs.", 83);
            settlementNames.Add("Hövsan qəs.", 85);
            settlementNames.Add("Köhnə günəşli qəs.", 31);
            settlementNames.Add("Qaraçuxur qəs.", 86);
            settlementNames.Add("Suraxanı qəs.", 92);
            settlementNames.Add("Yeni Günəşli qəs.", 94);
            settlementNames.Add("Yeni Suraxanı qəs.", 95);
            settlementNames.Add("Zığ qəs.", 96);
            settlementNames.Add("20-ci sahə", 77);
            settlementNames.Add("Badamdar qəs.", 78);
            settlementNames.Add("Bayıl qəs.", 79);
            settlementNames.Add("Bibi Heybət qəs.", 44);
            //settlementNames.Add("İçəri şəhər", 22);
            settlementNames.Add("NZS", 32);
            settlementNames.Add("Əhmədli", 29);
            settlementNames.Add("H.Aslanon qəs", 30);
            settlementNames.Add("Ağ şəhər", 103);
            settlementNames.Add("Binə qəs.", 33);
            settlementNames.Add("Buzovna", 34);
            settlementNames.Add("Dübəndi", 35);
            settlementNames.Add("Mərdəkan", 38);
            settlementNames.Add("Qala", 37);
            settlementNames.Add("Şağan", 39);
            settlementNames.Add("Şimal qres", 40);
            settlementNames.Add("Şüvəlan", 41);
            settlementNames.Add("Türkan", 42);
            settlementNames.Add("Zaqulba", 16);
            settlementNames.Add("Zirə", 43);
            settlementNames.Add("Yasamal", 97);
            settlementNames.Add("Yeni Yasamal", 98);
            settlementNames.Add("Gürgən", 36);
            settlementNames.Add("Pirallahı", 178);
            //Abseron

            settlementNames.Add("Aşağı Güzdək", 170);
            settlementNames.Add("Fatmayı", 4);
            settlementNames.Add("Goradil", 5);
            settlementNames.Add("Güzdək", 108);
            settlementNames.Add("Hökməli", 6);
            settlementNames.Add("Masazır", 9);
            settlementNames.Add("Mehdiabad", 10);
            settlementNames.Add("Novxanı", 12);
            settlementNames.Add("Qobu", 8);
            settlementNames.Add("Saray", 14);
            settlementNames.Add("Məhəmmədli", 178);
            settlementNames.Add("Ceyranbatan qəs.", 1);
            settlementNames.Add("Digah", 3);
            settlementNames.Add("Qurtuluş 93", 180);
            settlementNames.Add("Gencler seherciyi", 178);
           // settlementNames.Add("Yeni Baki", 178);
            settlementNames.Add("Atyalı", 106);
           // settlementNames.Add("Pirəküşkül", 178);
            settlementNames.Add("Nübar qəsəbəsi", 111);


        }

        public Dictionary<string, int> GetSettlementNamesAll()
        {
            return settlementNames;
        }
    }
}
