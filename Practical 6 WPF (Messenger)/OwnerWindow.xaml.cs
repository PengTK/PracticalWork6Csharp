using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Threading;
using System.Collections;

namespace Practical_6_WPF__Messenger_
{
    /// <summary>
    /// Логика взаимодействия для OwnerWindow.xaml
    /// </summary>
    public partial class OwnerWindow : Window
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        private Socket socketServer;
        private List<Socket> clients = new List<Socket>();
        private List<string> allUsers = new List<string>();
        string adminName, newUser;
        bool LogsAreShown = false;

        string userName;

        TcpServer tcpServer;
        TcpClient tcpClient;

        public IEnumerable ListBoxItems
        {
            get { return Users_LBX.ItemsSource; }
            set {
                Users_LBX.ItemsSource = null; 
                Users_LBX.ItemsSource = value; }
        }
        public IEnumerable LogsItems
        {
            get { return Logs_LBX.ItemsSource; }
            set
            {
                Logs_LBX.ItemsSource = null;
                Logs_LBX.ItemsSource = value;
            }
        }
        public IEnumerable MessageItems
        {
            get { return MessageChat_LBX.ItemsSource; }
            set { MessageChat_LBX.Items.Add(value);
                MessageChat_LBX.ScrollIntoView(value);
            }
        }
        public OwnerWindow(string adminName)
        {
            InitializeComponent();
            this.adminName = adminName;
            cts = new CancellationTokenSource();

            InitializeServer();
            InitializeClient();
        }
        private async Task InitializeServer()
        {
            tcpServer = new TcpServer(adminName);
            tcpServer.Start();
        }
        private async Task InitializeClient()
        {
            tcpClient = new TcpClient(userName, "26.157.236.79");
            tcpClient.Start();
        }
        public void SetItemsSource(IEnumerable items)
        {
            Users_LBX.ItemsSource = null;
            Users_LBX.ItemsSource = items;
        }
        private void Send_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (InputMessage_Box.Text != "")
            {
                var message = InputMessage_Box.Text.Trim();
                var username = $"/username [{adminName}]";
                var sendMessage = $"/message {message}";
                MessageChat_LBX.Items.Add($"[{DateTime.Now}] [{adminName}]: {message}");
                MessageChat_LBX.ScrollIntoView($"[{DateTime.Now}] [{adminName}]: {message}");

                tcpServer.BroadCast($"{username} {sendMessage}");
            }
        }
        private void Logs_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!LogsAreShown)
            {
                Disabled_BTN_Name.Content = "Логи";
                Users_LBX.Visibility = Visibility.Collapsed;
                Logs_LBX.Visibility = Visibility.Visible;
            }
            else
            {
                Disabled_BTN_Name.Content = "Пользователи";
                Users_LBX.Visibility = Visibility.Visible;
                Logs_LBX.Visibility = Visibility.Collapsed;
            }
            LogsAreShown = !LogsAreShown;
        }
        private void Exit_BTN_Click(object sender, RoutedEventArgs e)
        {
            var message = "/stopChat";
            foreach (var cl in clients)
            {
                tcpServer.SendMessage(cl, $"{message}");
            }
            cts.Dispose();

            foreach (Window w in Application.Current.Windows)
            {
                w.Close();
            }

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();

            allUsers.Clear();
            tcpServer.UpdateUsersListBox();
        }
    }
}
