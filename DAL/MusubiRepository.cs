using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL
{
    public interface IMusubiRepository
    {
        void InsertEmails(List<Email> emails, string groupName);
        List<Account> GetAccounts();
        Account InsertAccount(Account account);

        List<Campaign> GetCampaigns();
        Campaign InsertCampaign(Campaign campaign);
    }
}
