using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareApp_2._0.Model
{
    class SaveFile
    {
        public static DateTime ThisDay = DateTime.Now;
        public static string LastOneSaved;
        public static string FileName;

        public static void SaveToFile(string IDvalue)
        {
            if (String.IsNullOrEmpty(FileName))
            {
                FileName = ThisDay.ToString("yyyy-MM-dd_HH-mm") + Constants.FILE_NAME_SUFFIX;
            }

            if (!String.Equals(LastOneSaved, IDvalue))
            {
                LastOneSaved = IDvalue;
                File.AppendAllText(FileName, IDvalue + Environment.NewLine);
            }
        }
    }
}
