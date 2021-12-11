using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.EmlakciAz
{
    public class EmlakciAzCountryNames
    {
        Dictionary<string, int> countryNames = new Dictionary<string, int>();

        public EmlakciAzCountryNames()
        {
            countryNames.Add("Bakı", 1);
            countryNames.Add("Abşeron", 75);
            countryNames.Add("Xırdalan", 26);
            countryNames.Add("Sumqayıt", 55);
            countryNames.Add("Gəncə", 18);
            countryNames.Add("Ağdaş", 4);
            countryNames.Add("Ağstafa", 6);
            countryNames.Add("Ağsu", 7);
            countryNames.Add("Astara ", 8);
            countryNames.Add("Balakən ", 9);
            countryNames.Add("Bərdə", 11);
            countryNames.Add("Beyləqan", 10);
            countryNames.Add("Biləsuvar", 12);
            countryNames.Add("Cəbrayıl", 13);
            countryNames.Add("Cəlilabad", 14);
            countryNames.Add("Daşkəsən", 15);
            countryNames.Add("Dəliməmmədli", 74);
            countryNames.Add("Füzuli", 16);
            countryNames.Add("Gədəbəy", 17);
            countryNames.Add("Goranboy ", 19);
            countryNames.Add("Göyçay", 20);
            countryNames.Add("Göygöl", 21);
            countryNames.Add("Göytəpə", 22);
            countryNames.Add("Hacıqabul", 23);
            countryNames.Add("Horadiz", 73);
            countryNames.Add("İmişli ", 31);
            countryNames.Add("İsmayıllı", 32);
            countryNames.Add("Kəlbəcər", 33);
            countryNames.Add("Kürdəmir", 34);
            countryNames.Add("Laçın", 41);
            countryNames.Add("Lənkəran", 43);
            countryNames.Add("Lerik", 42);
            countryNames.Add("Liman", 76);
            countryNames.Add("Masallı", 44);
            countryNames.Add("Mingəçevir", 45);
            countryNames.Add("Naftalan", 46);
            countryNames.Add("Naxçıvan", 47);
            countryNames.Add("Neftçala", 48);
            countryNames.Add("Oğuz", 49);
            countryNames.Add("Qax", 35);
            countryNames.Add("Qazax", 36);
            countryNames.Add("Qəbələ", 37);
            countryNames.Add("Qobustan", 72);
            countryNames.Add("Quba", 38);
            countryNames.Add("Qubadlı", 39);
            countryNames.Add("Qusar", 40);
            countryNames.Add("Saatlı", 50);
            countryNames.Add("Sabirabad", 51);
            countryNames.Add("Şabran", 56);
            countryNames.Add("Şamaxı", 57);
            countryNames.Add("Samux", 53);
            countryNames.Add("Şəki", 58);
            countryNames.Add("Salyan", 52);
            countryNames.Add("Şəmkir", 59);
            countryNames.Add("Şirvan", 60);
            countryNames.Add("Siyəzən", 54);
            countryNames.Add("Şuşa", 61);
            countryNames.Add("Tərtər", 62);
            countryNames.Add("Tovuz", 63);
            countryNames.Add("Ucar", 64);
            countryNames.Add("Xaçmaz", 24);
            countryNames.Add("Xudat", 30);
            countryNames.Add("Xaçmaz / Nabran", 71);
            countryNames.Add("Xızı", 27);
            countryNames.Add("Xocavənd", 29);
            countryNames.Add("Ağcəbədi", 2);
            countryNames.Add("Yardımlı", 65);
            countryNames.Add("Yevlax", 66);
            countryNames.Add("Zaqatala", 67);
            countryNames.Add("Zəngilan", 68);
            countryNames.Add("Zərdab", 69);
            countryNames.Add("Ağdam", 3);

            countryNames.Add("Ağcabədi", 2);

        }

        public Dictionary<string, int> GetCountryNames()
        {
            return countryNames;
        }
    }
}
