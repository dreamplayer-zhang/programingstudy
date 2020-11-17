using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;

namespace RootTools_Vision
{
    public class Recipe
    {

        [XmlIgnore] RecipeEditor m_RecipeEditor; // 그리기 데이터
        public RecipeInfo m_RecipeInfo; // 레시피 정보 데이터
        public RecipeData m_ReicpeData; // ROI 정보 데이터
        public Parameter m_Parameter; // 파라미터

        public Recipe()
        {
            Init();
        }

        public void Init()
        {
            m_ReicpeData = new RecipeData(true);
            m_RecipeInfo = new RecipeInfo(true);
            m_Parameter = new Parameter();
            m_RecipeEditor = new RecipeEditor(m_ReicpeData);
        }

        #region [ Recipe Save / Load ]

        public void Save(string sFilePath)
        {
            SaveRecipeData(sFilePath);
            SaveRecipeInfo(sFilePath);
            SaveRecipeParameter(sFilePath);
        }

        public void Load(string sFilePath)
        {
            LoadRecipeData(sFilePath);
            LoadRecipeInfo(sFilePath);
            LoadRecipeParameter(sFilePath);
        }

        public void SaveRecipeParameter(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_Parameter.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            FileInfo fi = new FileInfo(sResultFilePath);
            if (fi.Exists)
                fi.Delete();

            using (TextWriter tw = new StreamWriter(sResultFilePath))
            {
                XmlSerializer xml = new XmlSerializer(m_Parameter.GetType());
                xml.Serialize(tw, m_Parameter);
            }
        }

        public void LoadRecipeParameter(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_Parameter.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            XmlSerializer xml = new XmlSerializer(m_Parameter.GetType());
            using (Stream reader = new FileStream(sResultFilePath, FileMode.Open))
            {
                m_Parameter = (Parameter)xml.Deserialize(reader);
            }
        }
        
        public void SaveRecipeInfo(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_RecipeInfo.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            m_RecipeInfo.SetRecipeInfo(sFileName); // FileName

            FileInfo fi = new FileInfo(sResultFilePath);
            if (fi.Exists)
                fi.Delete();

            using (TextWriter tw = new StreamWriter(sResultFilePath))
            {
                XmlSerializer xml = new XmlSerializer(m_RecipeInfo.GetType());
                xml.Serialize(tw, m_RecipeInfo);
            }
        }

        public void LoadRecipeInfo(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_RecipeInfo.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            XmlSerializer xml = new XmlSerializer(m_RecipeInfo.GetType());
            using (Stream reader = new FileStream(sResultFilePath, FileMode.Open))
            {
                m_RecipeInfo = (RecipeInfo)xml.Deserialize(reader);
            }
        }

        public void SaveRecipeData(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_RecipeData.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            FileInfo fi = new FileInfo(sResultFilePath);
            if (fi.Exists)
                fi.Delete();

            using (TextWriter tw = new StreamWriter(sResultFilePath))
            {
                XmlSerializer xml = new XmlSerializer(m_ReicpeData.GetType());
                xml.Serialize(tw, m_ReicpeData);
            }

            // Recipe Data의 ImageFeature
            RecipeData_Position position = m_ReicpeData.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;
            position.SaveFeatures(sPath);
            //
        }

        public void LoadRecipeData(string sFilePath)
        {
            string sPath = Path.GetDirectoryName(sFilePath);
            string sFileName = Path.GetFileNameWithoutExtension(sFilePath);
            string sNewFileName = sFileName + "_RecipeData.xml";
            string sResultFilePath = Path.Combine(sPath, sNewFileName);

            XmlSerializer xml = new XmlSerializer(m_ReicpeData.GetType());
            using (Stream reader = new FileStream(sResultFilePath, FileMode.Open))
            {
                m_ReicpeData = (RecipeData)xml.Deserialize(reader);
            }

            // Recipe Data의 ImageFeature
            RecipeData_Position position = m_ReicpeData.GetRecipeData(typeof(RecipeData_Position)) as RecipeData_Position;
            position.LoadFeatures(sPath);
            //
        }

        #endregion

        public ref RecipeData GetRecipeData() { return ref m_ReicpeData; }
        public ref RecipeInfo GetRecipeInfo() { return ref m_RecipeInfo; }
        public ref RecipeEditor GetRecipeEditor() { return ref m_RecipeEditor; }
        public ref Parameter GetParameter() { return ref m_Parameter; }

        public IRecipeData GetRecipeData(Type type)
        {
            return this.m_ReicpeData.GetRecipeData(type);
        }
        public IParameterData GetParameter(Type type)
        {
            return this.m_Parameter.GetParameter(type);
        }
        public IRecipeInfo GetRecipeInfo(Type type)
        {
            return this.m_RecipeInfo.GetRecipeInfo(type);
        }

    }
}
