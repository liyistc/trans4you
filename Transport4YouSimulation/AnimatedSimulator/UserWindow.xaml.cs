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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();

            userList.DataContext = AccountManager.UserBase.Keys;
        }

        private void userList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userList.SelectedIndex != -1)
            {
                UserInfo ui = AccountManager.UserBase[(ulong)userList.SelectedItem];

                hasTicket.IsChecked = ui.hasTicket;
                phoneNo.Text = ui.cellPhoneNumber;
                name.Text = ui.name;
                issueTime.Content = ui.ticketBeginTime;
                balance.Text = ui.balance.ToString();
                credit.IsChecked = ui.isPayByCreditCard;
                cardNo.Text = ui.creditCardNumber;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)register.IsChecked)
            {
                try
                {
                    UserInfo ui = new UserInfo();
                    ui.balance = Convert.ToInt32(balance.Text);
                    ui.cellPhoneAddr = Convert.ToUInt32(userID.Text);
                    ui.cellPhoneNumber = phoneNo.Text;
                    ui.creditCardNumber = cardNo.Text;
                    ui.hasTicket = false;
                    ui.isPayByCreditCard = (bool)credit.IsChecked;
                    ui.name = name.Text;
                    ui.ticketBeginTime = DateTime.Now;

                    if (AccountManager.AddUser(ui.cellPhoneAddr, ui))
                    {
                        userList.Items.Refresh();
                        MessageBox.Show("User Registration Successful!");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Illegal Registration Information!");
                        return;
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please Check Parameter Format!");
                    return;
                }
            }
            else if ((bool)login.IsChecked)
            {
                if (userList.SelectedIndex == -1)
                {
                    return;
                }
                try
                {
                    UserInfo ui = new UserInfo();
                    ui.ticketBeginTime = (DateTime)issueTime.Content;
                    ui.balance = Convert.ToInt32(balance.Text);
                    ui.cellPhoneNumber = phoneNo.Text;
                    ui.creditCardNumber = cardNo.Text;
                    ui.isPayByCreditCard = (bool)credit.IsChecked;
                    ui.cellPhoneAddr = Convert.ToUInt32(userList.SelectedItem);
                    ui.name = name.Text;
                    ui.hasTicket = (bool)hasTicket.IsChecked;
                    AccountManager.ModifyUser(ui);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please Check Parameter Format!");
                    return;
                }
            }
            else { return; }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void register_Checked(object sender, RoutedEventArgs e)
        {
            userList.SelectedIndex = -1;

            hasTicket.IsChecked = false;
            phoneNo.Text = "";
            name.Text = "";
            issueTime.Content = "";
            balance.Text = "";
            credit.IsChecked = false;
            cardNo.Text = "";
        }
    }
}
