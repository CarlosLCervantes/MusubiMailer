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
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using AutoMapper;
using DAL;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MusubiMailer.Pages
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : Page
    {
        IMusubiRepository _musubiRepo = new MusubiSQLRepository();

        public Import()
        {
            InitializeComponent();
        }

        private void btnChoseFile_Click(object sender, RoutedEventArgs e)
        {
            string selectedFile = string.Empty;
            OpenFileDialog selectFileDialog = new OpenFileDialog();
            if (selectFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFile = selectFileDialog.FileName;
                txtCSVFile.Text = selectedFile;
            }
        }

        private void btnImportNow_Click(object sender, RoutedEventArgs e)
        {
            List<EmailFileRecord> records = (List<EmailFileRecord>)dgEmails.ItemsSource;
            List<DAL.Email> emails = Mapper.Map<List<EmailFileRecord>, List<DAL.Email>>(records);
            string importGroupName = txtGroupName.Text;

            _musubiRepo.InsertEmails(emails, importGroupName);

            Welcome welcomePage = new Welcome();
            this.NavigationService.Navigate(welcomePage);
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string fileToImport = txtCSVFile.Text;
            if (String.IsNullOrWhiteSpace(fileToImport))
            {
                log("NO FILENAME SUPPLIED");
                return;
            }
            //Check indexs
            if (String.IsNullOrWhiteSpace(txtFirstNameIndex.Text)) { log("FIRST NAME INDEX INVALID"); return; }
            if (String.IsNullOrWhiteSpace(txtLastNameIndex.Text)) { log("LAST NAME INDEX INVALID"); return; }
            if (String.IsNullOrWhiteSpace(txtEmailNameIndex.Text)) { log("EMAIL INDEX INVALID"); return; }

            //Check Group Name
            if (String.IsNullOrWhiteSpace(txtGroupName.Text)) { log("IMPORT ALIAS REQUIRED"); return; }

            //Check if file is valid
            List<string> errors = new List<string>();
            if (!isFileValid(fileToImport, out errors))
            {
                foreach (string error in errors) { log(error); }
                return;
            }

            //Load File
            int firstNameIndex = Convert.ToInt32(txtFirstNameIndex.Text);
            int lastNameIndex = Convert.ToInt32(txtLastNameIndex.Text);
            int emailIndex = Convert.ToInt32(txtEmailNameIndex.Text);
            List<EmailFileRecord> emails = readFile(fileToImport, firstNameIndex, lastNameIndex, emailIndex);
            dgEmails.ItemsSource = emails;
            dgEmails.DataContext = emails;

            log(String.Format("Imported {0} Records", emails.Count));
            btnImportNow.IsEnabled = true;
        }

        private List<EmailFileRecord> readFile(string fileName, int firstNameIndex, int lastNameIndex, int emailIndex)
        {
            var reader = new TextFieldParser(fileName);
            reader.TextFieldType = FieldType.Delimited;
            reader.SetDelimiters(GlobalConst.IMPORT_FILE_DELIMITER);

            int skipCount = 0;
            int rowCount = 0;
            List<EmailFileRecord> lines = new List<EmailFileRecord>();
            Regex emailRegex = new Regex(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$", RegexOptions.IgnoreCase);
            try
            {
                while (!reader.EndOfData)
                {
                    string[] values = reader.ReadFields();

                    string email = values[emailIndex];
                    bool emailValid = true;
                    
                    if (email == null || !emailRegex.IsMatch(email))
                    {
                        emailValid = false;
                        skipCount++;
                    }

                    if (emailValid)
                    {
                        string fname = "";
                        string lname = "";
                        if (firstNameIndex == -1)
                        {
                            fname = "Fashionista";
                            lname = "Jones";
                        }
                        else if (firstNameIndex == lastNameIndex)
                        {
                            string[] nameVals = values[firstNameIndex].Split(GlobalConst.IMPORT_FILE_DELIMITER[0]);
                            fname = nameVals[0];
                            lname = (nameVals.Length > 2) ? lname = nameVals[1] : "Jones";
                        }
                       
                        else
                        {
                            fname = values[firstNameIndex];
                            lname = values[lastNameIndex];
                        }

                        EmailFileRecord record = new EmailFileRecord(fname, lname, email, "TODO: ADD This");
                        lines.Add(record);
                        rowCount++;    
                    }

                    log(String.Format("PR {0}, VR {1}", (rowCount + skipCount), rowCount));
                }
            }
            catch (Exception ex) { throw ex; }
            finally { reader.Close(); }

            log(String.Format("SKIPPED {0} Records", skipCount));

            return lines;
        }

        private bool isFileValid(string fileName, out List<string> errors)
        {
            //var reader = new StreamReader(File.OpenRead(fileName));
            var reader = new TextFieldParser(fileName);
            reader.TextFieldType = FieldType.Delimited;
            reader.SetDelimiters(GlobalConst.IMPORT_FILE_DELIMITER);

            int columnCount = 0;
            int rowCount = 0;
            bool fileValid = true;
            errors = new List<string>();
            try
            {
                while (!reader.EndOfData)
                {
                    string[] values = reader.ReadFields();

                    //Check file valid 
                    if (rowCount > 1 && columnCount != values.Length)
                    {
                        fileValid = false;
                        errors.Add(String.Format("Found an inconsistent column length in file at line {0}.", rowCount));
                        reader.Close();
                        return fileValid;
                    }
                    columnCount = values.Length;
                    rowCount++;
                }
            }
            catch (Exception ex) { throw ex; }
            finally { reader.Close(); }

            return fileValid;
        }

        private void log(string message)
        {
            //tbConsole.Inlines.Add(new Bold(new Run(message)));
            //tbConsole.Text = tbConsole.Text + "\n" + message;
            Debugger.Log(1, "test", message);
            tbConsole.Text = message;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            btnImportNow.IsEnabled = false;
        }

        
    }
}
