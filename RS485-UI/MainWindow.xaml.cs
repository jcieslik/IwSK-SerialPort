using RS485_Model.Enums;
using RS485_Model.Model;
using System;
using System.IO.Ports;
using System.Text;
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

        ModbusSlaveRequestHandler SlaveRequestHandler = new ModbusSlaveRequestHandler();

        public MainWindow()
        {        
            InitializeComponent();
            DataContext = new ViewModel();
            SlaveRequestHandler.MessageReceived += SlaveMessageReceived;
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
            Command2.IsEnabled = (TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Addressed;
            Command2Button.IsEnabled = (TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Addressed;
        }

        private void OpenConnectionSlave(object sender, RoutedEventArgs e)
        {
            try
            {
                SlaveHandler.Address = byte.Parse(SelectedAddressSlave.Text);
                SlaveHandler.PortName = (string)this.PortsComboSlave.SelectedItem;
                SlaveHandler.MaxCharInterval = (int)characterDistanceSlaveSliderValue.Value;
                SlaveHandler.RequestHandler = SlaveRequestHandler;
                SlaveHandler.Run();
                MessageBox.Show("Otwarto połączenie", "Slave", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenConnectionMaster(object sender, RoutedEventArgs e)
        {
            try
            {
                MasterHandler.TransactionRetry = (int)retransmissionsSliderValue.Value;
                MasterHandler.TransactionTimeout = (int)transactionSliderValue.Value;
                MasterHandler.PortName = (string)this.PortsComboMaster.SelectedItem;
                MasterHandler.MaxCharInterval = (int)characterDistanceMasterSliderValue.Value;
                MasterHandler.Open(); 
                MessageBox.Show("Otwarto połączenie", "Master", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private byte GetCommandAddress()
        {
            if((TransactionType)TransactionTypeCombo.SelectedValue == TransactionType.Addressed)
            {
                return Byte.Parse(SelectedAddressMaster.Text);
            }
            else
            {
                return 0;
            }
        }
        private async void ExecuteCommand1(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new ModbusFrame();
                request.Address = GetCommandAddress();
                request.Data = Encoding.ASCII.GetBytes(Command1.Text);
                request.Function = 1;
                var response = await MasterHandler.MakeRequest(request);

                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("Wykonano rozkaz 1", "Master", MessageBoxButton.OK, MessageBoxImage.Information);
                Command1.Text = "";

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
                var request = new ModbusFrame();
                request.Address = GetCommandAddress();
                request.Data = Array.Empty<byte>();
                request.Function = 2;
                var response = await MasterHandler.MakeRequest(request);
                if(response != null)
                {
                    Command2.Text = Encoding.ASCII.GetString(response.Data);
                }
                //await handler.WriteAsync(SendTextBox.Text);
                //SendTextBox.Text = "";
                MessageBox.Show("Wykonano rozkaz 2", "Master", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SlaveMessageReceived(object sender, string message)
        {
            Dispatcher.Invoke(() => {
                Command1Slave.Text = message;
            });
        }

        private void SlaveResponseTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            SlaveRequestHandler.MessageToSend = textBox.Text;
        }

    }
}
