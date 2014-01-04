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
    /// Interaction logic for Campaigns.xaml
    /// </summary>
    public partial class Campaigns : Page
    {
        IMusubiRepository _musubiRepo = new MusubiSQLRepository();

        public Campaigns()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bindCampaigns();
        }

        private void bindCampaigns() 
        {
            var campaigns = _musubiRepo.GetCampaigns();
            ItemListView.ItemsSource = campaigns;
            ItemListView.DataContext = campaigns;
        }

        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Campaign selectedCampaign = ItemListView.SelectedItem as Campaign;
            ManageCampaign page = new ManageCampaign(selectedCampaign);
            this.NavigationService.Navigate(page);
        }

        private void btnSelectCampaign_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NewCampaign page = new NewCampaign();
            this.NavigationService.Navigate(page);
        }


        




        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
