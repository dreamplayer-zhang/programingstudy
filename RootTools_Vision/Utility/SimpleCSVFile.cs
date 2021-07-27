using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Utility
{
    public class SimpleCSVFile
    {
        private string filePath = "";

        private string separator = ",";

        public SimpleCSVFile(string filePath)
        {
            this.filePath = filePath;

            CreateFile();
        }

        public void SetSeparator(string seperator)
        {
            this.separator = seperator;

        }

        private void CreateFile()
        {
            if (File.Exists(this.filePath))
                File.Delete(this.filePath);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.Create(filePath).Close();
        }

        public void WriteDataTable(DataTable table, bool append = true, bool writeHeader = true)
        {
            using (StreamWriter sw = new StreamWriter(this.filePath, append, Encoding.UTF8))
            {
                if(writeHeader)
                {
                    string header = string.Join(",", table.Columns.Cast<object>());
                    sw.WriteLine(header);
                }

                foreach(DataRow row in table.Rows)
                {
                    string content = string.Join(",", row.ItemArray.Cast<object>());
                    sw.WriteLine(content);
                }
            }
        }

        public void WriteString(string dataString, bool append = true)
        {
            using (StreamWriter sw = new StreamWriter(this.filePath, append, Encoding.UTF8))
            {
                sw.WriteLine(dataString);
            }
        }

        public void WriteList<T>(List<T> dataList, bool append = true, bool writeHeader = true, bool writeTitle = true)
        {
            using (StreamWriter sw = new StreamWriter(this.filePath, true, Encoding.UTF8))
            {
                if (writeTitle)
                    WriteTitle<T>(sw);

                if (writeHeader)
                    WriteHeader<T>(sw);

                WriteContents<T>(sw, dataList);
            }
        }

        private void WriteTitle<T>(StreamWriter sw)
        {
            sw.WriteLine(typeof(T).Name);
        }


        private void WriteHeader<T>(StreamWriter sw)
        {
            StringBuilder builder = new StringBuilder();
            FieldInfo[] fields = typeof(T).GetFields();

            int index = 0;
            foreach (FieldInfo field in fields)
            {
                if (index == fields.Length - 1)
                {
                    builder.Append(field.Name);
                }
                else
                {
                    builder.Append(field.Name + separator);
                }
                index++;
            }
            sw.WriteLine(builder.ToString());
        }

        private void WriteContents<T>(StreamWriter sw, List<T> dataList)
        {
            StringBuilder builder = new StringBuilder();
            FieldInfo[] fields = typeof(T).GetFields();

            int indexLine = 0;
            foreach (T data in dataList)
            {
                int index = 0;
                foreach (FieldInfo field in fields)
                {
                    var val = field.GetValue(data);
                    if (index == fields.Length - 1)
                    {
                        builder.Append(val);
                    }
                    else
                    {
                        builder.Append(val + separator);
                    }
                    index++;
                }
                if (indexLine != dataList.Count - 1)
                    builder.Append("\n");
            }

            sw.WriteLine(builder.ToString());
        }
    }
}
