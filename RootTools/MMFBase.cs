using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RootTools
{
    public class MMFBase
    {
        #region bool
        public class Bool
        {
            bool _value = false;
            public bool p_value
            {
                get
                {
                    m_mmf.m_acc.Read(m_iAdd, out _value);
                    return _value;
                }
                set
                {
                    m_mmf.m_acc.Write(m_iAdd, value);
                }
            }

            MMFBase m_mmf;
            int m_iAdd = 0;
            public Bool(MMFBase mmf, ref int iAdd)
            {
                m_mmf = mmf;
                m_iAdd = iAdd;
                iAdd += Marshal.SizeOf(_value);
            }
        }
        protected Bool GetBool()
        {
            Bool value = new Bool(this, ref m_iSize);
            return value;
        } 
        #endregion

        #region byte
        public class Byte
        {
            byte _value = 0;
            public byte p_value
            {
                get
                {
                    m_mmf.m_acc.Read(m_iAdd, out _value);
                    return _value;
                }
                set
                {
                    m_mmf.m_acc.Write(m_iAdd, value);
                }
            }

            MMFBase m_mmf;
            int m_iAdd = 0;
            public Byte(MMFBase mmf, ref int iAdd)
            {
                m_mmf = mmf;
                m_iAdd = iAdd;
                iAdd += Marshal.SizeOf(_value);
            }
        }
        protected Byte GetByte()
        {
            Byte value = new Byte(this, ref m_iSize);
            return value;
        }
        #endregion

        #region short
        public class Short
        {
            short _value = 0;
            public short p_value
            {
                get
                {
                    m_mmf.m_acc.Read(m_iAdd, out _value);
                    return _value;
                }
                set
                {
                    m_mmf.m_acc.Write(m_iAdd, value);
                }
            }

            MMFBase m_mmf;
            int m_iAdd = 0;
            public Short(MMFBase mmf, ref int iAdd)
            {
                m_mmf = mmf;
                m_iAdd = iAdd;
                iAdd += Marshal.SizeOf(_value);
            }
        }
        protected Short GetShort()
        {
            Short value = new Short(this, ref m_iSize);
            return value;
        }
        #endregion

        #region int
        public class Int
        {
            int _value = 0;
            public int p_value
            {
                get
                {
                    m_mmf.m_acc.Read(m_iAdd, out _value);
                    return _value;
                }
                set
                {
                    m_mmf.m_acc.Write(m_iAdd, value);
                }
            }

            MMFBase m_mmf; 
            int m_iAdd = 0;
            public Int(MMFBase mmf, ref int iAdd)
            {
                m_mmf = mmf; 
                m_iAdd = iAdd;
                iAdd += Marshal.SizeOf(_value);
            }
        }
        protected Int GetInt()
        {
            Int value = new Int(this, ref m_iSize);
            return value; 
        }
        #endregion

        #region double
        public class Double
        {
            double _value = 0;
            public double p_value
            {
                get
                {
                    m_mmf.m_acc.Read(m_iAdd, out _value);
                    return _value;
                }
                set
                {
                    m_mmf.m_acc.Write(m_iAdd, value);
                }
            }

            MMFBase m_mmf;
            int m_iAdd = 0;
            public Double(MMFBase mmf, ref int iAdd)
            {
                m_mmf = mmf;
                m_iAdd = iAdd;
                iAdd += Marshal.SizeOf(_value);
            }
        }
        protected Double GetDouble()
        {
            Double value = new Double(this, ref m_iSize);
            return value;
        }
        #endregion

        #region string
        public class String
        {
            string _value = "";
            public string p_value
            {
                get
                {
                    _value = "";
                    int nLength = 0; 
                    m_mmf.m_acc.Read(m_iAdd, out nLength);
                    int iAdd = m_iAdd + sizeof(int); 
                    for (int n = 0; n < nLength; n++)
                    {
                        char ch;
                        m_mmf.m_acc.Read(iAdd, out ch);
                        iAdd += sizeof(char);
                        _value += ch; 
                    }
                    return _value;
                }
                set
                {
                    _value = value;
                    int iAdd = m_iAdd;
                    if (_value.Length >= m_maxLength) _value = _value.Substring(0, m_maxLength - 1);
                    m_mmf.m_acc.Write(iAdd, _value.Length); iAdd += sizeof(int); 
                    foreach (char ch in _value)
                    {
                        m_mmf.m_acc.Write(iAdd, ch);
                        iAdd += sizeof(char); 
                    }
                }
            }

            MMFBase m_mmf;
            int m_iAdd = 0;
            int m_maxLength = 80; 
            public String(MMFBase mmf, int maxLength, ref int iAdd)
            {
                m_mmf = mmf;
                m_iAdd = iAdd;
                iAdd += m_maxLength + sizeof(int);
            }
        }
        protected String GetString(int maxLength)
        {
            String value = new String(this, maxLength, ref m_iSize);
            return value;
        }
        #endregion

        MemoryMappedFile m_memFile = null;
        MemoryMappedViewAccessor m_acc = null;
        int m_iSize; 
        protected string m_id;
        protected void Init(string id)
        {
            m_id = id;
        }

        protected void Open()
        {
            try { m_memFile = MemoryMappedFile.OpenExisting(m_id); }
            catch { m_memFile = null; }
            m_memFile = MemoryMappedFile.CreateOrOpen(m_id, m_iSize);
            m_acc = m_memFile.CreateViewAccessor(); 
        }
    }
}
