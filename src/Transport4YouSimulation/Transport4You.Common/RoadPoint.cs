using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Transport4YouSimulation
{
    public class RoadPoint
    {
        public Point Coordinate
        {
            get;
            set;
        }

        public RoadPoint(int x, int y)
        {
            this.Coordinate = new Point(x, y);
        }
    }

    public class RoadStop : RoadPoint
    {
        public string StopName
        {
            get;
            set;
        }
        public List<String> BusLines
        {
            get;
            set;
        }
        public RoadStop Opposite
        {
            get;
            set;
        }
        public HashSet<Passenger> Passengers
        {
            get;
            set;
        }

        public RoadStop(int x, int y, String name)
            : base(x, y)
        {
            this.StopName = name;
            BusLines = new List<string>();            
            Passengers = new HashSet<Passenger>();
            Opposite = null;
        }

        public void AddBusLine(string lineName)
        {
            if (!BusLines.Contains(lineName))
            {
                this.BusLines.Add(lineName);
            }
        }

        public void RemoveBusLine(string lineName)
        {
            this.BusLines.Remove(lineName);
        }

        public string PassengerToString()
        {
            string output = "Passengers:{";
            foreach (Passenger p in Passengers)
            {
                output += p.CellPhoneAddress + ",";
            }
            return output + "}";
        }

        public string BusLineToString()
        {
            string output = "BusLines:{";
            foreach (string b in BusLines)
            {
                output += b + ",";
            }
            return output + "}";
        }
    }

    public class StopInfoControl
    {
        protected Button StopControl
        {
            get;
            set;
        }
        private Point StopControlPosition
        {
            get;
            set;
        }
        private RoadStop StopPoint
        {
            get;
            set;
        }

        public StopInfoControl(Point control, int angle, String name, Canvas container)
        {
            StopControlPosition = control;
            StopControl = new Button();
            StopControl.Margin = new Thickness(control.X, control.Y, 0, 0);
            if (angle != 0)
                StopControl.LayoutTransform = new RotateTransform(angle);
            Image icon = new Image();
            icon.Source = new BitmapImage(new Uri(@"Img/busstop.png", UriKind.Relative));
            StopControl.Content = icon;
            StopControl.ToolTip = name + "(" + control.X + "," + control.Y + ")";
            StopControl.Click += ButtonEventHandler;
            container.Children.Add(StopControl);
        }

        private void ButtonEventHandler(object sender, RoutedEventArgs e)
        {
            if (StopPoint != null)
            {
                MessageBox.Show(this.StopPoint.StopName + "\n" + StopPoint.BusLineToString() + "\n" + StopPoint.PassengerToString());
            }
            else
            {
                MessageBox.Show("Stop is Empty");
            }
        }

        public void AddRoadStop(RoadStop rs)
        {
            this.StopPoint = rs;
        }
    }

    public class TerminalControl : StopInfoControl
    {
        public TerminalControl(Point control, int angle, String name, Canvas container)
            : base(control, angle, name, container)
        {
            base.StopControl.Width = 50;
            base.StopControl.Height = 50;
        }
    }

    public class StopControl : StopInfoControl
    {
        public StopControl(Point control, int angle, String name, Canvas container)
            : base(control, angle, name, container)
        {
            base.StopControl.Width = 30;
            base.StopControl.Height = 30;
        }
    }
}
