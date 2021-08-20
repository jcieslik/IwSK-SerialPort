using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using RS232_Model.Model;
using RS232_Model.Enums;
using System.Diagnostics;

namespace RS232_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHandler handler = new SerialPortHandler();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BaudRateTextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out int value))
            {
                if (value > 115200)
                    textbox.Text = "115200";
                else if (value < 150)
                    textbox.Text = "150";
            } 
            else
            {
                textbox.Text = "150";
            }
        }

        private void InitializeConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigureHandler();
                handler.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureHandler()
        {
            if ((Terminator)TerminatorCombo.SelectedValue == Terminator.Custom && TerminatorTextBox.Text.Length == 0)
            {
                throw new Exception("Terminator string must have one or two characters!");
            }
            handler.PortName = (string)PortsCombo.SelectedItem;
            handler.DataBitsNumber = (DataBitsNumber)DataBitsCombo.SelectedValue;
            handler.ParityBitsNumber = (ParityBitsNumber)ParityBitsCombo.SelectedValue;
            handler.StopBitsNumber = (StopBitsNumber)StopBitsCombo.SelectedValue;
            handler.Terminator = (Terminator)TerminatorCombo.SelectedValue;
            handler.FlowControlType = (FlowControlType)FlowControlCombo.SelectedValue;
            handler.CustomTerminator = TerminatorTextBox.Text;
            if (int.TryParse(BaudRateTextBox.Text, out int baudRate))
            {
                handler.BaudRate = baudRate;
            }
            else
            {
                handler.BaudRate = 150;
            }
            handler.TextReceived -= ReceiveData;//Na wypadek, gdyby handler wcześniej był już zarejestrowany
            handler.TextReceived += ReceiveData;
        }

        private async void SendData(object sender, RoutedEventArgs e)
        {
            try
            {
                await handler.WriteAsync(SendTextBox.Text);
                SendTextBox.Text = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /*
        private void ReceiveData(object sender, ByteReceivedEventArgs e)
        {
            ReceiveTextBox.Dispatcher.Invoke(() => ReceiveTextBox.Text += Convert.ToChar(e.ReceivedByte));
        }*/

        private void ReceiveData(object sender, TextReceivedEventArgs e)
        {
            ReceiveTextBox.Dispatcher.Invoke(() => ReceiveTextBox.Text += e.ReceivedText);
        }

        private void TerminatorChanged(object sender, SelectionChangedEventArgs e)
        {
            if((Terminator)TerminatorCombo.SelectedValue == Terminator.Custom)
            {
                TerminatorTextBox.IsEnabled = true;
            }
            else
            {
                TerminatorTextBox.IsEnabled = false;
            }
        }

        private async void OnPingButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PingTextBox.Text += "PING";
                long elapsedTime = await handler.PingAsync();
                PingTextBox.Text += " - OK - " + elapsedTime + "ms\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
