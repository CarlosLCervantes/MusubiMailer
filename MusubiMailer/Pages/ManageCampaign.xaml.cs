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
using EmailLib;
using EmailLib.TemplateModels;
using System.Diagnostics;

namespace MusubiMailer.Pages
{
    /// <summary>
    /// Interaction logic for ManageCampaign.xaml
    /// </summary>
    public partial class ManageCampaign : Page
    {
        MusubiSQLRepository _musubiRepo = new MusubiSQLRepository();
        private Campaign _campaign = null;
        public ManageCampaign(Campaign campaign)
        {
            InitializeComponent();
            _campaign = campaign;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Basic Binding
            lblCampaignTitle.Content = _campaign.Name;

            bindEmails();

            //Display - Accounts
            bindAccountDetails();
            
        }

        private void bindEmails()
        {
            //Bind Grid
            List<Email> emails = _campaign.EmailCampaigns.Select(ec => ec.Email).ToList();
            dgEmails.DataContext = emails;
            dgEmails.ItemsSource = emails;

            //Display - Email
            lblEmailsUsed.Content = _campaign.EmailCampaigns.Where(ec => ec.StatusID != 0).Count();
            lblEmailAvailableCount.Content = _campaign.EmailCampaigns.Where(ec => ec.StatusID == 0).Count();
            lblEmailsTotal.Content = _campaign.EmailCampaigns.Count();
        }

        private void bindAccountDetails()
        {
            List<Account> accounts = _campaign.AccountCampaigns.Select(ac => ac.Account).ToList();

            int accountsCount = accounts.Count();
            lblAccountsCount.Content = accountsCount;

            decimal emailsSent = 0;
            decimal emailsCap = 0;
            foreach(Account acc in accounts)
            {
                var activities = acc.AccountActivities.Where(ac => ac.Date > DateTime.Now.AddHours(-24));
                decimal sum = activities.Sum(ac => ac.EmailSendCount);
                emailsSent += sum;
                if (acc.TypeID == 0)
                {
                    emailsCap += 500;
                }
            }
            lblEmailAvailableCount.Content = (emailsCap - emailsSent);
        }

        private int getEmailAvailableCount(Account acc)
        {
            decimal emailsSent = 0;
            decimal emailsCap = 0;

            var activities = acc.AccountActivities.Where(ac => ac.Date > DateTime.Now.AddHours(-24));
            decimal sum = activities.Sum(ac => ac.EmailSendCount);
            emailsSent += sum;
            if (acc.TypeID == 0)
            {
                emailsCap += 500;
            }

            return (int)(emailsCap - emailsSent);
        }

        private void btnLoadAllAccounts_Click(object sender, RoutedEventArgs e)
        {
            var accounts = _musubiRepo.GetAccounts();
            _campaign = _musubiRepo.AddAccountsToCampaign(_campaign, accounts);

            bindAccountDetails();
        }

        private void btnLoadAllEmails_Click(object sender, RoutedEventArgs e)
        {
            var emails = _musubiRepo.GetEmails();
            _campaign = _musubiRepo.AddEmailsToCampaign(_campaign, emails);

            bindEmails();
        }

        private int getEmailsAvailableForCampaign()
        {
            int emailsAvailable = 0;
            foreach (AccountCampaign acc in _campaign.AccountCampaigns)
            {
                emailsAvailable += getEmailAvailableCount(acc.Account);
            }

            return emailsAvailable;
        }

        private void btnSendAll_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            int emailsAvailable = getEmailsAvailableForCampaign();
            List<EmailCampaign> emailsToSend = _campaign.EmailCampaigns.Take(emailsAvailable).ToList();
            
            int numAccounts = _campaign.AccountCampaigns.Count - 1;
            List<AccountCampaign> accountCampaigns = _campaign.AccountCampaigns.ToList();

            int emailsSent = 0;
            int accountCounter = 0;
            foreach (EmailCampaign emailToSend in emailsToSend)
            {
                AccountCampaign acc = accountCampaigns[accountCounter];
                if (accountCounter <= (numAccounts - 1)) { accountCounter++; }
                else { accountCounter = 0; }
                EmailLib.EmailManager emailManager = new EmailManager(getGoogleEmailConfig(acc.Account.UserName, acc.Account.Password));

                bool slipsumTrigger = (rand.Next(12) == 1);
                if (!slipsumTrigger)
                {
                    Invite emailInvite = new Invite() { FirstName = emailToSend.Email.FirstName };
                    try
                    {
                        //bool status = false;
                        bool status = emailManager.SendRandomInvite(emailToSend.Email.Address, acc.Account.UserName, emailInvite);
                        //if (emailsSent == 3) throw new Exception("derp");
                        if (status)
                        {
                            emailToSend.StatusID = (int)EmailStatusID.Sent;
                            _musubiRepo.UpdateEmail(emailToSend.Email); //TODO: FIX
                        }
                        else { log("FAILED TO SEND EMAIL"); }
                    }
                    catch (Exception ex)
                    {
                        accountCampaigns.Remove(acc);
                        numAccounts--;
                        log(String.Format("{0} failed", emailToSend.Email.Address));
                    }
                    log(String.Format("Sending REGULAR email to {0}, from {1}", emailToSend.Email.Address, acc.Account.UserName));
                    acc.Account.ActivityCount++;
                    emailsSent++;
                }
                else
                {
                    Slipsum slipsum = new Slipsum();
                    string[] slipsum_address = { "liquid90605@yahoo.com", "substitut3@gmail.com", "fcarelyrae900@gmail.com, test@mailinator.com"};
                    int slipsumIndex = rand.Next(slipsum_address.Length - 1);
                    string slipsumTo = slipsum_address[slipsumIndex];

                    //emailManager.SendSlipsum(slipsumTo, acc.Account.UserName, slipsum.Generate(1), slipsum.Generate(5));
                    log(String.Format("Sending Slipsum email to {0}, from {1}", slipsumTo, acc.Account.UserName));
                }
                
                log("Emails Sent = " + emailsSent);
                System.Threading.Thread.Sleep(1000 * 60 * 2);
            }

            foreach (AccountCampaign acc in _campaign.AccountCampaigns)
            {
                _musubiRepo.UpdateAccountActivity(acc.Account, acc.Account.ActivityCount);
            }
        }

        private void sendEmailsOld(List<EmailCampaign> emailsToSend)
        {
            int emailsSent = 0;
            foreach(AccountCampaign acc in _campaign.AccountCampaigns)
            {
                int emailsAvailable = getEmailAvailableCount(acc.Account);
                if (emailsAvailable <= 0) { break; }

                EmailLib.EmailManager emailManager = new EmailManager(getGoogleEmailConfig(acc.Account.UserName, acc.Account.Password));
                while (emailsAvailable > 0 && emailsToSend.Count > 0)
                {
                    EmailCampaign emailToSend = emailsToSend.First();
                    Invite emailInvite = new Invite() { FirstName = emailToSend.Email.FirstName };
                    bool status = emailManager.SendInvite(emailToSend.Email.Address, acc.Account.UserName, emailInvite);
                    if (status)
                    {
                        emailToSend.StatusID = (int)EmailStatusID.Sent;
                        _musubiRepo.UpdateEmail(emailToSend.Email); //TODO: FIX
                    }
                    emailsAvailable++;
                    emailsSent++;
                    log("Emails Sent = " + emailsSent);
                    _musubiRepo.UpdateAccountActivity(acc.Account, emailsSent);
                    emailsToSend.Remove(emailToSend);
                }
            }
        }

        private void log(string message)
        {
            Debugger.Log(1, "test", message + Environment.NewLine);
            tbConsole.Text = message;
        }

        private EmailConfig getGoogleEmailConfig(string user, string pass)
        {
            EmailConfig config = new EmailConfig();
            config.UserName = user;
            config.Password = pass;
            config.UseSSL = true;
            config.Port = 587;
            config.Address = "smtp.gmail.com";
            return config;
        }
    }


    public enum EmailStatusID { Unsent = 0, Sent = 1, Confirmed = 2, Bounced = 3, Unopened = 4 }
}
