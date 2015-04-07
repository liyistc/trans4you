using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserBehaviorAnalyst
{
    class UserConfig
    {
        private DateTime _pBoardingTime;
        public DateTime pBoardingTime
        {
            get { return _pBoardingTime; }
            set { _pBoardingTime = value; }
        }
        private DateTime _pAlightTime;
        public DateTime pAlightTime
        {
            get { return _pAlightTime; }
            set
            {
                if (value.Subtract(_pBoardingTime).TotalMinutes < 5)
                {
                    _pAlightTime = _pBoardingTime.AddMinutes(5);
                }
                else
                {
                    _pAlightTime = value;
                }
            }
        }
        public string pBusLine
        {
            get;
            set;
        }
        public double possibility
        {
            get;
            set;
        }
        private int _errorInMinutes;
        public int errorInMinutes
        {
            get { return _errorInMinutes; }
            set 
            { 
                if (2 * value > _pAlightTime.Subtract(_pBoardingTime).TotalMinutes)
                {
                    _errorInMinutes = (int)_pAlightTime.Subtract(_pBoardingTime).TotalMinutes / 2; 
                } 
                else 
                { 
                    _errorInMinutes = value;
                } 
            }
        }
    }

    class DataGeneration
    {
        private int no_of_Users;
        private int no_of_BusLines;
        private int no_of_Records;
        private Random r;
        //private DataOperation DataOperation;
        private const int DEFAULT_ERROR_IN_MINUTES = 10;
        private const double DEFAULT_POSSIBILITY = 0.6;

        public Dictionary<int, UserConfig> userConfigBase
        {
            get;
            set;
        }
        public List<string> busLines
        {
            get;
            set;
        }

        public DataGeneration(int nUser, int nBus, int nRecord)
        {
            this.no_of_BusLines = nBus;
            this.no_of_Users = nUser;
            this.no_of_Records = nRecord;
            r = new Random();
            userConfigBase = new Dictionary<int, UserConfig>();
            busLines = new List<string>();

            for (int i = 0; i < no_of_Users; i++)
            {
                DateTime time = RandomTime();
                userConfigBase.Add(i, new UserConfig() { possibility = DEFAULT_POSSIBILITY, pBusLine = "Line" + r.Next(no_of_BusLines), pBoardingTime = time, pAlightTime = DerivedAlightTime(time, 120), errorInMinutes = r.Next(2,DEFAULT_ERROR_IN_MINUTES) });
            }
            for (int i = 0; i < no_of_BusLines; i++)
            {
                busLines.Add("Line" + i);
            }
        }

        private bool RandomEventOccours(double possibility)
        {
            if (r.NextDouble() <= possibility)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private DateTime RandomTime()
        {
            //Random Time on 15/1/2011
            return new DateTime(2011, 1, 15, r.Next(7,22), r.Next(59), r.Next(59));
        }

        private DateTime DerivedRandomTime(DateTime bTime, int spanInMinutes)
        {
            TimeSpan ts = new TimeSpan(0,r.Next(spanInMinutes),0);

            if (RandomEventOccours(0.5))
            {
                return bTime.Add(ts);
            }
            else
            {
                return bTime.Subtract(ts);
            }
        }

        private DateTime DerivedAlightTime(DateTime bTime, int spanInMinutes)
        {
            TimeSpan ts = new TimeSpan(0, r.Next(spanInMinutes), 0);

            return bTime.Add(ts);
        }

        private int RandomExcept(int max,int e)
        {
            int n = r.Next(max);
            while (n == e)
            {
                n = r.Next(max);
            }
            return n;
        }

        public void GenerateData()
        {
            DataOperation.OpenConnection();

            for (int i = 0; i < no_of_Records; i++)
            {
                int user = r.Next(no_of_Users);
                UserConfig uc;
                userConfigBase.TryGetValue(user, out uc);

                DateTime bTime;
                DateTime aTime;
                string bus;

                if (RandomEventOccours(uc.possibility))
                {
                    bTime = DerivedRandomTime(uc.pBoardingTime,uc.errorInMinutes);
                    aTime = DerivedRandomTime(uc.pAlightTime,uc.errorInMinutes);
                    bus = uc.pBusLine;
                }
                else
                {
                    bTime = RandomTime();
                    aTime = DerivedAlightTime(bTime,120);
                    //bus = "Line" + RandomExcept(no_of_BusLines,Convert.ToInt32(uc.pBusLine.Substring(4)));
                    bus = "Line" + r.Next(no_of_BusLines);
                }

                DataOperation.InsertData(new TripRecord() { userId = user, boardingTime = bTime, alightTime = aTime, busLine = bus });
            }

            DataOperation.CloseConnection();
        }
    }
}
