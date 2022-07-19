using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UcuzTapAz
{
    public class UcuzTapAzMarksNames
    {
        Dictionary<string, int> marksNames = new Dictionary<string, int>();

        public UcuzTapAzMarksNames()
        {
            marksNames.Add("Axundov bağı", 2);
            marksNames.Add("Ayna Sultanova heykəli", 7);
            marksNames.Add("Azadlıq meydanı", 8);
            marksNames.Add("Azneft meydanı", 12);
            marksNames.Add("Azərbaycan Dillər Universiteti", 9);
            marksNames.Add("Azərbaycan kinoteatrı", 10);
            marksNames.Add("Azərbaycan Тurizm İnstitutu", 11);
            marksNames.Add("Bakı Asiya Universiteti", 13);
            marksNames.Add("Bakı Dövlət Universiteti", 14);
            marksNames.Add("Bakı Musiqi Akademiyası", 15);
            marksNames.Add("Bakı Slavyan Universiteti", 16);
            marksNames.Add("Bayıl parkı", 17);
            marksNames.Add("Beşmərtəbə", 18);
            marksNames.Add("Botanika bağı", 19);
            marksNames.Add("Cavanşir körpüsü", 20);
            marksNames.Add("Dağüstü parkı", 21);
            marksNames.Add("Dostluq kinoteatrı", 22);
            marksNames.Add("Dövlət Statistika Komitəsi", 24);
            marksNames.Add("Dövlət İdarəçilik Akademiyası", 23);
            marksNames.Add("Fontanlar bağı", 25);
            marksNames.Add("Hüseyn Cavid parkı", 26);
            marksNames.Add("Keşlə bazarı", 33);
            marksNames.Add("Koala parkı", 34);
            marksNames.Add("M.Hüseynzadə parkı", 38);
            marksNames.Add("M.Ə.Sabir parkı", 37);
            marksNames.Add("Molokan bağı", 39);
            marksNames.Add("Milli Konservatoriya", 42);
            marksNames.Add("Mərkəzi Univermaq", 41);
            marksNames.Add("Neapol dairəsi", 44);
            marksNames.Add("Neft Akademiyası", 45);
            marksNames.Add("Neftçi bazası", 46);
            marksNames.Add("Nizami kinoteatrı", 50);
            marksNames.Add("Nəriman Nərimanov parkı", 47);
            marksNames.Add("Nərimanov heykəli", 48);
            marksNames.Add("Nəsimi bazarı", 49);
            marksNames.Add("Park Zorge", 51);
            marksNames.Add("Pedaqoji Universiteti", 52);
            marksNames.Add("Prezident parkı", 54);
            marksNames.Add("Qubernator parkı", 36);
            marksNames.Add("Qış parkı", 35);
            marksNames.Add("Respublika stadionu", 55);
            marksNames.Add("Rusiya səfirliyi", 57);
            marksNames.Add("Rəssamlıq Akademiyası", 56);
            marksNames.Add("Sahil bağı", 58);
            marksNames.Add("Sevil Qazıyeva parkı", 60);
            marksNames.Add("Sirk", 62);
            marksNames.Add("Sovetski", 63);
            marksNames.Add("Space TV", 64);
            marksNames.Add("Səməd Vurğun parkı", 61);
            marksNames.Add("Texniki Universiteti", 69);
            marksNames.Add("Tibb Universiteti", 71);
            marksNames.Add("Təhsil Nazirliyi", 70);
            marksNames.Add("Ukrayna dairəsi", 73);
            marksNames.Add("Yasamal bazarı", 74);
            marksNames.Add("Zabitlər parkı", 75);
            marksNames.Add("Zoopark", 77);
            marksNames.Add("Zərifə Əliyeva adına park", 76);
            marksNames.Add("İncəsənət və Mədəniyyət Un.", 31);
            marksNames.Add("İqtisadiyyat Universiteti", 30);
            marksNames.Add("İzmir parkı", 32);
            marksNames.Add("İçəri Şəhər", 28);
            marksNames.Add("Şəfa stadionu", 65);
            marksNames.Add("Şəhidlər xiyabanı", 66);
            marksNames.Add("Şəlalə parkı", 67);
            marksNames.Add("Şərq Bazarı", 68);
            marksNames.Add("Port Baku", 53);

        }

        public Dictionary<string, int> GetMarksNamesAll()
        {
            return marksNames;
        }
    }
}
