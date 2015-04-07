using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace Transport4YouSimulation
{
    public class Bus : INotifyPropertyChanged
    {
        private const int SPEED = 40;
        private Thread AtBusStop;
        private Point[] points;
        private Image BusImage;
        private static Random rn = new Random();
        private Storyboard pasb;
        private List<Point[]> traces;
        private int localid = 0;
        private delegate void UpdateBusInfo();
        private BusNetComm netComm;
        private static Object thisLock = new Object();
        private ManualResetEvent mre = new ManualResetEvent(true);
        private bool IsPaused = false;
        private bool MouseEventPause = false;
        private List<Line> tempTrace = new List<Line>();        

        private List<RoadPoint> _route;
        public List<RoadPoint> Route
        {
            get { return _route; }
            set { _route = value; }
        }

        public bool IsStarted
        {
            get;
            set;
        }

        public List<string> Stops
        {
            get;
            set;
        }
        public string BusLineName
        {
            get;
            set;
        }

        private Brush BusColor
        {
            get;
            set;
        }
        public int BusLineNo
        {
            get { return Convert.ToInt32(BusLineName.Substring(4)); }
        }

        public HashSet<Passenger> OnBoard
        {
            get;
            set;
        }

        private string _onBoardDesc;
        public string OnBoardDesc
        {
            get { return _onBoardDesc; }
            set
            {
                _onBoardDesc = value;
                OnPropertyChanged("OnBoardDesc");
            }
        }

        private Canvas Parent;

        public Bus(List<RoadPoint> route, String name, Canvas parent, Brush color)
        {
            this.Route = route;
            this.BusLineName = name;
            this.OnBoard = new HashSet<Passenger>();
            this.Stops = new List<string>();
            this.Parent = parent;
            this.BusColor = color;   
            this.traces = new List<Point[]>();
            this.netComm = new BusNetComm();
            this.pasb = new Storyboard();

            BusImage = new Image();
            BusImage.Source = new BitmapImage((new Uri(@"Img\bus.bmp", UriKind.Relative)));
            BusImage.Height = 20;
            BusImage.Width = 20;
            BusImage.ToolTip = this.BusLineName;
            BusImage.MouseEnter += MouseOverEventHandler;
            BusImage.MouseLeave += MouseLeaveEventHandler;
            this.Parent.Children.Add(this.BusImage);

            int i = 0;
            this.points = new Point[Route.Count];
            foreach (RoadPoint rd in Route)
            {
                if (rd is RoadStop)
                {
                    RoadStop rs = (RoadStop)rd;
                    Stops.Add(rs.StopName);
                }
                points[i] = rd.Coordinate;
                i++;
            }
        }

        //Start New Animation
        public void Start()
        {
            ResetStopsAndPoints();

            IniBusLineAnimation();

            AtBusStop = new Thread(AtBusStopActivity);
            AtBusStop.Name = "AtBusStopThread";
            AtBusStop.Start();
            this.IsStarted = true;
            this.IsPaused = false;
        }

        private void ResetStopsAndPoints()
        {
            int i = 0;
            Stops.Clear();
            this.points = new Point[Route.Count];

            foreach (RoadPoint rd in Route)
            {
                if (rd is RoadStop)
                {
                    RoadStop rs = (RoadStop)rd;
                    Stops.Add(rs.StopName);
                }
                points[i] = rd.Coordinate;
                i++;
            }
        }

        //Reset Animation
        public void Reset()
        {
            if (Parent.FindName("MatrixTransform" + BusLineNo) != null)
            {
                Parent.UnregisterName("MatrixTransform" + BusLineNo);
            }
            if (AtBusStop != null)
            {   
                if (AtBusStop.IsAlive)
                {
                    AtBusStop.Abort();
                }
                pasb.Stop(Parent);

                localid = 0;
                mre.Set();
                //Start();

                //if (IsPaused)
                //{
                //    IsPaused = false;
                //    BusPause();
                //}
            }
            else
            {
                ResetStopsAndPoints();
                return;
            }
            this.IsStarted = false;
            this.IsPaused = false;
        }

        //Dispose Animation
        public void Dispose()
        {
            Reset();
            this.Parent.Children.Remove(BusImage);
            System.GC.SuppressFinalize(this);
        }

        //Split the path
        private void IniBusLineAnimation()
        {
            bool flag = false;
            List<Point> tempList = new List<Point>();
            traces.Clear();

            for (int i = 0; i < Route.Count; i++)
            {
                RoadPoint rp = Route.ElementAt<RoadPoint>(i);
                if (rp is RoadStop)
                {
                    if (flag)
                    {
                        tempList.Add(rp.Coordinate);
                        traces.Add(tempList.ToArray());
                        
                        if (i != Route.Count - 1)
                        {
                            tempList = new List<Point>();
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                tempList.Add(rp.Coordinate);
            }
        }

        //Convert points[] to storyboard
        private Storyboard CreateStoryBoard(Point[] points)
        {
            MatrixTransform buttonMatrixTransform = new MatrixTransform();
            BusImage.RenderTransform = buttonMatrixTransform;
            Parent.RegisterName("MatrixTransform" + BusLineNo, buttonMatrixTransform);

            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();
            pFigure.StartPoint = points[0];
            PolyLineSegment pLineSegment = new PolyLineSegment();

            for (int i = 1; i < points.Length; i++)
            {
                pLineSegment.Points.Add(points[i]);
            }
            pFigure.Segments.Add(pLineSegment);
            animationPath.Figures.Add(pFigure);

            animationPath.Freeze();

            MatrixAnimationUsingPath matrixAnimation = new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(Duration(SPEED,points));

            matrixAnimation.DoesRotateWithTangent = true;
            Storyboard.SetTargetName(matrixAnimation, "MatrixTransform" + BusLineNo);
            Storyboard.SetTargetProperty(matrixAnimation, new PropertyPath(MatrixTransform.MatrixProperty));

            this.pasb = new Storyboard();
            this.pasb.Children.Add(matrixAnimation);
            this.pasb.Completed += new EventHandler(nextStop);

            return pasb;
        }

        //The storyboard complete event handler
        private void nextStop(object sender, EventArgs e)
        {
            //At a bus stop:
            Parent.UnregisterName("MatrixTransform" + BusLineNo);
            AtBusStop = new Thread(AtBusStopActivity);
            AtBusStop.Name = "AtBusStopThread";
            AtBusStop.Start();
        }

        //Thread: Update Passenger and BusLine info
        private void AtBusStopActivity()
        {
            RoadStop currentStop = StopBase.StopTable[Stops[localid % Stops.Count]];
            lock (currentStop)
            {
                //Passengers alight---------------------------------  
                //Bus Report
                BusReportMsg alightMessage = new BusReportMsg();
                alightMessage.busLine = this.BusLineName;
                alightMessage.isBoard = false;
                alightMessage.stopName = currentStop.StopName;
                alightMessage.time = System.DateTime.Now;
                alightMessage.busId = (uint)BusLineNo;
                HashSet<ulong> alightAddresses = new HashSet<ulong>();

                //Terminal Stop
                if (localid % Stops.Count == Stops.Count - 1)
                {
                    foreach (Passenger p in this.OnBoard)
                    {
                        currentStop.Passengers.Add(p);
                    }
                    foreach (Passenger p in this.OnBoard)
                    {
                        alightAddresses.Add(p.CellPhoneAddress);
                    }
                    this.OnBoard.Clear();             
                }
                //Other Stops
                else
                {
                    List<Passenger> removeList = new List<Passenger>();
                    foreach (Passenger p in this.OnBoard)
                    {
                        if (!p.NextAction().Equals(this.BusLineName))
                        {
                            removeList.Add(p);
                            currentStop.Passengers.Add(p);
                        }
                    }
                    foreach (Passenger p in removeList)
                    {
                        alightAddresses.Add(p.CellPhoneAddress);
                        //Remove on board
                        this.OnBoard.Remove(p);
                    }
                }
                alightMessage.cellPhoneAddrSet = alightAddresses;
                if (alightMessage.cellPhoneAddrSet.Count != 0)
                {
                    netComm.SendMsg(alightMessage);
                }

                //Pick up passengers---------------------------------
                List<Passenger> removeList2 = new List<Passenger>();
                BusReportMsg boardMessage = new BusReportMsg();
                boardMessage.busLine = this.BusLineName;
                boardMessage.isBoard = true;
                boardMessage.stopName = currentStop.StopName;
                boardMessage.time = System.DateTime.Now;
                boardMessage.busId = (uint)BusLineNo;
                HashSet<ulong> boardAddresses = new HashSet<ulong>();

                foreach (Passenger p in currentStop.Passengers)
                {
                    if (p.NextAction().Equals(this.BusLineName))
                    {
                        this.OnBoard.Add(p);
                        boardAddresses.Add(p.CellPhoneAddress);
                        removeList2.Add(p);
                    }
                }
                foreach (Passenger p in removeList2)
                {
                    currentStop.Passengers.Remove(p);
                }

                //Bus Report
                boardMessage.cellPhoneAddrSet = boardAddresses;
                if (boardMessage.cellPhoneAddrSet.Count != 0)
                {
                    netComm.SendMsg(boardMessage);
                }

                OnBoardDesc = GetOnBoardDesc();
            }

            //Spend some time at the bus stop
            Thread.Sleep(rn.Next(1000, 6000));
            
            mre.WaitOne();
            Parent.Dispatcher.Invoke(new UpdateBusInfo(UpdateBusInfoDelegate));
        }

        private void UpdateBusInfoDelegate()
        {
            if (localid % Stops.Count != Stops.Count - 1)
            {
                //Move to the next stop:
                foreach (Passenger p in this.OnBoard)
                {
                    p.TakeBusOneStop();
                }

                lock (thisLock)
                {
                    CreateStoryBoard(traces.ElementAt<Point[]>(localid % (traces.Count + 1))).Begin(Parent,true);
                }
                
                localid++;
            }
            else
            {
                localid++;
                AtBusStop = new Thread(AtBusStopActivity);
                AtBusStop.Start();
            }
        }

        private int Duration(int speed, Point[] points)
        {
            int distance = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                distance += (int)Math.Sqrt(Math.Pow(points[i + 1].X - points[i].X, 2) + Math.Pow(points[i + 1].Y - points[i].Y, 2));
            }
            return distance / speed;
        }

        public void BusPause()
        {
            if (AtBusStop != null && !IsPaused)
            {
                if (AtBusStop.IsAlive)
                {
                    mre.Reset();
                }
                pasb.Pause(Parent);
                IsPaused = true;
            }
        }
        public void BusResume()
        {
            if (AtBusStop != null && IsPaused)
            {
                if (AtBusStop.IsAlive)
                {
                    mre.Set();
                }
                pasb.Resume(Parent);
                IsPaused = false;
            }
        }

        //Draw a trace and return the trace
        public List<Line> OneStopTrace(string start)
        {
            List<Point> tempList = new List<Point>();
            int index = Route.Count + 1;

            for (int i = 0; i < Route.Count; i++)
            {
                if (i > index)
                {
                    tempList.Add(Route.ElementAt<RoadPoint>(i).Coordinate);
                }
                if (Route.ElementAt<RoadPoint>(i) is RoadStop)
                {
                    if (i > index)
                        break;
                    RoadStop rs = (RoadStop)Route.ElementAt<RoadPoint>(i);
                    if (rs.StopName.Equals(start))
                    {
                        index = i;
                        tempList.Add(rs.Coordinate);
                    }
                }
            }
            return LineDrawing.DrawLine(tempList.ToArray(), this.Parent, this.BusColor);
        }

        //Draw Bus Line
        private void DrawBusLineTrace()
        {
            List<Point> tempList = new List<Point>();
            foreach (RoadPoint rp in Route)
            {
                tempList.Add(rp.Coordinate);
            }
            tempTrace = LineDrawing.DrawLine(tempList.ToArray(), this.Parent, this.BusColor);
        }

        //Remove Bus Line Drawing
        private void RemoveBusLineTrace()
        {
            if (tempTrace != null)
            {
                foreach (Line line in tempTrace)
                {
                    this.Parent.Children.Remove(line);
                }
                this.tempTrace.Clear();
            }
        }

        private void MouseOverEventHandler(object sender, RoutedEventArgs e)
        {
            if (!IsPaused)
            {
                MouseEventPause = true;
                Panel.SetZIndex(BusImage, 15);
                BusPause();
                DrawBusLineTrace();
            }
        }

        private void MouseLeaveEventHandler(object sender, RoutedEventArgs e)
        {
            if (MouseEventPause)
            {
                Panel.SetZIndex(BusImage, BusLineNo);
                BusResume();
                MouseEventPause = false;
                RemoveBusLineTrace();
            }
        }

        private string GetOnBoardDesc()
        {
            string output = "Passenger:{";
            foreach (Passenger p in OnBoard)
            {
                output += p.CellPhoneAddress + ",";
            }
            return output + "}";
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    //public class PassengerSet : HashSet<Passenger>, INotifyCollectionChanged
    //{
    //    new public bool Add(Passenger p)
    //    {
    //        if (CollectionChanged != null)
    //        {
    //            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //        }
    //        return base.Add(p);
    //    }

    //    new public bool Remove(Passenger p)
    //    {
    //        if (CollectionChanged != null)
    //        {
    //            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //        }
    //        return base.Remove(p);
    //    }

    //    new protected void Clear()
    //    {
    //        base.Clear();
    //        if (CollectionChanged != null)
    //        {
    //            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //        }
    //    }

    //    #region INotifyCollectionChanged Members

    //    public event NotifyCollectionChangedEventHandler CollectionChanged;

    //    #endregion
    //}

    class LineDrawing
    {
        Canvas parent;
        private static int n = 0;

        public LineDrawing(Canvas parent)
        {
            this.parent = parent;
        }

        public static List<Line> DrawLine(Point[] path, Canvas parent, Brush color)
        {
            List<Line> tempTrace = new List<Line>();
            for (int i = 0; i < path.Length - 1; i++)
            {
                Line trace = new Line();
                trace.Stroke = color;
                trace.X1 = path[i].X;
                trace.X2 = path[i + 1].X;
                trace.Y1 = path[i].Y;
                trace.Y2 = path[i + 1].Y;
                trace.StrokeThickness = 5;

                parent.Children.Add(trace);
                tempTrace.Add(trace);
            }
            n++;
            return tempTrace;
        }
    }
}
