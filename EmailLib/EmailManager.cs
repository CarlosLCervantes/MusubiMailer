using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using EmailLib.TemplateModels;
//using EmailLib.TemplateModels;

namespace EmailLib
{
    public class EmailManager
    {
        private string _xmlFolder = "";
        private SMTPEmail.SMTPEMailS _emailer = null;

        public EmailManager(string user, string password, string address, int? addressPort, bool? useSSL)
        {
            _emailer = new SMTPEmail.SMTPEMailS(null, address, user, password, addressPort, useSSL);
            if (String.IsNullOrEmpty(_xmlFolder))
                _xmlFolder = Directory.GetCurrentDirectory();
            _emailer.ThrowErrorOnSendingEmail = false;
        }

        public EmailManager(string user, string password, string address, int? addressPort, bool? useSSL, string xmlFolder)
            : this(user, password, address, addressPort, useSSL)
        {
            this._xmlFolder = xmlFolder;
        }

        public EmailManager(EmailConfig config)
        {
            _emailer = new SMTPEmail.SMTPEMailS(null, config.Address, config.UserName, config.Password, config.Port, config.UseSSL);
            if (String.IsNullOrEmpty(config.xmlFolder))
                _xmlFolder = Directory.GetCurrentDirectory();
            _emailer.ThrowErrorOnSendingEmail = false;
        }

        public bool SendEmail(string[] emailTo, string xmlFileName, Dictionary<string, string> replaceVars, string[] bccList = null)
        {
            XDocument xmlDoc = new XDocument();
            string fullFileName = _xmlFolder + "\\" + xmlFileName;
            FileInfo f = new FileInfo(fullFileName);
            if (f == null)
            {
                string errorMessage = String.Format("Could not find file {0}", fullFileName);
                throw new FileNotFoundException(errorMessage);
            }
            using (FileStream fs = f.OpenRead())
            {
                xmlDoc = XDocument.Load(fs);
            }

            _emailer.ReplaceVariables = replaceVars;
            bool success = _emailer.SendEmailFromXmlFormat(emailTo, xmlDoc, null, true);

            return success;
        }

        public bool SendUserWelcomeEmail(string to)
        {
            string[] emailTo = new string[] { to };
            return false;
            //return SendEmail(emailTo, EMAIL_TEMPLATES.USER_WELCOME, null);
        }

        public bool SendInvite(string to, Invite invite)
        {
            string[] emailTo = new string[] { to };

            Dictionary<string, string> replaceVars = new Dictionary<string, string>();
            replaceVars.Add("FIRST_NAME", invite.FirstName);

            return SendEmail(emailTo, EMAIL_TEMPLATES.INVITE_BASIC, replaceVars);
        }

        public bool SendEmailTest(string[] emailTo, string emailFrom, string subject, string body, Dictionary<string, string> replaceVars)
        {
            _emailer.ReplaceVariables = replaceVars;
            bool success = _emailer.SendEmail(emailTo, emailFrom, subject, body, null, true);

            return success;
        }

        private void HandleEmailIssue(string emailTemplate)
        {

        }
    }

    public static class EMAIL_TEMPLATES
    {
        public static string INVITE_BASIC = "INVITE_BASIC.xml";
    }
}
