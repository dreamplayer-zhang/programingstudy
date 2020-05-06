using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Root_Vega
{
    
    class AlarmSortHelper
    {
        public string m_sCode;
        public string m_sState;
        public string m_sClassName;
        public string m_sAlarmName;
        public List<DateTime> m_ListSetT = new List<DateTime>();
        public List<DateTime> m_ListClearT = new List<DateTime>();
    }
    class AlarmRawData
    {
        public DateTime day;
        public string m_sDate
        {
            get;
            set;
        }
        public string m_sCode
        {
            get;
            set;
        }
        public string m_sAlarmName
        {
            get;
            set;
        }
        public string m_sState
        {
            get;
            set;
        }
        public string m_sClassName
        {
            get;
            set;
        }
    }
    
    class MonthChartHelper
    {

    }
    class WeekChartHelper
    {

    }
    public class DayChartPoint
    {
        public DayChartPoint()
        {
        }

        public DayChartPoint(int sec, double value)
        {
            Sec = sec;
            Value = value;
        }
        public int Sec
        {
            get;
            set;
        }
        public double Value
        {
            get;
            set;
        }
    }

    class _7_AlarmViewModel : ObservableObject
    {
        public _7_AlarmViewModel()
        {
            //ReadAlaram(@"C:\Users\JTL\Desktop\Log");
            //ChartInit();

       }

        #region Property
        private ObservableCollection<AlarmRawData> _alarmDatas = new ObservableCollection<AlarmRawData>();
        public ObservableCollection<AlarmRawData> AlarmDatas
        {
            get
            {
                return _alarmDatas;
            }
            set
            {
                SetProperty(ref _alarmDatas, value);
            }
        }
        private SeriesCollection _series;
        public SeriesCollection Series
        {
            get
            {
                return _series;
            }
            set
            {
                SetProperty(ref _series, value);
            }
        }
        private long _min;
        public long Min
        {
            get
            {
                return _min;
            }
            set
            {
                SetProperty(ref _min, value);
            }
        }
        private long _max;
        public long Max
        {
            get
            {
                return _max;
            }
            set
            {

                SetProperty(ref _max, value);
            }
        }
        private long _step;
        public long Step
        {
            get
            {
                return _step;
            }
            set
            {
                SetProperty(ref _step, value);
            }
        }
        private Func<double, string> _formatter;
        public Func<double, string> Formatter
        {
            get
            {
                return _formatter;
            }
            set
            {
                SetProperty(ref _formatter, value);
            }
        }
        private Func<double, string> _gaugeFormatter;
        public Func<double, string> GaugeFormatter
        {
            get
            {
                return _gaugeFormatter;
            }
            set
            {
                SetProperty(ref _gaugeFormatter, value);
            }
        }
        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            
            get
            {
                return _selectedDate;
            }
            set
            {
                SetProperty(ref _selectedDate, value);
                Day_ShowChart(SelectedDate);
            }
        }
        private double _operatingRate;
        public double OperatingRate
        {
            get
            {
                var a = _operatingRate;
                return _operatingRate;
            }
            set
            {
                SetProperty(ref _operatingRate, value);
            }
        }
        private string _strRate;
        public string StrRate
        {
            get
            {
                return _strRate;
            }
            set
            {
                SetProperty(ref _strRate, value);
            }
        }
        private string _uptime;
        public string Uptime
        {
            get
            {
                return _uptime;
            }
            set
            {
                SetProperty(ref _uptime, value);
            }
        }
        private string _downtime;
        public string Downtime
        {
            get
            {
                return _downtime;
            }
            set
            {
                SetProperty(ref _downtime, value);
            }
        }
        private string _mostOften;
        public string MostOften
        {
            get
            {
                return _mostOften;
            }
            set
            {
                SetProperty(ref _mostOften, value);
            }
        }
        private int _alarmCount;
        public int AlarmCount
        {
            get
            {
                return _alarmCount;
            }
            set
            {
                SetProperty(ref _alarmCount, value);
            }
        }

        #endregion
        List<AlarmRawData> m_listAlarmData = new List<AlarmRawData>();
        List<AlarmSortHelper> m_listAlarmSortHelpr = new List<AlarmSortHelper>();
        void ChartInit()
        {
            var Config = Mappers.Xy<DayChartPoint>()
                 .X(DatePoint => DatePoint.Sec)
                 .Y(DatePoint => DatePoint.Value);

            Series = new SeriesCollection(Config);
            Min = 0;
            Max = 3600 * 24;
            Step = 3600;


            GaugeFormatter = value =>
            {
                return "";
                //return value.ToString("F2") + "%";
            };
            Formatter = value =>
            {

                if (value % 3600 == 0)
                {
                    return new DateTime((long)value * TimeSpan.FromSeconds(1).Ticks).ToString("HH:mm");
                }
                else
                    return new DateTime((long)value * TimeSpan.FromSeconds(1).Ticks).ToString("HH:mm:ss");
            };
        }
        void Day_ShowRate(int tick)
        {
            OperatingRate = (double)(tick * 100) / (double)86400;
            StrRate = OperatingRate.ToString("F2") + "%";
        }
        void Day_ShowAlarmList(DateTime selectedday)
        {
            AlarmDatas.Clear();
            foreach(AlarmRawData ad in m_listAlarmData)
            {
                if(ad.day.Date == SelectedDate.Date)
                {
                    AlarmDatas.Add(ad);
                }
            }
            var a = AlarmDatas;
        }
        void Day_ShowChart(DateTime selectedDay)
        {
            if(Series != null)
                Series.Clear();
            int cntTick = DayTickCalc(selectedDay);
            Day_ShowSummary(cntTick);
            Day_ShowRate(cntTick);
            Day_ShowAlarmInfo(selectedDay);
            Day_ShowAlarmList(selectedDay);

            int ChartIndex = 0;
            foreach (AlarmSortHelper alarm in m_listAlarmSortHelpr)
            {
                if (alarm.m_ListSetT.First().Date == selectedDay.Date)
                {
                    var a = alarm.m_sAlarmName;
                    ChartIndex++;
                    LineSeries ls = new LineSeries();
                    ChartValues<DayChartPoint> val = new ChartValues<DayChartPoint>();

                    for(int i =0; i<alarm.m_ListSetT.Count; i++)
                    {
                        int SetTick = TickConverter(alarm.m_ListSetT[i]);
                        val.Add(new DayChartPoint(SetTick, ChartIndex));
                        
                        int ClrTick = TickConverter(alarm.m_ListClearT[i]);
                        val.Add(new DayChartPoint(ClrTick, ChartIndex));

                        val.Add(new DayChartPoint(ClrTick, double.NaN));                     
                    }
                    //ls.Fill = Brushes.Transparent;
                    ls.Opacity = 1;
                    ls.StrokeThickness = 5;
                    ls.Title = alarm.m_sAlarmName;
                    ls.Values = val;
                    Series.Add(ls);
                }
            }           
        }
        void Day_ShowSummary(int tick)
        {
            TimeSpan ts = TimeSpan.FromSeconds(tick);
            DateTime dt = new DateTime(ts.Ticks);
            var a = dt.ToString("HH:mm:ss");   
            if(ts.Days == 1)
            {
                Uptime = "24:00:00";
                Downtime = "00:00:00";
            }
            else
            {
                Uptime = dt.ToString("HH:mm:ss");   
                
                int reverse = 86400 - tick;
                ts = TimeSpan.FromSeconds(reverse);
                dt = new DateTime(ts.Ticks);
                Downtime = dt.ToString("HH:mm:ss");
            }
        }

        void Day_ShowAlarmInfo(DateTime selectedday)
        {
            AlarmSortHelper most = new AlarmSortHelper();
            int Cnt = 0;
            foreach(AlarmSortHelper ah in m_listAlarmSortHelpr)
            {
                if (ah.m_ListSetT.First().Date == selectedday.Date)
                {
                    if (most.m_ListSetT.Count < ah.m_ListSetT.Count)
                        most = ah;
                    
                    for(int i=0; i<ah.m_ListSetT.Count; i++)
                    {
                        Cnt++;
                    }

                }
                }
            AlarmCount = Cnt;
            MostOften = most.m_sAlarmName;
            }

        int DayTickCalc(DateTime selectedday)
        {
            bool[] dayTick = new bool[86400];
            dayTick = Enumerable.Repeat(true, 86400).ToArray();
            foreach(AlarmSortHelper ah in m_listAlarmSortHelpr)
            {
                if (ah.m_ListSetT.First().Date == selectedday.Date)
                {
                    for (int i = 0; i < ah.m_ListClearT.Count; i++)
                    {
                        int clrT = TickConverter(ah.m_ListClearT[i]);
                        int setT = TickConverter(ah.m_ListSetT[i]);

                        if (setT > clrT)
                            return 0;
                        while (setT != clrT)
                        {
                            dayTick[setT] = false;
                            setT++;
                        }
                    }
                }
            }
            int cntTick = dayTick.Count(value => value == true);
            return cntTick;
        }
        int TickConverter(DateTime _dateTime)
        {
            int hour = _dateTime.Hour * 3600;
            int minute = _dateTime.Minute * 60;
            int sec = _dateTime.Second;

            int tick = hour + minute + sec;
            
            return tick;
        }
        void ReadAlaram(string path)
        {
            return;
            DirectoryInfo[] dirs = new DirectoryInfo(path).GetDirectories(EQ.m_sModel, SearchOption.AllDirectories);
            foreach (DirectoryInfo di in dirs)
            {
                List<AlarmSortHelper> AlarmTemp = new List<AlarmSortHelper>();
                FileInfo[] files = di.GetFiles("*.csv", SearchOption.AllDirectories);
                foreach (FileInfo fi in files)
                {
                    
                    List<AlarmSortHelper> BeforeSort = new List<AlarmSortHelper>();
                    if (fi.Name.Contains("AlarmData"))
                    {
                        string fileName = fi.Name.Replace("\\\\", "\\");
                        using (FileStream fs = new FileStream(fi.FullName, FileMode.Open))
                        {
                            StreamReader r = new StreamReader(fs, Encoding.Default, true);
                            string str;
                            char sp = '\t';
                            char sp2 = ' ';
                            AlarmSortHelper ash;
                            AlarmRawData ad;
                            while ((str = r.ReadLine()) != string.Empty && str != null && str != " ")
                            {                    
                                ad = new AlarmRawData();
                                ash = new AlarmSortHelper();
                                string[] Log;
                                string[] Temp;
                                Log = str.Split(sp);
                                Temp = Log[3].Split(sp2);

                                ash.m_sAlarmName = Temp[4];
                                ad.m_sAlarmName = Temp[4];

                                ash.m_sCode = Log[1];
                                ad.m_sCode = Log[1];

                                ash.m_sState = Temp[0];
                                ad.m_sState = Temp[0];

                                ash.m_sClassName = Temp[2];
                                ad.m_sClassName = Temp[2];

                                ad.day = Convert.ToDateTime(Log[0]);
                                ad.m_sDate = Convert.ToDateTime(Log[0]).ToString("MM/dd HH:mm:ss");
                                if (ash.m_sState == "Set")
                                {
                                    ash.m_ListSetT.Add(Convert.ToDateTime(Log[0]));
                                }
                                if (ash.m_sState == "Clear")
                                {
                                    ash.m_ListClearT.Add(Convert.ToDateTime(Log[0]));
                                }
                                BeforeSort.Add(ash);
                                m_listAlarmData.Add(ad);
                            }

                            List<string> ListName = new List<string>();
                            AlarmTemp = new List<AlarmSortHelper>();
                            for (int i = 0; i < BeforeSort.Count; i++)
                            {
                                AlarmSortHelper find = BeforeSort[i];
                                AlarmSortHelper sort = new AlarmSortHelper();

                                foreach(AlarmSortHelper ah in AlarmTemp) // 현재 File에서 같은 이름은 무시
                                {
                                    ListName.Add(ah.m_sAlarmName);
                                }
                                if (ListName.Contains(find.m_sAlarmName))
                                    continue;

                                var Data = from data in BeforeSort
                                           where data.m_sAlarmName == find.m_sAlarmName
                                           where data.m_sClassName == find.m_sClassName
                                           select data;
                                List<AlarmSortHelper> sorting = Data.ToList();
                                
                                sort.m_sAlarmName = find.m_sAlarmName;
                                sort.m_sClassName = find.m_sClassName;
                                sort.m_sCode = find.m_sCode;

                                foreach(AlarmSortHelper ah in sorting)
                                {
                                    sort.m_ListSetT.AddRange(ah.m_ListSetT);
                                    sort.m_ListClearT.AddRange(ah.m_ListClearT);
                                }
                                if(sort.m_ListSetT.Count > sort.m_ListClearT.Count)
                                {
                                    sort.m_ListClearT.Add(sort.m_ListSetT.Last().Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                                }
                                if(sort.m_ListClearT.Count > sort.m_ListSetT.Count)
                                {
                                    sort.m_ListSetT.Insert(0, (sort.m_ListClearT.Last().Date));
                                }

                                m_listAlarmSortHelpr.Add(sort);
                                AlarmTemp.Add(sort);
                            }
                            r.Close();
                        }
                    }
                }
            }
        }
    }
}
