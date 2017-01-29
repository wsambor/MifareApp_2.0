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
        public static string LastOneSaved = "";

        public static void SaveToFile(string IDvalue)
        {
            DateTime thisDay = DateTime.Today;
            string fileName = (thisDay.ToString("u")).Substring(0, 10) + "_ExamPresence.txt";

            if (!String.Equals(LastOneSaved, IDvalue))
            {
                LastOneSaved = IDvalue;
                File.AppendAllText(fileName, IDvalue + Environment.NewLine);
            }
        }
    }
}
