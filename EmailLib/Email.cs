using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;

namespace SMTPEmail
{
    /// <summary>
    /// Object for sending SMTP emails.
    /// </summary>
    /// <remarks>
    /// This is basically a wrapper for the .Net MailMessage and SMTPClient objects
    /// Simple SMTP Email Object - Carlos Cervantes
    /// Version 1.5
    /// 11/20/2007 Added HTML functionality.
    /// 11/24/2007 Added MHTML functionality.
    /// 06/03/2009 Refactoring to .Net 3.5 Specifications
    /// 08/05/2009 Added Replace Variable Support.
    /// 08/20/2009 Added support for XML Email Formats. This way you can fill int email failes like From and Subject based off of the file and not the code.
    /// 09/04/2009 Added Basic XSD Validation for Email formats. This way you can know at runtime if you email format is correct. Can be disabled.
    /// 10/30/2009 Added Property to control throwing errors on email sending.
    /// </remarks>
    public class SMTPEMailS
    {
        #region SMTPServerVariables
        private string smtpServer;
        private string smtpLogin;
        private string smtpPass;
        private bool? smtpUseSSL;
        private int? smtpPort;
        #endregion

        #region Properties
        /*public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string HtmlBodyFile { get; set; }
        public string Attachment1 { get; set; }
        public string HtmlFileLocation { get; set; }*/
        /// <summary>
        /// Resources to attach to an MHTML email
        /// </summary>
        public ArrayList LinkedResources { get; set; }
        /// <summary>
        /// Values to replace in email body.
        /// </summary>
        public Dictionary<string, string> ReplaceVariables { get; set; }
        /// <summary>
        /// If enabled throws an error when Replace Value is not found in the HTML body.
        /// </summary>
        public bool ErrorOnEmailVariableMisMatch { get; set; }

        /// <summary>
        /// Indicates the start of a replace value in the HTML body. Default is '['
        /// </summary>
        private char emailVariableStartChar = '[';
        public char EmailVariableStartChar
        {
            get { return emailVariableStartChar; }
            set { emailVariableStartChar = value; }
        }

        /// <summary>
        /// Indicates the start of a replace value in the HTML body. Default is ']'
        /// </summary>
        private char emailVariableEndChar = ']';
        public char EmailVariableEndChar
        {
            get { return emailVariableEndChar; }
            set { emailVariableEndChar = value; }
        }

        /// <summary>
        /// If an error was caught and handled, this property will be populated.
        /// </summary>
        public string LastError { get; set; }

        public bool ThrowErrorOnSendingEmail { get; set; }

        /// <summary>
        /// Disable/Enable Validation Email Format against XSD document. Default to True.
        /// </summary>
        public bool ValidateEmailFormatByXSD { get; set; }

        /// <summary>
        /// If this is true when calling SendEmailFromXmlFormat() and ValidateEmailFormatByXSD is true. If the Email Format passed does not validate against the schema 
        /// then the validation errors will be thrown into execution. If this is false then the validation errors will be suppressed.
        /// </summary>
        public bool ThrowErrorsOnEmailSchemaValidation { get; set; }
        #endregion

        /// <summary>
        /// Instantiated object and sets SMTP server info by system.net smtp configuration.
        /// </summary>
        /// <param name="emailVariables">Dictionary containing values to be replaced in the HTML body.</param>
        public SMTPEMailS(Dictionary<string, string> emailVariables)
        {
            ReplaceVariables = (emailVariables != null) ? emailVariables : new Dictionary<string, string>();
            ValidateEmailFormatByXSD = false;
        }

        /// <summary>
        /// Object to Send a SMTP Email. Sets SMTP server info by parameters.
        /// </summary>
        /// <param name="emailVariables">Dictionary containing values to be replaced in the HTML body.</param>
        /// <param name="address">Address of the SMTP Host. Pass null to use webconfig.</param>
        /// <param name="login">Login Name for SMTP Server. Pass null to use webconfig.</param>
        /// <param name="pass">Password for the SMTP Server.Pass null to use webconfig.</param>
        public SMTPEMailS(Dictionary<string, string> emailVariables, string address, string login, string pass, int? port = null, bool? useSSL = null)
        {
            ReplaceVariables = (emailVariables != null) ? emailVariables : new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(address)) { smtpServer = address; }
            if (!String.IsNullOrEmpty(login)) { smtpLogin = login; }
            if (!String.IsNullOrEmpty(pass)) { smtpPass = pass; }
            smtpPort = port;
            smtpUseSSL = useSSL;
            LinkedResources = new ArrayList();
            ValidateEmailFormatByXSD = false;
        }

        /// <summary>
        /// Sends an email with plain text or HTML contents
        /// </summary>
        /// <param name="emailTo">The recipeints email addresses</param>
        /// <param name="emailFrom">The From address of the email. If null, web config will be used.</param>
        /// <param name="subject">The Subject line of the email</param>
        /// <param name="body">The body contents as plain string or HTML</param>
        /// <param name="attachments">Email Attachments. Can be null.</param>
        /// <param name="isHtml">Is the body parameter comprises of HTML then set this true</param>
        /// <returns>Operation Success.</returns>
        public bool SendEmail(IEnumerable<string> emailTo, string emailFrom, string subject, string body, IEnumerable<string> attachments, bool isHtml = true, string[] bccList = null)
        {
            bool success = false;
            MailMessage myMailMessage = new MailMessage();

            // If emailFrom was specified then use that, otherwise we will be using the system.net > mailsettings > from attribute which is automatically inserted by the framework.
            if (!String.IsNullOrEmpty(emailFrom))
            {
                myMailMessage.From = new MailAddress(emailFrom);
            }

            foreach (string mailToAddress in emailTo)
                myMailMessage.To.Add(mailToAddress);

            if (bccList != null)
            {
                foreach (string bcc in bccList)
                    myMailMessage.Bcc.Add(bcc);
            }

            myMailMessage.Subject = subject;
            myMailMessage.IsBodyHtml = isHtml;
            ReplaceEmailVariables(ref body);
            myMailMessage.Body = body;

            if (attachments != null)
            {
                foreach (string attachment in attachments)
                {
                    if (File.Exists(attachment))
                    {
                        myMailMessage.Attachments.Add(new Attachment(attachment));
                    }
                    else { throw new FileNotFoundException(String.Format("The attachment {0} could not be found", attachment)); }
                }
            }

            // If smtpServer was specified manualy by programmer then use it, otherwise we will be using the 
            // system.net > mailsettings > smtp > network > host attribute from attribute which is automatically inserted by the framework. 
            SmtpClient myClient = (!String.IsNullOrEmpty(smtpServer)) ? new SmtpClient(smtpServer) : new SmtpClient();
            if (smtpUseSSL.HasValue)
                myClient.EnableSsl = smtpUseSSL.Value;
            if (smtpPort.HasValue)
                myClient.Port = smtpPort.Value;

            // If credentials were specified manualy by programmer then use them, otherwise we will be using the 
            // system.net > mailsettings > smtp > network > username and password attribute from attributes which are automatically inserted by the framework. 
            if (!String.IsNullOrEmpty(smtpLogin) && !String.IsNullOrEmpty(smtpPass))
            {
                myClient.Credentials = new NetworkCredential(smtpLogin, smtpPass);
            }

            try
            {
                myClient.Send(myMailMessage);
                success = true;
            }
            catch (SmtpException smtpEx)
            {
                LastError = smtpEx.Message;
                success = false;

                if (ThrowErrorOnSendingEmail)
                {
                    throw new Exception(smtpEx.Message, smtpEx.InnerException);
                }
            }
            return success;
        }

        /// <summary>
        /// This method loads a flat file set by the file Property into the EMail body and sends out an Email.
        /// </summary>
        /// <param name="emailTo">The recipeints email addresses</param>
        /// <param name="emailFrom">The From address of the email. IF null, webconfig will be used.</param>
        /// <param name="subject">The Subject line of the email</param>
        /// <param name="file">File location to set as body contents</file>
        /// <param name="attachments">Email Attachments. Can be null.</param>
        /// <param name="isHtml">Is the body parameter comprises of HTML then set this true</param>
        /// <returns>Operation Success.</returns>
        public bool SendEmailFromFile(IEnumerable<string> emailTo, string emailFrom, string subject, string file, IEnumerable<string> attachments, bool isHtml, string[] bccList = null)
        {
            string body = File.ReadAllText(file);

            bool success = SendEmail(emailTo, emailFrom, subject, body, attachments, isHtml);
            return success;
        }

        /// <summary>
        /// Sends an HTML Email based
        /// </summary>
        /// <param name="emailTo">The recipeints email addresses<</param>
        /// <param name="xmlEmail">An XDocument containing the html format in escaped form.</param>
        /// <param name="attachments">Email Attachments. Can be null.</param>
        /// <param name="isHtml">Is the body parameter comprises of HTML then set this true</param>
        /// <returns>Operation Success.</returns>
        public bool SendEmailFromXmlFormat(IEnumerable<string> emailTo, XDocument xmlEmail, IEnumerable<string> attachments, bool isHtml)
        {
            if (ValidateEmailFormatByXSD)
            {
                ValidateXML(xmlEmail);
            }

            var emailValues = (from nodes in xmlEmail.Elements("message_format")
                               select new
                               {
                                   To = nodes.Element("to").Value,
                                   BCC = nodes.Element("bcc").Value,
                                   From = nodes.Element("from").Value,
                                   Subject = nodes.Element("subject").Value,
                                   Body = nodes.Element("body").Value
                               }).Single();

            List<string> emailToComplete = emailTo.ToList();
            emailToComplete.AddRange(emailValues.To.Split(','));

            string[] bccList = emailValues.BCC.Split(',').ToArray();

            bool success = SendEmail(emailTo, emailValues.From, emailValues.Subject, System.Net.WebUtility.HtmlDecode(emailValues.Body), attachments, isHtml, bccList);

            return success;
        }

        /// <summary>
        /// This method includes resources such as Pictures and Files embedded into an HTML file using the
        /// CID attribute.
        /// </summary>
        /// <returns>Success</returns>
        public bool SendMHTMLEmailFromFile()
        {
            /*bool success = false;
            MailMessage myMailMessage = new MailMessage();
            MailAddress mailFrom = new MailAddress(FromAddress);
            myMailMessage.From = mailFrom;
            //Make To Address List
            char[] delimeters = { ';' };
            string[] mailToAddresses = ToAddress.Split(delimeters, StringSplitOptions.None);
            foreach (string mailToAddress in mailToAddresses)
            {
                myMailMessage.To.Add(mailToAddress);
            }
            //End Make To Address List
            myMailMessage.Subject = Subject;
            myMailMessage.IsBodyHtml = true;
            myMailMessage.Body = HtmlBodyFile;
            //***************************************Make Images show on the email.
            //AddImages2Email();
            AlternateView htmlView = new AlternateView(HtmlFileLocation, "text/html");
            foreach (object res in LinkedResources)
            {
                htmlView.LinkedResources.Add((LinkedResource)res);
            }
            myMailMessage.AlternateViews.Add(htmlView);
            //***************************************End Make images show
            if (Attachment1 != "" || Attachment1 != null)
            {//If There is an attachment.
                try
                {
                    Attachment mailAttachment1 = new Attachment(Attachment1);
                    myMailMessage.Attachments.Add(mailAttachment1);
                }
                catch (Exception attachException)
                {
                    LastError = attachException.Message;
                }
            }
            SmtpClient myClient = new SmtpClient(smtpServer);
            myClient.Credentials = new NetworkCredential(smtpLogin, smtpPass);
            try
            {
                myClient.Send(myMailMessage);
                success = true;
            }
            catch (SmtpException smtpEx)
            {
                LastError = smtpEx.Message;
                success = false;
            }
            return success;*/

            return true;
        }

        private void ReplaceEmailVariables(ref string body)
        {
            if (this.ReplaceVariables != null)
            {

                foreach (string key in this.ReplaceVariables.Keys)
                {
                    if (ErrorOnEmailVariableMisMatch)
                    {
                        if (body.IndexOf(key) == -1) { throw new Exception(String.Format("Couldn't find a variable named {} in the email format", key)); }
                    }
                    string stringToReplace = emailVariableStartChar + key + emailVariableEndChar;
                    body = body.Replace(stringToReplace, ReplaceVariables[key]);
                }
            }
        }

        /// <summary>
        /// Loads the resource into the MHTML Email. Must have CID defined in source MHTML
        /// file to work.
        /// </summary>
        /// <param name="fileLoc">Local Relative Location of the Resource.</param>
        /// <param name="strMIMEType">The Mime type of the Reourcse.</param>
        /// <param name="cID">CID corresponding to the one in the MHTL file.</param>
        private void AddCID(string fileLoc, string strMIMEType, string cID)
        {
            LinkedResources.Add(new LinkedResource(fileLoc, strMIMEType));
            ((LinkedResource)LinkedResources[LinkedResources.Count]).ContentId = cID;
        }

        /// <summary>
        /// A wrapper for getting a configKey. This way you know right away which key is missing if one is not available.
        /// </summary>
        /// <param name="configurationKey"></param>
        /// <returns></returns>
        private string GetConfigurationValue(string configurationKey)
        {
            string configVal = String.Empty;

            configVal = ConfigurationManager.AppSettings.Get(configurationKey);

            if (String.IsNullOrEmpty(configVal)) { throw new ConfigurationErrorsException(String.Format("ConfigKey {0} is missing.", configurationKey)); }

            return configVal;
        }

        public void ValidateXML(XDocument email)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            string emailSchemaLocation = Directory.GetCurrentDirectory() + @"\Email\Email.xsd";
            schemas.Add(null, emailSchemaLocation);

            email.Validate(schemas, email_ValidationEventHandler);
        }

        private void email_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            LastError = e.Message;
            if (ThrowErrorsOnEmailSchemaValidation)
            {
                throw e.Exception;
            }
        }
    }
}
