using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace Transport4YouSimulation
{
    [Serializable]
    public struct BusReportMsg
    {
        public DateTime time;
        public string busLine;
        public string stopName;
        public bool isBoard;
        public HashSet<ulong> cellPhoneAddrSet;
        public uint busId;
    }

    public class UserInfo
    {
        public ulong cellPhoneAddr
        {
            set;
            get;
        }
        public string name
        {
            set;
            get;
        }
        public string cellPhoneNumber
        {
            set;
            get;
        }
        public bool hasTicket
        {
            set;
            get;
        }
        public DateTime ticketBeginTime
        {
            set;
            get;
        }
        public bool isPayByCreditCard
        {
            set;
            get;
        }
        public string creditCardNumber
        {
            set;
            get;
        }
        public int balance
        {
            set;
            get;
        }
    }

    public enum ServiceType
    {
        //a group of constant to define services types
        //public static readonly string SERVICE_BUSREPORT = "SERVICE_BUSREPORT";
        SERVICE_BUSREPORT,
        SERVICE_ROADCOND,
        SEVICE_REGISTRATION,
    }

    [Serializable]
    public struct RequestInfo
    {
        //indicate waht the request is for
        public ServiceType servicetype;
        //contain the content of the request
        public object Content;
    }

    public struct TripRecord
    {
        public DateTime time
        {
            get;
            set;
        }
        public string busLine
        {
            get;
            set;
        }
        public bool isBoard
        {
            get;
            set;
        }
        public string stopName
        {
            get;
            set;
        }
        public uint busId;
    }

    public struct TripInfo
    {
        public ulong userCellPhoneAddr;
        public List<TripRecord> tripRecords;
    }

    public static class PassengerBase
    {
        public static Dictionary<ulong, Passenger> PassengerTable
        {
            get { return passengers; }
            set { passengers = value; }
        }
        private static Dictionary<ulong, Passenger> passengers = new Dictionary<ulong,Passenger>();
    }

    public static class BusLineBase
    {
        public static Brush[] colors = new Brush[] { Brushes.Red, Brushes.Blue, Brushes.Chocolate, Brushes.Green, Brushes.Black, Brushes.Brown, Brushes.Purple };
        private const double Lambda = 130;

        public static Dictionary<string, Bus> BusLineTable
        {
            get { return buslines; }
            set { buslines = value; }
        }
        private static Dictionary<string, Bus> buslines = new Dictionary<string, Bus>();

        public static void InitBus(string buslineconfig,Canvas Carrier)
        {
            StopBase.StopTable.Clear();
            BusLineBase.BusLineTable.Clear();
            TextReader tr = new StreamReader(buslineconfig);
            String line = tr.ReadLine();

            int lineNo = 0;
            while (line != null)
            {
                List<RoadPoint> route = new List<RoadPoint>();
                string linename = line.Substring(0, line.IndexOf(":"));
                line = line.Substring(line.IndexOf(":") + 1);
                string[] points = line.Split(';');
                for (int i = 0; i < points.Length; i++)
                {
                    string point = points[i];
                    if (!point[0].Equals('('))
                    {
                        string stopname = point.Substring(0, point.IndexOf("("));
                        RoadStop rs = null;
                        if (!StopBase.StopTable.TryGetValue(stopname, out rs))
                        {
                            int x = Convert.ToInt32(point.Substring(point.IndexOf("(") + 1, point.IndexOf(",") - point.IndexOf("(") - 1));
                            int y = Convert.ToInt32(point.Substring(point.IndexOf(",") + 1, point.IndexOf(")") - point.IndexOf(",") - 1));

                            rs = new RoadStop(x, y, stopname);
                            StopBase.StopTable.Add(stopname, rs);
                        }
                        if (i + 1 != points.Length)
                            rs.AddBusLine(linename);
                        route.Add(rs);
                    }
                    else
                    {
                        int x = Convert.ToInt32(point.Substring(point.IndexOf("(") + 1, point.IndexOf(",") - point.IndexOf("(") - 1));
                        int y = Convert.ToInt32(point.Substring(point.IndexOf(",") + 1, point.IndexOf(")") - point.IndexOf(",") - 1));
                        RoadPoint rp = new RoadPoint(x, y);
                        route.Add(rp);
                    }
                }
                BusLineBase.BusLineTable.Add(linename, new Bus(route, linename, Carrier, colors[lineNo / 2]));
                line = tr.ReadLine();
                lineNo++;

                //break;
            }

            //Calculate Stops that are opposite to each other
            for (int i = 0; i < StopBase.StopTable.Values.Count; i++)
            {
                RoadStop rs1 = StopBase.StopTable.Values.ElementAt<RoadStop>(i);
                for (int j = i + 1; j < StopBase.StopTable.Values.Count; j++)
                {
                    RoadStop rs2 = StopBase.StopTable.Values.ElementAt<RoadStop>(j);
                    if (isClose(rs1.Coordinate, rs2.Coordinate))
                    {
                        rs1.Opposite = rs2;
                        rs2.Opposite = rs1;
                        break;
                    }
                }
            }
        }

        //Check if two points are close enough
        private static bool isClose(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2)) < Lambda;
        }
    }

    public static class StopBase
    {
        public static Dictionary<string, RoadStop> StopTable
        {
            get { return stops; }
            set { stops = value; }
        }
        private static Dictionary<string, RoadStop> stops = new Dictionary<string, RoadStop>();

        public static void InitStopInfoControl(string stopconfig, Canvas Carrier)
        {
            TextReader tr = new StreamReader(stopconfig);
            String line = tr.ReadLine();
            int stopID = 1;
            while (line != null)
            {
                StopInfoControl control;
                int x = Convert.ToInt32(line.Substring(line.IndexOf("(") + 1, line.IndexOf(",") - line.IndexOf("(") - 1));
                int y = Convert.ToInt32(line.Substring(line.IndexOf(",") + 1, line.IndexOf(")") - line.IndexOf(",") - 1));
                int angle = Convert.ToInt32(line.Substring(line.IndexOf("[") + 1, line.IndexOf("]") - line.IndexOf("[") - 1));
                String name = line.Substring(line.IndexOf("{") + 1, line.IndexOf("}") - line.IndexOf("{") - 1);
                if (stopID <= 4)
                    control = new TerminalControl(new Point(x, y), angle, name, Carrier);
                else
                {
                    control = new StopControl(new Point(x, y), angle, name, Carrier);
                }

                RoadStop rs = null;
                StopBase.StopTable.TryGetValue(name, out rs);
                control.AddRoadStop(rs);

                stopID++;
                line = tr.ReadLine();
            }
        }
    }

    //public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    //{
    //    public event NotifyCollectionChangedEventHandler CollectionChanged;
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    new public void Add(TKey k, TValue v)
    //    {
    //        base.Add(k, v);

    //        if (CollectionChanged != null)
    //        {
    //            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //        }
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs("Keys"));
    //            PropertyChanged(this, new PropertyChangedEventArgs("Values"));
    //        }
    //    }
    //}

    //public class ObservableList<T> : List<T>, INotifyCollectionChanged
    //{
    //    private Dictionary<NotifyCollectionChangedEventHandler, Dispatcher> collectionChangedHandlers;
    //    public event NotifyCollectionChangedEventHandler CollectionChanged
    //    {
    //        add
    //        {
    //            Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
    //            collectionChangedHandlers.Add(value, dispatcher);
    //        }
    //        remove
    //        {
    //            collectionChangedHandlers.Remove(value);
    //        }
    //    }

    //    new public void Add(T item)
    //    {
    //        base.Add(item);

    //        if (CollectionChanged != null)
    //        {
    //            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //        }
    //    }
    //}
}