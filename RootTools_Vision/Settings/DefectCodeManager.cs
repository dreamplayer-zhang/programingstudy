using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class DefectCodeData
    {
        public string DefectCode { get; set; }
        public string Name { get; set; }
    }

    public class DefectCodeManager
    {
        IniFile iniFile;

        public DefectCodeManager()
        {
            iniFile = new IniFile(Constants.FilePath.DefectCodeFilePath);
        }

        public DefectCodeManager(string iniFilePath)
        {
            iniFile = new IniFile(iniFilePath);
        }

        public void ReadDefectCodes()
        {
            string[] sections = iniFile.GetSectionNames();

            foreach(string section in sections)
            {
                string[] codes = iniFile.GetEntryNames(section);

            }
        }
    }
}
