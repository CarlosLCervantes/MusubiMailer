using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DAL
{
    public class MusubiSQLRepository : IMusubiRepository
    {
        private MusubiMailerEntities _dbContext = null;

        public MusubiSQLRepository()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MusubiMailerEntities"].ConnectionString;
            //string connStr = @"metadata=res://*/MusubiDB.csdl|res://*/MusubiDB.ssdl|res://*/MusubiDB.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=SIN-OMEGA\SQLEXPRESS;initial catalog=MusubiMailer;integrated security=True;multipleactiveresultsets=True;App=EntityFramework&quot;";
            _dbContext = new MusubiMailerEntities(connStr);
        }

        public void InsertEmails(List<Email> emails, string groupName)
        {
            Group emailGroup = null;
            if (!String.IsNullOrEmpty(groupName))
            {
                emailGroup = new Group()
                {
                    Name = groupName
                };
                _dbContext.SaveChanges();
            }

            foreach(Email email in emails)
            {
                email.CreatedDate = DateTime.Now;
                email.EditedDate = DateTime.Now;
                //EmailGroup emailGroupJoin = new EmailGroup() { GroupID = emailGroup.ID };
                //email.EmailGroups.Add(emailGroupJoin);
                _dbContext.Emails.AddObject(email);
            }
            _dbContext.SaveChanges();
        }

        public void UpdateEmail(Email email)
        {
            _dbContext.SaveChanges();
        }

        public List<Email> GetEmails()
        {
            //_dbContext.Emails.Where(e => e.)
            return _dbContext.Emails.ToList();
        }

        public List<Account> GetAccounts()
        {
            return _dbContext.Accounts.Where(a => a.Active).ToList();
        }

        public Account InsertAccount(Account account)
        {
            account.CreateDate = DateTime.Now;
            account.Active = true;
            _dbContext.Accounts.AddObject(account);
            _dbContext.SaveChanges();

            return account;
        }

        public AccountActivity UpdateAccountActivity(Account account, int activityCount)
        {
            AccountActivity activity = new AccountActivity();
            activity.Date = DateTime.Now;
            activity.EmailSendCount = activity.EmailSendCount + activityCount;
            account.AccountActivities.Add(activity);
            _dbContext.SaveChanges();

            return activity;
        }

        #region *************************Campaigns*************************


        public List<Campaign> GetCampaigns()
        {
            return _dbContext.Campaigns.Where(c => c.Active).ToList();
        }

        public Campaign InsertCampaign(Campaign campaign) 
        {
            campaign.Active = true;
            campaign.CreateDate = DateTime.Now;

            _dbContext.Campaigns.AddObject(campaign);
            _dbContext.SaveChanges();

            return campaign;
        }

        public Campaign AddAccountsToCampaign(Campaign campaign, List<Account> accounts)
        {
            foreach(Account a in accounts)
            {
                AccountCampaign accountCampaign = new AccountCampaign();
                accountCampaign.AccountID = a.ID;
                accountCampaign.CampaignID = campaign.ID;
                //accountCampaign.Account = a;
                //accountCampaign.Campaign = campaign;
                accountCampaign.ID = (_dbContext.AccountCampaigns.Max(ac => ac.ID) + 1); //TODO: Fix
                _dbContext.AccountCampaigns.AddObject(accountCampaign);
                _dbContext.SaveChanges();
            }
            return campaign;
        }

        public Campaign AddEmailsToCampaign(Campaign campaign, List<Email> emails)
        {
            foreach (Email e in emails)
            {
                EmailCampaign ec = new EmailCampaign();
                ec.EmailID = e.ID;
                ec.CampaignID = campaign.ID;
                ec.LastSendDate = DateTime.Now;
                ec.CreateDate = DateTime.Now;
                _dbContext.EmailCampaigns.AddObject(ec);
                _dbContext.SaveChanges();
            }
            return campaign;
        }


        #endregion


    }
}
