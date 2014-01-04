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
using DAL;

namespace MusubiMailer.Pages
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class Accounts : Page
    {
        IMusubiRepository _musubiRepo = new MusubiSQLRepository();

        public Accounts()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bindAccounts();
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            Account newAccount = new Account();
            newAccount.TypeID = Convert.ToInt32(cmbMailType.Tag);
            newAccount.UserName = txtUsername.Text;
            newAccount.Password = txtPassword.Text;
            _musubiRepo.InsertAccount(newAccount);

            bindAccounts();
        }

        private void bindAccounts()
        {
            var accounts = _musubiRepo.GetAccounts();
            dgAccounts.ItemsSource = accounts;
            dgAccounts.DataContext = accounts;
        }

        private bool testNewAccount()
        {
            bool pass = true;
            //TODO: Add Test
            return pass;
        }
    }
}
