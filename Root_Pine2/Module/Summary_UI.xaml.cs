using RootTools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Pine2.Module
{
    /// <summary>
    /// Summary_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Summary_UI : UserControl
    {
        public Summary_UI()
        {
            InitializeComponent();
        }

        Dictionary<Summary.Data.Strip.Unit.eResult, Brush> m_aBrush = new Dictionary<Summary.Data.Strip.Unit.eResult, Brush>(); 
        Summary m_summary; 
        public void Init(Summary summary)
        {
            m_summary = summary;

            m_aBrush.Add(Summary.Data.Strip.Unit.eResult.Good, Brushes.LightGreen);
            m_aBrush.Add(Summary.Data.Strip.Unit.eResult.Bad, Brushes.LightPink);
            m_aBrush.Add(Summary.Data.Strip.Unit.eResult.PosError, Brushes.LightBlue);
            m_aBrush.Add(Summary.Data.Strip.Unit.eResult.XOut, Brushes.Orange);
            m_aBrush.Add(Summary.Data.Strip.Unit.eResult.Unknown, Brushes.White);

            InitStripInfo(); 
            InitStripCount();
            InitUnitCount();
            InitTactTime();
            InitUnitMap();

            InitTimer(); 
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_summary.m_bUpdated == false) return;
            m_summary.m_bUpdated = false;
            OnUpdateStripInfo(); 
            OnUpdateStripCount();
            OnUpdateUnitCount();
            OnUpdateTactTime();
            OnUpdateUnitMap();
        }
        #endregion

        #region Get UI
        Label GetLabel(string sContent)
        {
            Label label = new Label();
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Content = sContent;
            return label; 
        }

        Grid GetGrid(int x, int y, Label label, Brush brushes)
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(1);
            grid.Background = brushes;
            grid.Children.Add(label);
            Grid.SetColumn(grid, x);
            Grid.SetRow(grid, y);
            return grid;
        }
        #endregion

        #region Strip Info
        Label[,] m_labelStripInfo = new Label[4, 4];
        Label m_labelStripID;
        Label m_labelMapSize; 
        void InitStripInfo()
        {
            gridStripInfo.Children.Clear();
            gridStripInfo.ColumnDefinitions.Clear();
            for (int x = 0; x < 5; x++) gridStripInfo.ColumnDefinitions.Add(new ColumnDefinition());
            gridStripInfo.RowDefinitions.Clear();
            for (int y = 0; y < 6; y++) gridStripInfo.RowDefinitions.Add(new RowDefinition());
            gridStripInfo.Children.Add(GetGrid(0, 0, GetLabel("StripID"), Brushes.AliceBlue));
            m_labelStripID = GetLabel("0000");
            gridStripInfo.Children.Add(GetGrid(1, 0, m_labelStripID, Brushes.Beige));
            gridStripInfo.Children.Add(GetGrid(3, 0, GetLabel("Size"), Brushes.AliceBlue));
            m_labelMapSize = GetLabel("0,0");
            gridStripInfo.Children.Add(GetGrid(4, 0, m_labelMapSize, Brushes.Beige));
            gridStripInfo.Children.Add(GetGrid(0, 1, GetLabel(""), Brushes.AliceBlue));
            gridStripInfo.Children.Add(GetGrid(1, 1, GetLabel("Total"), Brushes.AliceBlue));
            gridStripInfo.Children.Add(GetGrid(2, 1, GetLabel("Top3D"), Brushes.AliceBlue));
            gridStripInfo.Children.Add(GetGrid(3, 1, GetLabel("Top2D"), Brushes.AliceBlue));
            gridStripInfo.Children.Add(GetGrid(4, 1, GetLabel("Bottom"), Brushes.AliceBlue));
            gridStripInfo.Children.Add(GetGrid(0, 2, GetLabel("Good"), m_aBrush[Summary.Data.Strip.Unit.eResult.Good]));
            gridStripInfo.Children.Add(GetGrid(0, 3, GetLabel("Bad"), m_aBrush[Summary.Data.Strip.Unit.eResult.Bad]));
            gridStripInfo.Children.Add(GetGrid(0, 4, GetLabel("Pos"), m_aBrush[Summary.Data.Strip.Unit.eResult.PosError]));
            gridStripInfo.Children.Add(GetGrid(0, 5, GetLabel("Xout"), m_aBrush[Summary.Data.Strip.Unit.eResult.XOut]));
            for (int y = 2; y < 6; y++)
            {
                for (int x = 1; x < 5; x++)
                {
                    Label label = GetLabel("0");
                    gridStripInfo.Children.Add(GetGrid(x, y, label, Brushes.Beige));
                    m_labelStripInfo[x - 1, y - 2] = label;
                }
            }
        }

        void OnUpdateStripInfo()
        {
            m_labelStripID.Content = m_summary.m_data.m_sStripID;
            m_labelMapSize.Content = m_summary.m_data.m_szMap.X.ToString() + ", " + m_summary.m_data.m_szMap.Y.ToString(); 
            foreach (Summary.Data.eVision eVision in Enum.GetValues(typeof(Summary.Data.eVision)))
            {
                int x = (int)eVision;
                Summary.Data.Strip.Unit unit = m_summary.m_data.m_aStrip[eVision].m_unit;
                foreach (Summary.Data.Strip.Unit.eResult eResult in Enum.GetValues(typeof(Summary.Data.Strip.Unit.eResult)))
                {
                    int y = (int)eResult;
                    if (eResult != Summary.Data.Strip.Unit.eResult.Unknown) m_labelStripInfo[x, y].Content = unit.m_aCount[eResult].ToString(); 
                }
            }
        }
        #endregion

        #region Strip Count
        Label[,] m_labelCountStrip = new Label[4, 4]; 
        void InitStripCount()
        {
            gridStripCount.Children.Clear();
            gridStripCount.ColumnDefinitions.Clear();
            for (int x = 0; x < 5; x++) gridStripCount.ColumnDefinitions.Add(new ColumnDefinition());
            gridStripCount.RowDefinitions.Clear();
            for (int y = 0; y < 5; y++) gridStripCount.RowDefinitions.Add(new RowDefinition());
            gridStripCount.Children.Add(GetGrid(0, 0, GetLabel(""), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(1, 0, GetLabel("Total"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(2, 0, GetLabel("Top3D"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(3, 0, GetLabel("Top2D"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(4, 0, GetLabel("Bottom"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(0, 1, GetLabel("Good"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(0, 2, GetLabel("Defect"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(0, 3, GetLabel("Pos"), Brushes.AliceBlue));
            gridStripCount.Children.Add(GetGrid(0, 4, GetLabel("BCD"), Brushes.AliceBlue));
            for (int y = 1; y < 5; y++)
            {
                for (int x = 1; x < 5; x++)
                {
                    Label label = GetLabel("0");
                    gridStripCount.Children.Add(GetGrid(x, y, label, Brushes.Beige));
                    m_labelCountStrip[x - 1, y - 1] = label; 
                }
            }
        }

        void OnUpdateStripCount()
        {
            foreach (Summary.Data.eVision eVision in Enum.GetValues(typeof(Summary.Data.eVision)))
            {
                int x = (int)eVision;
                Summary.CountStrip countStrip = m_summary.m_countStrip[eVision]; 
                foreach (Summary.Data.Strip.eResult eResult in Enum.GetValues(typeof(Summary.Data.Strip.eResult)))
                {
                    int y = (int)eResult;
                    m_labelCountStrip[x, y].Content = countStrip.m_aCount[eResult].ToString(); 
                }
            }
        }
        #endregion

        #region Unit Count
        Label[,] m_labelCountUnit = new Label[4, 4];
        void InitUnitCount()
        {
            gridUnitCount.Children.Clear();
            gridUnitCount.ColumnDefinitions.Clear();
            for (int x = 0; x < 5; x++) gridUnitCount.ColumnDefinitions.Add(new ColumnDefinition());
            gridUnitCount.RowDefinitions.Clear();
            for (int y = 0; y < 5; y++) gridUnitCount.RowDefinitions.Add(new RowDefinition());
            gridUnitCount.Children.Add(GetGrid(0, 0, GetLabel(""), Brushes.AliceBlue));
            gridUnitCount.Children.Add(GetGrid(1, 0, GetLabel("Total"), Brushes.AliceBlue));
            gridUnitCount.Children.Add(GetGrid(2, 0, GetLabel("Top3D"), Brushes.AliceBlue));
            gridUnitCount.Children.Add(GetGrid(3, 0, GetLabel("Top2D"), Brushes.AliceBlue));
            gridUnitCount.Children.Add(GetGrid(4, 0, GetLabel("Bottom"), Brushes.AliceBlue));
            gridUnitCount.Children.Add(GetGrid(0, 1, GetLabel("Good"), m_aBrush[Summary.Data.Strip.Unit.eResult.Good]));
            gridUnitCount.Children.Add(GetGrid(0, 2, GetLabel("Bad"), m_aBrush[Summary.Data.Strip.Unit.eResult.Bad]));
            gridUnitCount.Children.Add(GetGrid(0, 3, GetLabel("Pos"), m_aBrush[Summary.Data.Strip.Unit.eResult.PosError]));
            gridUnitCount.Children.Add(GetGrid(0, 4, GetLabel("Xout"), m_aBrush[Summary.Data.Strip.Unit.eResult.XOut]));
            for (int y = 1; y < 5; y++)
            {
                for (int x = 1; x < 5; x++)
                {
                    Label label = GetLabel("0");
                    gridUnitCount.Children.Add(GetGrid(x, y, label, Brushes.Beige));
                    m_labelCountUnit[x - 1, y - 1] = label;
                }
            }
        }

        void OnUpdateUnitCount()
        {
            foreach (Summary.Data.eVision eVision in Enum.GetValues(typeof(Summary.Data.eVision)))
            {
                int x = (int)eVision;
                Summary.CountUnit countUnit = m_summary.m_countUnit[eVision];
                foreach (Summary.Data.Strip.Unit.eResult eResult in Enum.GetValues(typeof(Summary.Data.Strip.Unit.eResult)))
                {
                    int y = (int)eResult;
                    if (eResult != Summary.Data.Strip.Unit.eResult.Unknown) m_labelCountUnit[x, y].Content = countUnit.m_aCount[eResult].ToString();
                }
            }
        }
        #endregion

        #region Tact Time
        Label[] m_labelTime = new Label[4]; 
        void InitTactTime()
        {
            gridTime.Children.Add(GetGrid(0, 0, GetLabel("Lot Start"), Brushes.AliceBlue));
            gridTime.Children.Add(GetGrid(0, 1, GetLabel("Lot Run"), Brushes.AliceBlue));
            gridTime.Children.Add(GetGrid(0, 2, GetLabel("Tact Time"), Brushes.AliceBlue));
            gridTime.Children.Add(GetGrid(0, 3, GetLabel("Tact Ave"), Brushes.AliceBlue));
            for (int n = 0; n < 4; n++)
            {
                m_labelTime[n] = GetLabel("00:00:00");
                gridTime.Children.Add(GetGrid(1, n, m_labelTime[n], Brushes.Beige));
            }
        }

        void OnUpdateTactTime()
        {
            m_labelTime[0].Content = m_summary.m_sLotStart;
            m_labelTime[1].Content = m_summary.m_sLotTime;
            m_labelTime[2].Content = m_summary.m_sTactTime;
            m_labelTime[3].Content = m_summary.m_sTactAve;
        }
        #endregion

        #region UnitMap
        public class UnitUI
        {
            public Dictionary<Summary.Data.eVision, Grid> m_aGrid = new Dictionary<Summary.Data.eVision, Grid>();

            public void SetResult(Summary.Data.eVision eVision, Brush brush)
            {
                m_aGrid[eVision].Background = brush; 
            }

            public UnitUI(CPoint cp)
            {
                foreach (Summary.Data.eVision eVision in Enum.GetValues(typeof(Summary.Data.eVision))) m_aGrid.Add(eVision, new Grid());

                Grid grid = m_aGrid[Summary.Data.eVision.Total]; 
                grid.Margin = new Thickness(1);
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                for (int x = 0; x < 2; x++) grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Clear();
                for (int x = 0; x < 2; x++) grid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(grid, cp.X);
                Grid.SetRow(grid, cp.Y);

                InitGrid(m_aGrid[Summary.Data.eVision.Top3D], 0, 0);
                InitGrid(m_aGrid[Summary.Data.eVision.Top2D], 1, 0);
                InitGrid(m_aGrid[Summary.Data.eVision.Bottom], 1, 1);
            }

            void InitGrid(Grid grid, int x, int y)
            {
                grid.Margin = new Thickness(2);
                Grid.SetColumn(grid, x);
                Grid.SetRow(grid, y);
                m_aGrid[Summary.Data.eVision.Total].Children.Add(grid); 
            }
        }

        public List<List<UnitUI>> m_aUnitUI = new List<List<UnitUI>>();
        CPoint m_szMap = new CPoint(); 
        void InitUnitMap()
        {
            if (m_szMap == m_summary.m_data.m_szMap) return;
            gridUnit.Children.Clear();
            m_aUnitUI.Clear();
            m_szMap = m_summary.m_data.m_szMap; 
            while (m_aUnitUI.Count < m_szMap.Y) m_aUnitUI.Add(new List<UnitUI>());
            for (int yp = 0; yp < m_szMap.Y; yp++)
            {
                while (m_aUnitUI[yp].Count < m_szMap.X)
                {
                    UnitUI ui = new UnitUI(new CPoint(m_aUnitUI[yp].Count, yp)); 
                    m_aUnitUI[yp].Add(ui);
                    gridUnit.Children.Add(ui.m_aGrid[Summary.Data.eVision.Total]); 
                }
            }
        }

        void OnUpdateUnitMap()
        {
            InitUnitMap();
            foreach (Summary.Data.Strip strip in m_summary.m_data.m_aStrip.Values)
            {
                CPoint szMap = strip.m_unit.m_szMap;
                for (int y = 0; y < szMap.Y; y++)
                {
                    for (int x = 0; x < szMap.X; x++)
                    {
                        m_aUnitUI[y][x].SetResult(strip.m_eVision, m_aBrush[strip.m_unit.m_aUnit[y][x]]);
                    }
                }
            }
        }
        #endregion
    }
}
