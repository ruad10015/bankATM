using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using WpfApp11.DataAccess;
using WpfApp11.Entities;

namespace WpfApp11
{
    public partial class MainWindow : Window
    {
        private readonly MyContext _context;
        private decimal selectedAmount;
        private Mutex Send_Mutex = new Mutex();
        private const int Send_CooldownSeconds = 5;
        private DateTime lastSended_Time = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            _context = new MyContext();
        }

        private void InsertCard_Click(object sender, RoutedEventArgs e)
        {
            long cardNumberr;
            if (!long.TryParse(cardNumber.Text, out cardNumberr))
            {
                MessageBox.Show("Please enter a valid card number.");
                return;
            }

            Account user = _context.Accounts.FirstOrDefault(a => a.CardNumber == cardNumberr);

            if (user != null)
            {
                Fullname.Content = user.Fullname;
                balance.Content = user.Balance;
            }
            else
            {
                Fullname.Content = "User cannot found";
            }

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            long cardNumbers;
            if (!long.TryParse(cardNumber.Text, out cardNumbers))
            {
                MessageBox.Show("Please enter a valid card number.", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Account user = _context.Accounts.FirstOrDefault(a => a.CardNumber == cardNumbers);

            if (user != null)
            {
                MessageBox.Show($"User: {user.Fullname} - Balance: {user.Balance}$", "About user", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("User cannot found", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btn20azn_Click(object sender, RoutedEventArgs e)
        {
            selectedAmount = 20;
        }

        private void btn50azn_Click(object sender, RoutedEventArgs e)
        {
            selectedAmount = 50;
        }

        private void btn100azn_Click(object sender, RoutedEventArgs e)
        {
            selectedAmount = 100;
        }

        private void btn200azn_Click(object sender, RoutedEventArgs e)
        {
            selectedAmount = 200;
        }

        private void btn300azn_Click(object sender, RoutedEventArgs e)
        {
            selectedAmount = 300;
        }

        private void SendMoney_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cardNumber.Text))
            {
                MessageBox.Show("Please enter a card number", "Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(moneytxt.Text))
            {
                MessageBox.Show("Please enter an amount to Send", "Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal SendAmount;
            if (!decimal.TryParse(moneytxt.Text, out SendAmount))
            {
                MessageBox.Show("Please enter a valid amount to Send", "Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SendAmount <= 0)
            {
                MessageBox.Show("Please select an amount greater than zero to Send", "Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!CanSend())
            {
                MessageBox.Show($"Please wait {Send_CooldownSeconds} seconds before initiating another Send", "Send Cooldown", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Send_Mutex.WaitOne(TimeSpan.FromSeconds(1)))
            {
                MessageBox.Show("Send operation is currently in progress. Please try again later", "Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                lastSended_Time = DateTime.Now;

                MessageBox.Show($"Send amount: {SendAmount}", "Send Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                Send_Mutex.ReleaseMutex();
            }
        }

        private bool CanSend()
        {
            return (DateTime.Now - lastSended_Time).TotalSeconds >= Send_CooldownSeconds;
        }
    }
}
