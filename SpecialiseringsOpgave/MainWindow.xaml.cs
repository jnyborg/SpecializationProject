using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpecialiseringsOpgave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadListData();
            var boxItemList = new List<ComboBoxItem>();
            var boxItemFri = new ComboBoxItem();
        }

        private void ListBoxDestinations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDestination = (DataRowView)ListBoxDestinations.SelectedItem;
            if (selectedDestination == null) return;
            TextBoxCountry.Text = selectedDestination["country"] as string;
            var switchDay = selectedDestination["switchDay"] as string;
            switch (switchDay)
            {
                case "fri":
                    ComboBoxSwitchDay.Text = "Fredag";
                    break;
                case "sat":
                    ComboBoxSwitchDay.Text = "Lørdag";
                    break;
                case "sun":
                    ComboBoxSwitchDay.Text = "Søndag";
                    break;
            }

            TextBoxFlightPrice.Text = Convert.ToDouble(selectedDestination["price"]) + "";
            DestinationMessage.Content = "";

        }

        private void ButtonDestinationDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Er du sikker på du vil slette destinationen " + ((DataRowView)ListBoxDestinations.SelectedItem)["country"] + "? Dette vil slette alle dens tilhørende ferieboliger og deres reservationer." , "Slet destination", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Service.DeleteDestination((int)ListBoxDestinations.SelectedValue);
                    LoadListData(); 
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void LoadListData()
        {
            var previousSelectedIndex = ListBoxDestinations.SelectedIndex;
            ListBoxDestinations.ItemsSource = Service.GetDestinations().DefaultView;
            ListBoxDestinations.DisplayMemberPath = "country";
            ListBoxDestinations.SelectedValuePath = "id";
            ListBoxDestinations.SelectedIndex = previousSelectedIndex;
        }

        private void ButtonDestinationUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDestination = (DataRowView) ListBoxDestinations.SelectedItem;
                if (selectedDestination == null) return;
                var id = (int) ListBoxDestinations.SelectedValue;
                var country = TextBoxCountry.Text;
                var switchDay = FormatSwitchDay(ComboBoxSwitchDay.SelectionBoxItem as string);
                var price = Convert.ToInt32(TextBoxFlightPrice.Text);
                Service.UpdateDestination(id, country, switchDay, price);
                LoadListData();
                DestinationMessage.Content = "Opdatering succesfuld!";
            }
            catch (FormatException)
            {
                DestinationMessage.Content = "Du har indtastet en ugyldig pris.";
            }
            catch (ServiceException serviceException)
            {
                DestinationMessage.Content = serviceException.Message;
            }
        }

        private string FormatSwitchDay(string switchDay)
        {
            switch (switchDay)
            {
                case "Fredag":
                    return "fri";
                case "Lørdag":
                    return "sat";
                case "Søndag":
                    return "sun";
            }
            return "";
        }

        private void ButtonDestinationCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var country = TextBoxCountry.Text;
                var switchDay = FormatSwitchDay(ComboBoxSwitchDay.SelectionBoxItem as string);
                var price = Convert.ToInt32(TextBoxFlightPrice.Text);
                Service.CreateDestination(country, switchDay, price);
                LoadListData();
                DestinationMessage.Content = "Destination oprettet!";
            }
            catch (FormatException)
            {
                DestinationMessage.Content = "Du har indtastet en ugyldig pris.";
            }
            catch (ServiceException serviceException)
            {
                DestinationMessage.Content = serviceException.Message;
            }
        }
    }
}
