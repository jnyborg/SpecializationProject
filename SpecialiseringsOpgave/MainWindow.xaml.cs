using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
            if (ListBoxDestinations.SelectedIndex == -1) return;
            var result = MessageBox.Show("Er du sikker på du vil slette destinationen " + ((DataRowView)ListBoxDestinations.SelectedItem)["country"] + "? Dette vil slette alle dens tilhørende ferieboliger og deres reservationer.", "Slet destination", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Service.DeleteDestination((int)ListBoxDestinations.SelectedValue);
                    LoadListData();
                    DestinationMessage.Content = "Destination slettet!";
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        //TODO Only load active tab
        private void LoadListData()
        {
            var previousSelectedIndex = ListBoxDestinations.SelectedIndex;
            ListBoxDestinations.ItemsSource = Service.GetDestinations().DefaultView;
            ListBoxDestinations.DisplayMemberPath = "country";
            ListBoxDestinations.SelectedValuePath = "id";
            ListBoxDestinations.SelectedIndex = previousSelectedIndex;

            ListBoxHolidayHomes.ItemsSource = Service.GetHolidayHomes().DefaultView;
            ListBoxHolidayHomes.DisplayMemberPath = "description";
            ListBoxHolidayHomes.SelectedValuePath = "id";

            ComboBoxCountries.ItemsSource = Service.GetDestinations().DefaultView;
            ComboBoxCountries.DisplayMemberPath = "country";
            ComboBoxCountries.SelectedValuePath = "id";

        }

        private void ButtonDestinationUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxDestinations.SelectedIndex == -1) return;
            try
            {
                var id = (int)ListBoxDestinations.SelectedValue;
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

        private void ListBoxHolidayHomes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedHolidayHome = (DataRowView)ListBoxHolidayHomes.SelectedItem;
            if (selectedHolidayHome == null) return;
            TextBoxDescription.Text = selectedHolidayHome["description"] + "";
            TextBoxMaxPersons.Text = selectedHolidayHome["maxPersons"] + "";
            TextBoxBeachDistance.Text = selectedHolidayHome["beachDistance"] + "";
            TextBoxShoppingDistance.Text = selectedHolidayHome["shoppingDistance"] + "";
            ComboBoxCountries.SelectedValue = (int)selectedHolidayHome["destinationId"];
            DataGridWeeklyHolidayHomeInfo.ItemsSource =
                Service.GetWeeklyHolidayHomeInfos((int)ListBoxHolidayHomes.SelectedValue).DefaultView;
        }

        private void ButtonHolidayHomeDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxHolidayHomes.SelectedIndex == -1) return;
            var result = MessageBox.Show("Er du sikker på at du vil slette denne feriebolig?", "Slet feriebolig", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Service.DeleteHolidayHome((int)ListBoxHolidayHomes.SelectedValue);
                    LoadListData();
                    break;
                case MessageBoxResult.No:
                    break;
            }

        }

        private void ButtonHolidayHomeUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxHolidayHomes.SelectedIndex == -1) return;
            try
            {
                var id = (int)ListBoxHolidayHomes.SelectedValue;
                var description = TextBoxDescription.Text;
                var maxPersons = Convert.ToInt32(TextBoxMaxPersons.Text);
                var shoppingDistance = Convert.ToInt32(TextBoxShoppingDistance.Text);
                var beachDistance = Convert.ToInt32(TextBoxBeachDistance.Text);
                var destinationId = Convert.ToInt32(ComboBoxCountries.SelectedValue);
                Service.UpdateHolidayHome(id, description, maxPersons, shoppingDistance, beachDistance, destinationId);
                LoadListData();
                HolidayHomeMessage.Content = "Opdatering succesfuld!";
            }
            catch (FormatException)
            {
                HolidayHomeMessage.Content = "Du har indtastet bogstaver, hvor der bør være tal.";
            }
            catch (ServiceException serviceException)
            {
                HolidayHomeMessage.Content = serviceException.Message;
            }
        }
    }
}
