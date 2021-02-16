using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.Database
{
    public class DatabaseAttribute : Attribute
    {
        public enum keyEnum
        {
            PRIMARYKEY,
            REFERENCEKEY,
            NONE
        } 

        public enum typeEnum
        {
            CHAR,
            VARCAHR,
            INT,
            DOUBLE,
            DATE,
            DATETIME,
            TIMESTAMP,
            BINARY,
        }
        
        private keyEnum key;
        private object defaultValue;
        private bool isNotNull;
        private bool isAutoIncrement;
        private int autoIncrementStartValue;
        private int size;
        private typeEnum type;

        public DatabaseAttribute(typeEnum type, int size = -1, bool isNotNull = false, bool isAutoIncrement = false, int autoIncrementStartValue = 0, keyEnum key = keyEnum.NONE, object defaultValue = null)
        {
            this.key = key;
            this.type = type;
            this.size = size;
            this.isNotNull = isNotNull;
            this.isAutoIncrement = isAutoIncrement;
            this.autoIncrementStartValue = autoIncrementStartValue;
            this.defaultValue = defaultValue;
        }

        public keyEnum Key 
        {
            get => key;
            set => key = value; 
        }
        public typeEnum Type
        {
            get => type;
            set => type = value;
        }
        public dynamic DefaultValue 
        {
            get => defaultValue;
            set => defaultValue = value;
        }
        public bool IsNotNull
        {
            get => isNotNull; 
            set => isNotNull = value; 
        }
        public bool IsAutoIncrement {
            get => isAutoIncrement;
            set => isAutoIncrement = value; 
        }
        public int Size { get => size; set => size = value; }
        public int AutoIncrementStartValue { get => autoIncrementStartValue; set => autoIncrementStartValue = value; }
    }
}
