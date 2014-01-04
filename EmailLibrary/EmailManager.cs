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
                _xmlFolder = @"C:\Users\S\Documents\visual studio 2010\Projects\MusubiMailer\EmailLibrary\EmailTemplates";
            //Directory.GetCurrentDirectory();
            _emailer.ThrowErrorOnSendingEmail = true;
        }

        public bool SendEmail(string[] emailTo, string emailFrom, string xmlFileName, Dictionary<string, string> replaceVars, string[] bccList = null)
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
            bool success = _emailer.SendEmailFromXmlFormat(emailTo, emailFrom, xmlDoc, null, true);

            return success;
        }

        public bool SendUserWelcomeEmail(string to)
        {
            string[] emailTo = new string[] { to };
            return false;
            //return SendEmail(emailTo, EMAIL_TEMPLATES.USER_WELCOME, null);
        }

        public bool SendInvite(string to, string from, Invite invite)
        {
            string[] emailTo = new string[] { to };

            Dictionary<string, string> replaceVars = new Dictionary<string, string>();
            replaceVars.Add("FIRST_NAME", invite.FirstName);

            return SendEmail(emailTo, from, EMAIL_TEMPLATES.INVITE_BASIC, replaceVars);
        }

        private string[] EMAIL_INVITE_TEMPLATES = { "INVITE_BASIC.xml", "INVITE_BASIC_2.xml", "INVITE_BASIC_3.xml", "INVITE_BASIC_4.xml", "INVITE_BASIC_5.xml", "INVITE_BASIC_6.xml", "INVITE_BASIC_7.xml" };
        public bool SendRandomInvite(string to, string from, Invite invite)
        {
            string[] emailTo = new string[] { to };
            int templatesCount = EMAIL_INVITE_TEMPLATES.Length - 1;
            int index = new Random().Next(templatesCount);
            string inviteTemplate = EMAIL_INVITE_TEMPLATES[index];

            Dictionary<string, string> replaceVars = new Dictionary<string, string>();
            replaceVars.Add("FIRST_NAME", invite.FirstName);

            return SendEmail(emailTo, from, inviteTemplate, replaceVars);
            //return false;
        }

        public bool SendSlipsum(string to, string from, string header, string body)
        {
            string[] emailTo = new string[] { to };

            Dictionary<string, string> replaceVars = new Dictionary<string, string>();
            replaceVars.Add("SLIPSUM_HEADER", header);
            replaceVars.Add("SLIPSUM", body);

            return SendEmail(emailTo, from, "SLIPSUM.xml", replaceVars);
            //return false;

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
