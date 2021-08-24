using RS485_Model.Enums;
using RS485_Model.Model;
using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace RS485_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModbusAsciiMaster MasterHandler = new ModbusAsciiMaster();

        ModbusAsciiSlave SlaveHandler = new ModbusAsciiSlave();

        public MainWindow()
        {        
            InitializeComponent();
            DataContext = new ViewModel();
        }
        private void PortsComboOpen(object sender, EventArgs e)
        {
            PortsComboMaster.ItemsSource = SerialPort.GetPortNames();
            PortsComboSlave.ItemsSource = SerialPort.GetPortNames();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AddressTextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out int value))
            {
                if (value > 247)
                    textbox.Text = "247";
                else if (value < 1)
                    textbox.Text = "1";
            }
            else
            {
                textbox.Text = "1";
            }
        }

        private void TransactionTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedAddressMaster.IsEnabled = (TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Addressed;
            Command2.IsEnabled = (TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Broadcast;
            Command2Button.IsEnabled = (TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Broadcast;
        }

        private async void OpenConnectionSlave(object sender, RoutedEventArgs e)
        {
            try
            {
                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("", "Otwarto połączenie Slave", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OpenConnectionMaster(object sender, RoutedEventArgs e)
        {
            try
            {
                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("", "Otwarto połączenie Master", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ExecuteCommand1(object sender, RoutedEventArgs e)
        {
            try
            {
                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("", "Wykonano rozkaz 1", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ExecuteCommand2(object sender, RoutedEventArgs e)
        {
            try
            {
                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("", "Wykonano rozkaz 2", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
