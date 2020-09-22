﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RootTools;

namespace RootTools_Vision
{
    /// <summary>
    /// Recipe 각 항목의 묶음.
    /// Recipe에 대한 기능 자체는 Manager에서 다룸.
    /// </summary>
    public class Recipe 
    {
        // ORIGIN, Die Pitch
        // WAFER MAP
        // POSITION
        // ROI (SURFACE, D2D)
        // INSPECTION PARAMETER


        [XmlIgnore] RecipeEditor m_RecipeEditor; // 그리기 데이터
        public RecipeInfo m_RecipeInfo; // 레시피 정보 데이터
        public RecipeData m_ReicpeData; // ROI 정보 데이터

        public Recipe()
        {
            Init();
        }

        public void Init()
        {
            m_ReicpeData = new RecipeData();
            m_RecipeInfo = new RecipeInfo();
            m_RecipeEditor = new RecipeEditor(m_ReicpeData);
        }
        
        public ref RecipeData GetRecipeData() { return ref m_ReicpeData; }
        public ref RecipeInfo GetRecipeInfo() { return ref m_RecipeInfo; }
        public ref RecipeEditor GetRecipeEditor() { return ref m_RecipeEditor; }

        public IRecipeData GetRecipeData(Type type)
        {
            return this.m_ReicpeData.GetRecipeData(type);
        }

    }
}
