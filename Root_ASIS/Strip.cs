﻿using RootTools;

namespace Root_ASIS
{
    public static class Strip
    {
        public static _Strip m_strip = new _Strip(); 

        public static bool p_bUseMGZ
        {
            get { return m_strip.p_bUseMGZ; }
            set { m_strip.p_bUseMGZ = value; }
        }

        public static bool p_bUsePaper
        {
            get { return m_strip.p_bUsePaper; }
            set { m_strip.p_bUsePaper = value; }
        }

        public static bool p_bUseCleanBlow
        {
            get { return m_strip.p_bUseCleanBlow; }
            set { m_strip.p_bUseCleanBlow = value; }
        }

        public static RPoint m_szStripTeach = new RPoint();
        public static RPoint p_szStrip
        {
            get { return m_strip.p_szStrip; }
            set { m_strip.p_szStrip = value; }
        }
    }

    public class _Strip : NotifyProperty
    {
        bool _bUseMGZ = false; 
        public bool p_bUseMGZ
        {
            get { return _bUseMGZ; }
            set
            {
                if (_bUseMGZ == value) return;
                _bUseMGZ = value;
                OnPropertyChanged(); 
            }
        }

        bool _bUsePaper = false;
        public bool p_bUsePaper
        {
            get { return _bUsePaper; }
            set
            {
                if (_bUsePaper == value) return;
                _bUsePaper = value;
                OnPropertyChanged();
            }
        }

        bool _bUseCleanBlow = true;
        public bool p_bUseCleanBlow
        {
            get { return _bUseCleanBlow; }
            set
            {
                if (_bUseCleanBlow == value) return;
                _bUseCleanBlow = value;
                OnPropertyChanged(); 
            }
        }

        RPoint _szStrip = new RPoint(77, 178); 
        public RPoint p_szStrip
        {
            get { return _szStrip; }
            set
            {
                if (_szStrip == value) return;
                _szStrip = value;
                OnPropertyChanged(); 
            }
        }
    }
}