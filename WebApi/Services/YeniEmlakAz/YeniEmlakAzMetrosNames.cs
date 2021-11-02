using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services.YeniEmlakAz
{
    public class YeniEmlakAzMetrosNames
    {
        Dictionary<string, int> metrosNames = new Dictionary<string, int>();


        public YeniEmlakAzMetrosNames()
        {
            metrosNames.Add("Həzi Aslanov ",12);
            metrosNames.Add("Əhmədli",11);
            metrosNames.Add("Xalqlar Dostluğu",10);
            metrosNames.Add("Neftçilər",25);
            metrosNames.Add("Qara Qarayev",9);
            metrosNames.Add("Koroğlu",8);
            metrosNames.Add("Ulduz",7);
            metrosNames.Add("Nəriman Nərimanov",5);
            metrosNames.Add("Gənclik",4);
            metrosNames.Add("28 May",3);
            metrosNames.Add("Nizami",13);
            metrosNames.Add("Elmlər Akademiyası",14);
            metrosNames.Add("İnşaatçılar",15);
            metrosNames.Add("20 Yanvar",16);
            metrosNames.Add("Memar Əcəmi",17);
            metrosNames.Add("Nəsimi",26);
            metrosNames.Add("Azadlıq prospekti",18);
            metrosNames.Add("Cəfər Cabbarlı",20);
            metrosNames.Add(" Xətai",21);
            metrosNames.Add("Sahil",2);
            metrosNames.Add("İçəri Şəhər",1);
            metrosNames.Add("Bakmil",6);
            metrosNames.Add("Dərnəgül",19);
            metrosNames.Add("Avtovağzal",23);
            metrosNames.Add("8 Noyabr",24);
        }



        public Dictionary<string, int> GetMerosNamesAll()
        {
            return metrosNames;
        }
    }
}
