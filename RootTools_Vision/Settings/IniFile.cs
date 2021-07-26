using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    class IniFile
    {
        private string iniFilePath = "";
        public string IniFilePath
        {
            get => this.iniFilePath;
            set => this.iniFilePath = value;
        }

        public IniFile(string iniFilePath)
        {
            this.iniFilePath = iniFilePath;
        }

        #region [Ini function Export]
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key,
                     string Value, StringBuilder Result, int Size, string FileName);


        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, int Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);


        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(int Section, string Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);

        [DllImport("kernel32")]
        public static extern int WritePrivateProfileString(string section, string key, string val, string filePath);
        #endregion


        public void WriteParameter(string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, this.iniFilePath);
        }

        public string ReadParameter(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, this.iniFilePath);

            return temp.ToString();
        }

        #region [Method]
        public string[] GetSectionNames()  // ini 파일 안의 모든 section 이름 가져오기
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(0, "", "", bytes, maxsize, this.iniFilePath);


                if (size < maxsize - 2)
                {
                    string Selected = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    return Selected.Split(new char[] { '\0' });
                }
            }
        }


        public string[] GetEntryNames(string section)   // 해당 section 안의 모든 키 값 가져오기
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(section, 0, "", bytes, maxsize, this.iniFilePath);

                if (size < maxsize - 2)
                {
                    string entries = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    return entries.Split(new char[] { '\0' });
                }
            }
        }
        #endregion
    }
}
