using System;
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
using System.Windows.Shapes;

namespace Transport4YouSimulation
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private bool[] isRouteModified;
        private Canvas Carrier;

        public ServerWindow(Canvas Carrier)
        {
            InitializeComponent();

            this.Carrier = Carrier;

            accountTable.DataContext = AccountManager.UserBase.Values;
            busLineList.DataContext = BusLineBase.BusLineTable.Keys;
            stopsList.DataContext = StopBase.StopTable.Keys;
            userList.DataContext = TripManager.TripBase.Keys;

            isRouteModified = new bool[BusLineBase.BusLineTable.Keys.Count];
        }

        private void busLineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (busLineList.SelectedItem != null)
            {
                busRouteView.DataContext = BusLineBase.BusLineTable[(string)busLineList.SelectedItem];
            }
            else
            {
                busRouteView.DataContext = null;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Route.Insert(busRouteView.SelectedIndex, new RoadPoint(0,0));
            busRouteView.Items.Refresh();
            isRouteModified[busLineList.SelectedIndex] = true;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Route.Insert(busRouteView.SelectedIndex+1, new RoadPoint(0, 0));
            busRouteView.Items.Refresh();
            isRouteModified[busLineList.SelectedIndex] = true;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Route.RemoveAt(busRouteView.SelectedIndex);
            if (busRouteView.SelectedItem is RoadStop)
            {
                RoadStop temp = (RoadStop)busRouteView.SelectedItem;
                temp.RemoveBusLine((string)busLineList.SelectedItem);
            }
            busRouteView.Items.Refresh();
            isRouteModified[busLineList.SelectedIndex] = true;
        }

        private void AddStopAbove_Click(object sender, RoutedEventArgs e)
        {
             BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Route.Insert(busRouteView.SelectedIndex, new RoadStop((int)StopBase.StopTable[(string)stopsList.SelectedItem].Coordinate.X, (int)StopBase.StopTable[(string)stopsList.SelectedItem].Coordinate.Y, (string)stopsList.SelectedItem));
             StopBase.StopTable[(string)stopsList.SelectedItem].AddBusLine((string)busLineList.SelectedItem);
             busRouteView.Items.Refresh();
             isRouteModified[busLineList.SelectedIndex] = true;
        }

        private void AddStopBelow_Click(object sender, RoutedEventArgs e)
        {
            BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Route.Insert(busRouteView.SelectedIndex + 1, new RoadStop((int)StopBase.StopTable[(string)stopsList.SelectedItem].Coordinate.X, (int)StopBase.StopTable[(string)stopsList.SelectedItem].Coordinate.Y, (string)stopsList.SelectedItem));
            StopBase.StopTable[(string)stopsList.SelectedItem].AddBusLine((string)busLineList.SelectedItem);
            busRouteView.Items.Refresh();
            isRouteModified[busLineList.SelectedIndex] = true;
        }

        private void DeleteStop_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Click_2(sender, e);
        }

        private void userList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tripTable.DataContext = TripManager.TripBase[(ulong)userList.SelectedItem].tripRecords;
        }

        private void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            BusLineBase.BusLineTable[(string)busLineList.SelectedItem].Reset();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < BusLineBase.BusLineTable.Count; i++)
            {
                if (isRouteModified[i])
                {
                    BusLineBase.BusLineTable.Values.ElementAt<Bus>(i).Reset();
                }
            }

        }

        private void LoadRouteData_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "NewRoute"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                for (int i = 0; i < BusLineBase.BusLineTable.Count; i++)
                {
                    BusLineBase.BusLineTable.Values.ElementAt<Bus>(i).Dispose();
                }

                BusLineBase.InitBus(filename, Carrier);
                StopBase.InitStopInfoControl("stopconfig.txt", Carrier);

                busLineList.DataContext = null;
                busLineList.DataContext = BusLineBase.BusLineTable.Keys;
                stopsList.DataContext = null;
                stopsList.DataContext = StopBase.StopTable.Keys;
            }
        }

        private void LoadAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutwin = new About();
            aboutwin.ShowDialog();
        }
    }

    [ValueConversion(typeof(RoadPoint), typeof(bool))]
    public class RoadPointTypeConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is RoadStop);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
