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
    /// Interaction logic for NewCampaign.xaml
    /// </summary>
    public partial class NewCampaign : Page
    {
        IMusubiRepository _musubiRepo = new MusubiSQLRepository();

        public NewCampaign()
        {
            InitializeComponent();
        }

        private void btnCreateCampaign_Click(object sender, RoutedEventArgs e)
        {
            Campaign campaign = new Campaign();
            campaign.Name = txtName.Text;
            Campaign newCampaign = _musubiRepo.InsertCampaign(campaign);

            ManageCampaign page = new ManageCampaign(newCampaign);
            this.NavigationService.Navigate(page);
        }
    }
}