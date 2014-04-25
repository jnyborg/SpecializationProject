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

            previousSelectedIndex = ListBoxHolidayHomes.SelectedIndex;
            ListBoxHolidayHomes.ItemsSource = Service.GetHolidayHomes().DefaultView;
            ListBoxHolidayHomes.DisplayMemberPath = "description";
            ListBoxHolidayHomes.SelectedValuePath = "id";
            ListBoxHolidayHomes.SelectedIndex = previousSelectedIndex;

            if (ComboBoxCountries.SelectedIndex > 0)
                previousSelectedIndex = ComboBoxCountries.SelectedIndex;
            else
                previousSelectedIndex = 0;
            ComboBoxCountries.ItemsSource = Service.GetDestinations().DefaultView;
            ComboBoxCountries.DisplayMemberPath = "country";
            ComboBoxCountries.SelectedValuePath = "id";
            ComboBoxCountries.SelectedIndex = previousSelectedIndex;

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
            LoadDataGrid();
        }

        private void LoadDataGrid()
        {
            DataGridWeeklyHolidayHomeInfo.ItemsSource =
                Service.GetWeeklyHolidayHomeInfos((int)ListBoxHolidayHomes.SelectedValue).DefaultView;
            DataGridWeeklyHolidayHomeInfo.Columns[0].Header = "Ugenummer";
            DataGridWeeklyHolidayHomeInfo.Columns[0].Width = 190;
            DataGridWeeklyHolidayHomeInfo.Columns[0].IsReadOnly = true;
            DataGridWeeklyHolidayHomeInfo.Columns[1].Header = "Pris i kr.";
            DataGridWeeklyHolidayHomeInfo.Columns[1].Width = 190;
            DataGridWeeklyHolidayHomeInfo.Columns[2].Header = "Ledig";
            ((DataGridCheckBoxColumn) DataGridWeeklyHolidayHomeInfo.Columns[2]).IsThreeState = false;
            DataGridWeeklyHolidayHomeInfo.CanUserAddRows = false;
            SetComboBoxWeekNumbers();
        }

        private void SetComboBoxWeekNumbers()
        {
            int previousValue = ComboBoxWeekNumbers.SelectedIndex;
            var availableWeekNumbers = new List<int>();
            for (int i = 1; i <= 52; i++)
            {
                availableWeekNumbers.Add(i);
            }
            DataTable loadedTable = ((DataView) DataGridWeeklyHolidayHomeInfo.ItemsSource).Table;
            foreach (DataRow row in loadedTable.Rows)
            {
                int weekNumber = Convert.ToInt32(row["weekNumber"]);
                availableWeekNumbers.Remove(weekNumber);
            }
            ComboBoxWeekNumbers.ItemsSource = availableWeekNumbers;
            if (previousValue < 0 || previousValue >= availableWeekNumbers.Count) ComboBoxWeekNumbers.SelectedIndex = 0;
            else ComboBoxWeekNumbers.SelectedIndex = previousValue;

            if (availableWeekNumbers.Count == 0) ButtonAddRow.IsEnabled = false;
            else ButtonAddRow.IsEnabled = true;
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

        private void ButtonHolidayHomeCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var description = TextBoxDescription.Text;
                var maxPersons = Convert.ToInt32(TextBoxMaxPersons.Text);
                var shoppingDistance = Convert.ToInt32(TextBoxShoppingDistance.Text);
                var beachDistance = Convert.ToInt32(TextBoxBeachDistance.Text);
                var destinationId = Convert.ToInt32(ComboBoxCountries.SelectedValue);
                Service.CreateHolidayHome(description, maxPersons, shoppingDistance, beachDistance, destinationId);
                LoadListData();
                HolidayHomeMessage.Content = "Destination oprettet!";
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

        private void ButtonRemoveWeeklyInfo_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridWeeklyHolidayHomeInfo.SelectedIndex == -1) return;
            ((DataView)DataGridWeeklyHolidayHomeInfo.ItemsSource).Table.Rows.RemoveAt(DataGridWeeklyHolidayHomeInfo.SelectedIndex);
            SetComboBoxWeekNumbers();

        }

        private void ButtonAddWeeklyInfo_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(DataGridWeeklyHolidayHomeInfo.ItemsSource);
            Service.AddWeeklyHolidayHomeInfos((int)ListBoxHolidayHomes.SelectedValue,((DataView)DataGridWeeklyHolidayHomeInfo.ItemsSource).Table);
            
        }

        private void DataGridWeeklyHolidayHomeInfo_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                
                var weekNumberElement = DataGridWeeklyHolidayHomeInfo.Columns[0].GetCellContent(e.Row);
                int weekNumber = 0;
                if (weekNumberElement.GetType() == typeof(TextBox))
                {
                    weekNumber = Convert.ToInt32(((TextBox)weekNumberElement).Text);
                    
                }
                var priceElement = DataGridWeeklyHolidayHomeInfo.Columns[1].GetCellContent(e.Row);
                int price = 0;
                if (priceElement.GetType() == typeof(TextBox))
                {
                    price = Convert.ToInt32(((TextBox)priceElement).Text);

                }
                Debug.WriteLine(weekNumber + " " + price);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ButtonAddRow_Click(object sender, RoutedEventArgs e)
        {
            int selectedWeek = Convert.ToInt32(ComboBoxWeekNumbers.SelectedItem);
            ((DataView) DataGridWeeklyHolidayHomeInfo.ItemsSource).Table.Rows.Add(selectedWeek, 0, false);
            ((List<int>) ComboBoxWeekNumbers.ItemsSource).Remove(selectedWeek);
            SetComboBoxWeekNumbers();
        }
    }
}
