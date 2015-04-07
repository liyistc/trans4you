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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UserBehaviorAnalyst
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        DataGeneration dg;
        BehaviorAnalysis ba;

        public Window1()
        {
            InitializeComponent();

            lstRecord.DataContext = DataOperation.RetrieveData("TripRecord");
        }

        private void DefineUserPreferences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dg = new DataGeneration(Convert.ToInt32(userCount.Text), Convert.ToInt32(busLineCount.Text), Convert.ToInt32(recordCount.Text));
            }
            catch (FormatException)
            {
                MessageBox.Show("Please Check the Format!");
                return;
            }

            userPreferences.IsEnabled = true;
            userIdSelect.DataContext = dg.userConfigBase.Keys;
            busLineSelect.DataContext = dg.busLines;

            defineButton.IsEnabled = false;
            generateButton.IsEnabled = true;
            analyseButton.IsEnabled = true;

            userCount.IsEnabled = false;
            busLineCount.IsEnabled = false;
            recordCount.IsEnabled = false;
        }

        private void userIdSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserConfig uc = dg.userConfigBase[(int)userIdSelect.SelectedItem];
            busLineSelect.SelectedItem = uc.pBusLine;
            ratio.DataContext = uc;
            bTime.Text = uc.pBoardingTime.ToShortTimeString();
            aTime.Text = uc.pAlightTime.ToShortTimeString();
            errorRange.Text = uc.errorInMinutes.ToString();
        }

        private void busLineSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserConfig uc = dg.userConfigBase[(int)userIdSelect.SelectedItem];
            if (uc.pBusLine != (string)busLineSelect.SelectedItem)
            {
                uc.pBusLine = (string)busLineSelect.SelectedItem;
            }
        }

        private void GenerateData_Click(object sender, RoutedEventArgs e)
        {
            dg.GenerateData();
            lstRecord.DataContext = DataOperation.RetrieveData("TripRecord");

            analyseButton.IsEnabled = true;
            resultTab.IsSelected = true;
        }

        private void ClearData_Click(object sender, RoutedEventArgs e)
        {
            DataOperation.ClearData();
            lstRecord.DataContext = DataOperation.RetrieveData("TripRecord");

            analyseButton.IsEnabled = false;
        }

        private void analyseButton_Click(object sender, RoutedEventArgs e)
        {
            ba = new BehaviorAnalysis();
            ba.Analyse();
            resultUserList.DataContext = ba.results.Keys;
            analyseTab.IsSelected = true;
        }

        private void resultUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<string, BusLineStatistics> st;
            if (resultUserList.SelectedIndex != -1 && ba.results.TryGetValue((int)resultUserList.SelectedItem, out st))
            {
                chart.DataContext = st;
            }
            else
            {
                return;
            }
        }

        private void pieChart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pieChart.SelectedItem == null)
            {
                timeInterval.Text = "";
            }
            else
            {
                KeyValuePair<string, BusLineStatistics> st = (KeyValuePair<string, BusLineStatistics>)pieChart.SelectedItem;
                timeInterval.Text = st.Key + " From: " + st.Value.EarlestBoardTime.ToShortTimeString() + " to " + st.Value.LatestAlightTime.ToShortTimeString();

                chart2.DataContext = st.Value.TimeStatistics; 
                popup.IsOpen = true;
            }
        }
    }
}
