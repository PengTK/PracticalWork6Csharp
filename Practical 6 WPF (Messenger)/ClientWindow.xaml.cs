using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Practical_6_WPF__Messenger_
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        int counter = 0;
        MainWindow main = new MainWindow();
        private List<string> _allUsers = new List<string>();
        string name = "";
        CancellationTokenSource cts = new CancellationTokenSource();
        private string[] users;
        private Socket socketClient;
        string userName, IP_address;
        public bool isClosed = false;


        TcpClient tcpClient;
        public ClientWindow(string userName, string IP_address)
        {
            InitializeComponent();
            this.userName = userName;
            this.IP_address = IP_address;

            InitializeClient();
            /*InitializeClientAsync();
            ReceivingMessage();
            SendMessage("/newUser " + userName);*/
        }
        public IEnumerable ListBoxItems
        {
            get { return Users_LBX.ItemsSource; }
            set
            {
                Users_LBX.ItemsSource = null;
                Users_LBX.ItemsSource = value;
            }
        }
        public IEnumerable MessageItems
        {
            get { return MessageChat_LBX.ItemsSource; }
            set { MessageChat_LBX.Items.Add(value);
                  MessageChat_LBX.ScrollIntoView(value);
            }
        }
        private async Task InitializeClient()
        {
            tcpClient = new TcpClient(userName, IP_address);
            await tcpClient.Start();
        }
        private void Send_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (InputMessage_Box.Text.Trim().StartsWith("/disconnect"))
            {
                InputMessage_Box.Text += $" {userName}";
                main.Show();
                isClosed = true;
                Close();
            }
            tcpClient.SendMessage(InputMessage_Box.Text.Trim());
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (!isClosed)
            {
                InputMessage_Box.Text = $"/disconnect {userName}";
                main.Show();
                tcpClient.SendMessage(InputMessage_Box.Text.Trim());
                isClosed = false;
            }
        }
        private void Exit_BTN_Click(object sender, RoutedEventArgs e)
        {
            isClosed = true;
            InputMessage_Box.Text = $"/disconnect {userName}";
            MainWindow main = new MainWindow();
            main.Show();
            Close();
            tcpClient.SendMessage(InputMessage_Box.Text.Trim());
            isClosed = false;
        }
    }
}
