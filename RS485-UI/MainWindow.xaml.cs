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

namespace RS485_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}
