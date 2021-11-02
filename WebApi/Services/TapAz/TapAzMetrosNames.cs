using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.TapAz
{
    public class TapAzMetrosNames
    {

        Dictionary<string, int> metrosNames = new Dictionary<string,int>();

        public TapAzMetrosNames()
        {
            metrosNames.Add("H. Aslanov m.", 12);
            metrosNames.Add("Əhmədli m.", 11);
            metrosNames.Add("Xalqlar Dostluğu m.", 10);
            metrosNames.Add("Neftçilər m.", 25);
            metrosNames.Add("Qara Qarayev m.", 9);
            metrosNames.Add("Koroğlu m.", 8);
            metrosNames.Add("Ulduz m.", 7);
            metrosNames.Add("Bakmil m.", 6);
            metrosNames.Add("Nərimanov m.", 5);
            metrosNames.Add("Gənclik m.", 4);
            metrosNames.Add("28 may m.", 3);
            metrosNames.Add("Şah İsmayıl Xətai m.", 21);
            metrosNames.Add("Sahil m.", 2);
            metrosNames.Add("İçəri-şəhər m.", 1);
            metrosNames.Add("Nizami m.", 13);
            metrosNames.Add("E. Akademiyası m.", 14);
            metrosNames.Add("İnşaatçılar m.", 15);
            metrosNames.Add("20 Yanvar m.", 16);
            metrosNames.Add("Memar Əcəmi m.", 17);
            metrosNames.Add("Avtovağzal m.", 23);
            metrosNames.Add("8 Noyabr m.", 24);
            metrosNames.Add("Nəsimi m.", 26);
            metrosNames.Add("Azadlıq m.", 18);
            metrosNames.Add("Dərnəgül m.", 19);
        }
        public Dictionary<string, int> GetMetroNameAll()
        {
            return metrosNames;
        }

    }
}
