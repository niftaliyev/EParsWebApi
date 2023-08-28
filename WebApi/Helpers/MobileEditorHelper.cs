using System.Text;

namespace WebApi.Helpers
{
    public class MobileEditorHelper
    {
        public static string Edit(StringBuilder mobile)
        {
            string[] charsToRemove = new string[] { "(", ")", " " };
            int legalLengthOfMobile = 10;
            foreach (var c in charsToRemove)
            {
                mobile = mobile.Replace(c, string.Empty);
            }

            if (mobile.Length > legalLengthOfMobile)
            {
                int divide = (mobile.Length / legalLengthOfMobile) - 1;
                int commaIdx = legalLengthOfMobile;

                while (divide > 0)
                {
                    mobile = mobile.Insert(commaIdx, ",");
                    commaIdx += legalLengthOfMobile;
                    divide--;
                }
            }

            return mobile.ToString();

        }
    }
}
