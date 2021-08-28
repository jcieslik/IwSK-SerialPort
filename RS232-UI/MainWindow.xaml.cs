using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RS232_Model.Model;
using RS232_Model.Enums;

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
            AddHandler(Keyboard.PreviewKeyDownEvent, (KeyEventHandler)controlKeyDownEvent);
        }

        private async void controlKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (handler.FlowControlType == FlowControlType.XOnXOff)
            {
                if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    await handler.WriteAsync(((char)19).ToString());
                }
                else if (e.Key == Key.Q && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    await handler.WriteAsync(((char)17).ToString());
                }
            }
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
                handler.CloseIfOpened();
                ConfigureHandler();
                handler.Open();
                BrushConverter bc = new BrushConverter();
                ConnectionState.Fill = (Brush)bc.ConvertFrom("Green");
                MessageBox.Show("Poprawnie otwarto port", "Port", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                handler.Close();
                BrushConverter bc = new BrushConverter();
                ConnectionState.Fill = (Brush)bc.ConvertFrom("Red");
                MessageBox.Show("Połączenie zostało zamknięte", "Port", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
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
            handler.ConnectionClosed -= ClosedConnection;
            handler.ConnectionClosed += ClosedConnection;
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

        private void ReceiveData(object sender, TextReceivedEventArgs e)
        {
            ReceiveTextBox.Dispatcher.Invoke(() => 
            {
                ReceiveTextBox.AppendText(e.ReceivedText);
                PingTextBox.ScrollToEnd();
            });
        }

        private void ClosedConnection(object sender, bool e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e)
                {
                    CloseConnection(this, new RoutedEventArgs());
                    PortsCombo.SelectedItem = "";
                }
            });
        }

        private void TerminatorChanged(object sender, SelectionChangedEventArgs e)
        {
            TerminatorTextBox.IsEnabled = (Terminator)TerminatorCombo.SelectedValue == Terminator.Custom;
        }

        private async void OnPingButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PingTextBox.AppendText("Ping");
                long elapsedTime = await handler.PingAsync();
                PingTextBox.AppendText(" - OK - " + elapsedTime + "ms\n");
                PingTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                PingTextBox.AppendText("\n");
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PortsComboOpen(object sender, EventArgs e)
        {
            PortsCombo.ItemsSource = SerialPort.GetPortNames();
        }
    }
}
