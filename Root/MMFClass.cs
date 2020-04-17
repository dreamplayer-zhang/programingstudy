using RootTools;

namespace Root
{
    class MMFClass : MMFBase
    {
        Bool m_bUse;
        Byte m_byteWhat; 
        Int m_nCount;
        String m_sInfo; 
        void InitValue()
        {
            m_bUse = GetBool();
            m_byteWhat = GetByte(); 
            m_nCount = GetInt();
            m_sInfo = GetString(50); 
        }

        public MMFClass(string id)
        {
            base.Init(id);
            InitValue(); 
            m_nCount.p_value = 3;
            m_sInfo.p_value = "Hello !!"; 
            Open(); 
        }
    }
}
