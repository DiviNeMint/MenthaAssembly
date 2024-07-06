using System.Globalization;

namespace MenthaAssembly.Globalization
{
    public static class CultureHelper
    {

        public static bool ExistsCulture(string CultureCode)
        {
            try
            {
                CultureInfo.GetCultureInfo(CultureCode);
                return true;
            }
            catch
            {
            }

            return false;
        }

    }
}