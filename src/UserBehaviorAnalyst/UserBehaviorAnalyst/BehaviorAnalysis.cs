using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace UserBehaviorAnalyst
{
    class BusLineStatistics
    {
        public BusLineStatistics()
        {
            TimeStatistics = new Dictionary<int, int>();
            for (int i = 6; i <= 23; i++)
            {
                TimeStatistics.Add(i, 0);
            }
        }
        public int Count
        {
            get;
            set;
        }
        public DateTime EarlestBoardTime
        {
            get;
            set;
        }
        public DateTime LatestAlightTime
        {
            get;
            set;
        }
        public Dictionary<int, int> TimeStatistics
        {
            get;
            set;
        }
    }

    class BehaviorAnalysis
    {
        //private DataOperation DataOperation;
        private DataSet records;
        public SortedDictionary<int,Dictionary<string,BusLineStatistics>> results;

        public BehaviorAnalysis()
        {
            //DataOperation = new DataOperation();
            records = DataOperation.RetrieveData("TripRecord");
            results = new SortedDictionary<int, Dictionary<string, BusLineStatistics>>();
        }

        public void Analyse()
        {
            DataTable table = records.Tables["TripRecord"];
            foreach (DataRow row in table.Rows)
            {
                int userID = (int)row["CellPhoneAddr"];
                string busLine = (string)row["BusLine"];
                DateTime BTime = (DateTime)row["BoardTime"];
                DateTime ATime = (DateTime)row["AlightTime"];

                Dictionary<string, BusLineStatistics> userSt;
                if (results.TryGetValue(userID, out userSt))
                {
                    BusLineStatistics busSt;
                    if (userSt.TryGetValue(busLine, out busSt))
                    {
                        busSt.Count++;
                        if (busSt.EarlestBoardTime.CompareTo(BTime) > 0)
                        {
                            busSt.EarlestBoardTime = BTime;
                        }
                        if (busSt.LatestAlightTime.CompareTo(ATime) < 0)
                        {
                            busSt.LatestAlightTime = ATime;
                        }
                        UpdateTimeStatistics(BTime, ATime, busSt);
                    }
                    else
                    {
                        busSt = new BusLineStatistics() { Count = 1, EarlestBoardTime = BTime, LatestAlightTime = ATime };
                        UpdateTimeStatistics(BTime, ATime, busSt);
                        userSt.Add(busLine, busSt);
                    }
                }
                else
                {
                    userSt = new Dictionary<string,BusLineStatistics>();
                    BusLineStatistics busSt = new BusLineStatistics() { Count = 1, EarlestBoardTime = BTime, LatestAlightTime = ATime };
                    UpdateTimeStatistics(BTime, ATime, busSt);
                    userSt.Add(busLine, busSt);
                    results.Add(userID, userSt);
                } 
            }
        }

        private void UpdateTimeStatistics(DateTime bTime, DateTime aTime, BusLineStatistics st)
        {
            int board = bTime.Hour;
            int alight = aTime.Hour;

            for (int i = board; i <= alight; i++)
            {
                st.TimeStatistics[i]++;
            }
        }
    }
}
