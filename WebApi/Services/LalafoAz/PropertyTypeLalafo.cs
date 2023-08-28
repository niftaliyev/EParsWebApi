using System;

namespace WebApi.Services.LalafoAz
{
    public class PropertyTypeLalafo
    {
        public int GetTitleOfProperty(string type)
        {

            if (type.Contains("Yeni tikili"))
                return 1;
            else if (type.Contains("Köhnə tikili"))
                return 2;
            else if (type.Contains("Həyət evləri və villalar"))
                return 5;
            else if (type.Contains("Bağ evləri"))
                return 4;
            else if (type.Contains("Kommersiya daşınmaz əmlakı"))
                return 7;
            else if (type.Contains("Torpaq"))
                return 9;
            else if (type.Contains("Qarajlar"))
                return 10;

            return 0;


        }

        public int GetTypeOfProperty(string type)
        {

            if (type.Contains("Kirayə",StringComparison.OrdinalIgnoreCase))
                return 1;
            else if (type.Contains("Satış", StringComparison.OrdinalIgnoreCase))
                return 2;


            return 0;
        }
    }
}
