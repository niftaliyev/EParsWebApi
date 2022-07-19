using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.UnvanAz
{
    public class UnvanAzMetroNames
    {
        Dictionary<string, int> metrosNames = new Dictionary<string, int>();

        public UnvanAzMetroNames()
        {
            metrosNames.Add("Həzi Aslanov metrosu", 12);
            metrosNames.Add("Əhmədli metrosu", 11);
            metrosNames.Add("Xalqlar Dostluğu metrosu", 10);
            metrosNames.Add("Neftçilər metrosu", 25);
            metrosNames.Add("Qara Qarayev metrosu", 9);
            metrosNames.Add("Koroğlu metrosu", 8);
            metrosNames.Add("Ulduz metrosu", 7);
            metrosNames.Add("Bakmil metrosu", 6);
            metrosNames.Add("Nəriman Nərimanov metrosu", 5);
            metrosNames.Add("Gənclik metrosu", 4);
            metrosNames.Add("28 May metrosu", 3);
            metrosNames.Add("Xətai metrosu", 21);
            metrosNames.Add("Sahil metrosu", 2);
            metrosNames.Add("İçəri Şəhər metrosu", 1);
            metrosNames.Add("Nizami metrosu", 13);
            metrosNames.Add("Elmlər Akademiyası metrosu", 14);
            metrosNames.Add("İnşaatçılar metrosu", 15);
            metrosNames.Add("20 Yanvar metrosu", 16);
            metrosNames.Add("Memar Əcəmi metrosu", 17);
            metrosNames.Add("Avtovağzal metrosu", 23);
            metrosNames.Add("8 Noyabr metrosu", 24);
            metrosNames.Add("Nəsimi metrosu", 26);
            metrosNames.Add("Azadlıq Prospekti metrosu", 18);
            metrosNames.Add("Dərnəgül metrosu", 19);
            metrosNames.Add("Cəfər Cabbarlı metrosu", 20);

        }
        public Dictionary<string, int> GetMetroNameAll()
        {
            return metrosNames;
        }

    }
}
