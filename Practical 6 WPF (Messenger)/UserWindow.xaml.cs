using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Practical_6_WPF__Messenger_
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private Socket socketClient;
        string userName;
        public UserWindow(string userName)
        {
            InitializeComponent();
            this.userName = userName;
        
            InitializeClientAsync();
            ReceivingMessage();
            SendMessage("/username " + userName);
        }
        private async Task InitializeClientAsync()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketClient.ConnectAsync("127.0.0.1", 8888);
        }
        private async Task ReceivingMessage()
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                await socketClient.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);

                string receivedMessage = Encoding.UTF8.GetString(bytes);
                /*if (receivedMessage.StartsWith("/username"))
                {
                    newUser = receivedMessage;*//*.Substring(8, receivedMessage.Length);*/
                    /* LBX_Update(receivedMessage.Substring(8, receivedMessage.Length));*//*
                }*/
                MessageChat_LBX.Items.Add($"[Вы]: {receivedMessage}");
            }
        }
        private async Task SendMessage(string receivedMessage)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(receivedMessage);
            await socketClient.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        }

        private void Send_BTN_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(InputMessage_Box.Text);
        }
    }
}
