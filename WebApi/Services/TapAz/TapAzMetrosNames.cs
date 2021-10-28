using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.TapAz
{
    public class TapAzMetrosNames
    {

        Dictionary<string, string> metrosNames = new Dictionary<string,string>();

        public TapAzMetrosNames()
        {
            metrosNames.Add("H. Aslanov m.", "Həzi Aslanov");
            metrosNames.Add("Əhmədli m.", "Əhmədli");
            metrosNames.Add("Xalqlar Dostluğu m.", "Xalqlar Dostluğu");
            metrosNames.Add("Neftçilər m.", "Neftçilər");
            metrosNames.Add("Qara Qarayev m.", "Qara Qarayev");
            metrosNames.Add("Koroğlu m.", "Koroğlu");
            metrosNames.Add("Ulduz m.", "Ulduz");
            metrosNames.Add("Bakmil m.", "Bakmil");
            metrosNames.Add("Nərimanov m.", "Nəriman Nərimanov");
            metrosNames.Add("Gənclik m.", "Gənclik");
            metrosNames.Add("28 may m.", "28 May");
            metrosNames.Add("Şah İsmayıl Xətai m.", "Şah İsmail Xətai");
            metrosNames.Add("Sahil m.", "Sahil");
            metrosNames.Add("İçəri-şəhər m.", "İçərişəhər");
            metrosNames.Add("Nizami m.", "Nizami");
            metrosNames.Add("E. Akademiyası m.", "Elmlər Akademiyası");
            metrosNames.Add("İnşaatçılar m.", "İnşaatçılar");
            metrosNames.Add("20 Yanvar m.", "20 Yanvar");
            metrosNames.Add("Memar Əcəmi m.", "Memar Əcəmi");
            metrosNames.Add("Avtovağzal m.", "Avtovağzal");
            metrosNames.Add("8 Noyabr m.", "8 Noyabr");
            metrosNames.Add("Nəsimi m.", "Nəsimi");
            metrosNames.Add("Azadlıq m.", "Azadlıq prospekti");
            metrosNames.Add("Dərnəgül m.", "Dərnəgül");
        }
        public string MetroName(string metroName)
        {           
            return metrosNames[metroName];
        }

    }
}
