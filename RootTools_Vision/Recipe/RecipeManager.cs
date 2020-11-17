using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace RootTools_Vision
{
    /// <summary>
    /// Recipe 클래스 기반 부가 기능 상세.
    /// </summary>
    /// 

    public class RecipeManager
    {
        // EXPORT
        // IMPORT
        // SAVE
        // LOAD
        // Convert Recipe
      
        public Recipe m_Recipe; // Serialize를 위해서 Public 선언이 필요
        public string m_sRecipeDefaultPath = @"C:\Root\Recipe";
        public RecipeManager()
        {
            m_Recipe = new Recipe();

            DirectoryInfo di = new DirectoryInfo(m_sRecipeDefaultPath);
            if (!di.Exists)
                di.Create();
        }

        public ref Recipe GetRecipe()
        {
            return ref m_Recipe;
        }

        public void SaveRecipe()
        {
            try
            {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.InitialDirectory = m_sRecipeDefaultPath;

                ofd.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string sFilePath = Path.GetDirectoryName(ofd.FileName); // 디렉토리명
                    string sFileNameNoExt = Path.GetFileNameWithoutExtension(ofd.FileName); // Only 파일이름
                    string sFileName = Path.GetFileName(ofd.FileName); // 파일이름 + 확장자
                    string sFolderPath = Path.Combine(sFilePath, sFileNameNoExt); // 레시피 이름으로 된 폴더
                    string sResultFileName = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                    DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                    if (!dir.Exists)
                        dir.Create();

                    this.Save(sResultFileName);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Save Recipe : " + ex.Message);
            }
        }

        public void LoadRecipe()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this.Load(ofd.FileName);
                    WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Load Recipe : " + ex.Message);
            }
        }

        public void Save(string sFilePath)
        {
            string sTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            StreamWriter wrtier;
            wrtier = File.CreateText(sFilePath);
            wrtier.WriteLine(sTime);
            wrtier.Close();

            m_Recipe.Save(sFilePath);
            this.Load(sFilePath);
            WorkEventManager.OnUIRedraw(this, new UIRedrawEventArgs());

            //MessageBox.Show("Recipe Save Done");
        }

        public void Load(string sFilePath)
        {
            m_Recipe.Load(sFilePath);
            //MessageBox.Show("Recipe Load Done");
        }


        public void ExportRecipe() { }
        public void ImportRecipe() { }


    }
}
