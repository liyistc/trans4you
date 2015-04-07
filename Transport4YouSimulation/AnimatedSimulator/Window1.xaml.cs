using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
//using InTheHand.Net.Sockets;
//using InTheHand.Net.Bluetooth;

namespace Transport4YouSimulation
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Image Map;
        private Point mouseDragStartPoint;
        private Point scrollStartOffset;
        private const double Lambda = 130;
        private List<Line> tempTrace = new List<Line>();
        private PATService PAT;

        public Window1()
        {
            InitializeComponent();
            InitMap();
            BusLineBase.InitBus("buslineconfig.txt",Carrier);
            StopBase.InitStopInfoControl("stopconfig.txt",Carrier);
            InitCombobox();
            this.PAT = new PATService();

            Server.Start();
        }

        private void InitCombobox()
        {
            startStop.ItemsSource = StopBase.StopTable.Keys;
            destStop.ItemsSource = StopBase.StopTable.Keys;
            selectedBus.ItemsSource = BusLineBase.BusLineTable.Keys;
            selectedUser.ItemsSource = PassengerBase.PassengerTable.Keys;
            t_cost.Text = "" + ServerConfig.GetT_COST();
            t_val.Text = "" + ServerConfig.GetT_VAL();
        }

        private void InitMap()
        {
            Map = new Image();
            Map.Source = new BitmapImage((new Uri(@"Img\Transport4YouMap.jpg", UriKind.Relative)));
            Map.Width = 2400;
            Map.Height = 1700;            
            Carrier.Children.Add(Map);
            Map.SetValue(Canvas.ZIndexProperty, -1);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            mouseDragStartPoint = e.GetPosition(this.ScrollBoard);
            scrollStartOffset.X = ScrollBoard.HorizontalOffset;
            scrollStartOffset.Y = ScrollBoard.VerticalOffset;

            // Update the cursor if scrolling is possible
            if (!this.Map.IsMouseDirectlyOver)
                return;

            this.Cursor = (ScrollBoard.ExtentWidth > ScrollBoard.ViewportWidth) ||
                (ScrollBoard.ExtentHeight > ScrollBoard.ViewportHeight) ?
                Cursors.Hand : Cursors.Arrow;

            this.CaptureMouse();
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                // Get the new mouse position. 
                Point mouseDragCurrentPoint = e.GetPosition(this.ScrollBoard);

                // Determine the new amount to scroll. 
                Point delta = new Point(
                    (mouseDragCurrentPoint.X > this.mouseDragStartPoint.X) ?
                    -(mouseDragCurrentPoint.X - this.mouseDragStartPoint.X) :
                    (this.mouseDragStartPoint.X - mouseDragCurrentPoint.X),
                    (mouseDragCurrentPoint.Y > this.mouseDragStartPoint.Y) ?
                    -(mouseDragCurrentPoint.Y - this.mouseDragStartPoint.Y) :
                    (this.mouseDragStartPoint.Y - mouseDragCurrentPoint.Y));

                // Scroll to the new position. 
                ScrollBoard.ScrollToHorizontalOffset(this.scrollStartOffset.X + delta.X);
                ScrollBoard.ScrollToVerticalOffset(this.scrollStartOffset.Y + delta.Y);
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.Cursor = Cursors.Arrow;
                this.ReleaseMouseCapture();
            }
            base.OnPreviewMouseUp(e);
        }

        //Plan route
        private void Plan_Click(object sender, RoutedEventArgs e)
        {
            if (startStop.SelectedIndex == -1 || destStop.SelectedIndex == -1)
            {
                MessageBox.Show("Please Select StartStop and Destination!");
                return;
            }
            ClearBoard();

            List<string> actionList = PAT.RoutePlan(StopBase.StopTable, BusLineBase.BusLineTable, (string)startStop.SelectedItem, (string)destStop.SelectedItem, cross.IsChecked.Value);
            string current_stop = "";
            string busline = "";
            string message = "";
            foreach (string action in actionList)
            {
                if (action[0].Equals('['))
                {
                    current_stop = action.Substring(18, action.IndexOf(")") - 18);
                }
                else if (action.Equals("cross"))
                {
                    message += "CrossRoad => ";
                    continue;
                }
                else
                {
                    busline = "Line" + action.Substring(8);
                    tempTrace.AddRange(BusLineBase.BusLineTable[busline].OneStopTrace(current_stop));
                    message += "TakeBus:" + busline + " at " + current_stop + " => ";
                }
            }
            MessageBox.Show(message);
        }

        private void ClearBoard()
        {
            //Clear Screen
            if (tempTrace != null)
            {
                foreach (Line trace in tempTrace)
                {
                    Carrier.Children.Remove(trace);
                }
                tempTrace.Clear();
            }
        }

        //Add new user
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (startStop.SelectedIndex == -1 || destStop.SelectedIndex == -1)
            {
                MessageBox.Show("Please Select StartStop and Destination!");
                return;
            }
            try
            {
                if (PassengerBase.PassengerTable.Keys.Contains<ulong>(Convert.ToUInt16(userid.Text)))
                {
                    MessageBox.Show("The Passenger Already Exists!");
                    return;
                }
                PassengerBase.PassengerTable.Add(Convert.ToUInt64(userid.Text),
                    new Passenger(Convert.ToUInt64(userid.Text), StopBase.StopTable[(string)startStop.SelectedValue], StopBase.StopTable[(string)destStop.SelectedValue],
                        PAT.RoutePlan(StopBase.StopTable, BusLineBase.BusLineTable, (string)startStop.SelectedItem, (string)destStop.SelectedItem, false)));
                selectedUser.Items.Refresh();
                MessageBox.Show("Passenger: " + userid.Text + " is added successfully!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Please Enter Correct User ID!");
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Server.Stop();
            GC.Collect();
            Environment.Exit(0);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearBoard();
                if (t_cost.IsEnabled && t_val.IsEnabled)
                {
                    ServerConfig.ModifyT_COST(Convert.ToUInt16(t_cost.Text));
                    ServerConfig.ModifyT_VAL(Convert.ToUInt16(t_val.Text));
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Illegal Parameters!");
                return;
            }

            //if (!pause_button.IsEnabled)
            //{
                foreach (Bus b in BusLineBase.BusLineTable.Values)
                {
                    if (!b.IsStarted)
                    {
                        b.Start();
                    }
                    else
                    {
                        b.BusResume();
                    }
                }
                pause_button.IsEnabled = true;
            //}
            //else
            //{
            //    foreach (Bus b in BusLineBase.BusLineTable.Values)
            //    {
            //        b.BusResume();
            //    }
            //}
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            foreach (Bus b in BusLineBase.BusLineTable.Values)
            {
                b.BusPause();
            }
        }

        private void selectedBus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            busStatus.DataContext = BusLineBase.BusLineTable[(string)selectedBus.SelectedItem];
        }

        private void selectedUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            userStatus.DataContext = PassengerBase.PassengerTable[(ulong)selectedUser.SelectedItem];
            userInfoGrid.DataContext = PassengerBase.PassengerTable[(ulong)selectedUser.SelectedItem];
        }

        private void server_button_Click(object sender, RoutedEventArgs e)
        {
            //Pause all buses
            foreach (Bus b in BusLineBase.BusLineTable.Values)
            {
                b.BusPause();
            }

            ServerWindow sw = new ServerWindow(Carrier);
            sw.ShowDialog();
        }

        private void user_button_Click(object sender, RoutedEventArgs e)
        {
            UserWindow uw = new UserWindow();
            uw.ShowDialog();
        }
    }
    
    //public void DetectDevice()
    //{
    //    DeviceBox.Text = "";
    //    BluetoothDeviceInfo[] devices = bClient.DiscoverDevices();
    //    foreach (BluetoothDeviceInfo bi in devices){
    //        DeviceBox.Text += (bi.DeviceName + ": " + bi.DeviceAddress +"\n");
    //    }
    //}
}