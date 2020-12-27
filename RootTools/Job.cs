using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RootTools
{
    public class Job
    {
        #region Group
        class Group
        {
            public string m_sGroup;
            public List<Group> m_aGroup = new List<Group>();
            public List<Item> m_aItem = new List<Item>();

            public Group(string sGroup)
            {
                m_sGroup = sGroup;
            }

            public Group GetGroup(string sGroup)
            {
                foreach (Group group in m_aGroup)
                {
                    if (group.m_sGroup == sGroup) return group;
                }
                Group groupSub = new Group(sGroup);
                m_aGroup.Add(groupSub);
                return groupSub;
            }

            public Item GetItem(string sID)
            {
                foreach (Item item in m_aItem)
                {
                    if (item.m_id == sID) return item;
                }
                return null;
            }

            public void Save(StreamWriter sw, string sLevel)
            {
                string sNextLevel = sLevel + "\t";
                sw.WriteLine(sLevel + "<" + m_sGroup + ">");
                foreach (Item item in m_aItem) item.Save(sw, sNextLevel);
                foreach (Group group in m_aGroup) group.Save(sw, sNextLevel);
                sw.WriteLine(sLevel + "</" + m_sGroup + ">");
            }

            public void Read(StreamReader sr)
            {
                string sLine = sr.ReadLine();
                while (sLine != null)
                {
                    sLine = sLine.Replace("\t", "");
                    if (sLine.Contains("="))
                    {
                        string[] sLines = sLine.Split('=');
                        Item item = new Item(sLines[0], sLines[1]);
                        m_aItem.Add(item);
                    }
                    else
                    {
                        string sGroup = sLine.Substring(1, sLine.Length - 2);
                        if (sGroup[0] == '/') return;
                        Group groupSub = new Group(sGroup);
                        m_aGroup.Add(groupSub);
                        groupSub.Read(sr);
                    }
                    sLine = sr.ReadLine();
                }
            }

            public void CollapseGroup()
            {
                if ((m_aItem.Count == 0) && (m_aGroup.Count == 1))
                {
                    m_sGroup = m_sGroup + "." + m_aGroup[0].m_sGroup;
                    Group group = m_aGroup[0];
                    m_aGroup.Clear();
                    m_aItem = group.m_aItem;
                    m_aGroup = group.m_aGroup;
                    CollapseGroup();
                }
                foreach (Group group in m_aGroup) group.CollapseGroup();
            }

            public void ExpandGroup()
            {
                string[] sGroups = m_sGroup.Split('.');
                if (sGroups.Length > 1)
                {
                    string sNewGroup = m_sGroup.Replace(sGroups[0] + ".", "");
                    Group newGroup = new Group(sNewGroup);
                    foreach (Item item in m_aItem) newGroup.m_aItem.Add(item);
                    m_aItem.Clear();
                    foreach (Group group in m_aGroup) newGroup.m_aGroup.Add(group);
                    m_aGroup.Clear();
                    m_sGroup = sGroups[0];
                    m_aGroup.Add(newGroup);
                    ExpandGroup();
                }
                foreach (Group group in m_aGroup) group.ExpandGroup();
            }
        }
        #endregion

        #region Item
        class Item
        {
            public string m_id;
            public dynamic m_value;

            public Item(string id, dynamic value)
            {
                m_id = id;
                m_value = value;
            }

            public void Save(StreamWriter sw, string sLevel)
            {
                sw.WriteLine(sLevel + m_id + "=" + m_value.ToString());
            }
        }
        #endregion

        #region GetElement
        Group GetGroup(string sGroup)
        {
            string[] asGroup = sGroup.Split('.');
            Group group = m_group;
            foreach (string sID in asGroup)
            {
                group = group.GetGroup(sID);
            }
            return group;
        }

        public dynamic Set(string sGroup, string sID, dynamic value, dynamic valueDef = null)
        {
            if (m_bSave) Save(sGroup, sID, value);
            else
            {
                if (valueDef == null) valueDef = value;
                Item item = Read(sGroup, sID, value);
                if (item == null) return valueDef;
                Type type = value.GetType();
                if (type == typeof(bool)) return Convert.ToBoolean(item.m_value);
                if (type == typeof(int)) return Convert.ToInt32(item.m_value);
                if (type == typeof(long)) return Convert.ToInt64(item.m_value);
                if (type == typeof(double)) return Convert.ToDouble(item.m_value);
                if (type == typeof(string)) return item.m_value;
                if (type == typeof(CPoint)) return new CPoint(item.m_value, m_log);
                if (type == typeof(RPoint)) return new RPoint(item.m_value, m_log);
                return item.m_value;
            }
            return value;
        }

        void Save(string sGroup, string sID, dynamic value)
        {
            if (m_bSave == false) return;
            Group group = GetGroup(sGroup);
            Item item = group.GetItem(sID);
            if (item != null) m_log.Warn("Add Same Property, Group = " + sGroup + " ID = " + sID);
            else
            {
                item = new Item(sID, value);
                group.m_aItem.Add(item);
            }
        }

        Item Read(string sGroup, string sID, dynamic value)
        {
            Group group = GetGroup(sGroup);
            return group.GetItem(sID);
        }
        #endregion

        #region File Stream
        public string m_sMemory = ""; 
        void Save(Stream stream)
        {
            StreamWriter sw = new StreamWriter(stream);
            foreach (Group group in m_group.m_aGroup) group.Save(sw, "");
            if (m_memoryStream != null)
            {
                sw.Flush();
                m_sMemory = Encoding.Default.GetString(m_memoryStream.ToArray()); 
            }
            sw.Close();
        }

        void Read(Stream stream)
        {
            m_group.m_aGroup.Clear();
            m_group.m_aItem.Clear();
            StreamReader sr = new StreamReader(stream);
            m_group.Read(sr);
            sr.Close();
        }
        #endregion

        #region Memory Stream

        #endregion

        bool m_bSave;
        public string m_sFile;
        Group m_group = new Group("ATI");

        Log m_log = null;
        public Job(string sFile, bool bSave, Log log)
        {
            m_sFile = sFile;
            m_bSave = bSave;
            m_log = log;
            try
            {
                if (m_bSave == false)
                {
                    Read(new FileStream(sFile, FileMode.Open));
                    m_group.ExpandGroup();
                }
            }
            catch (Exception e)
            {
                m_log.Error(e.ToString(), "Job Save Error");
            }
        }

        public MemoryStream m_memoryStream = null;
        public Job(MemoryStream memoryStream, bool bSave, Log log)
        {
            m_memoryStream = memoryStream; 
            m_bSave = bSave;
            m_log = log;
            try
            {
                if (m_bSave == false)
                {
                    Read(memoryStream);
                    m_group.ExpandGroup();
                }
            }
            catch (Exception e)
            {
                m_log.Error(e.ToString(), "Job Save Error");
            }
        }

        public void Close()
        {
            try
            {
                if (m_bSave)
                {
                    foreach (Group group in m_group.m_aGroup) group.CollapseGroup();
                    if (m_memoryStream != null) Save(m_memoryStream);
                    else Save(new FileStream(m_sFile, FileMode.Create));
                }
            }
            catch (Exception e)
            {
                m_log.Error(e.ToString(), "Job Save Error");
            }
        }

    }
}
