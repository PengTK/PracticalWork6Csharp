using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Practical_6_WPF__Messenger_
{
    /*public List<string> _allUsers = new List<string>();*/
    public class TcpServer
    {
        private Socket _socketServer;
        private List<Socket> _clients = new List<Socket>();
        private List<string> _allUsers = new List<string>();
        private List<string> _allLogs = new List<string>();
        string adminName, newUser;

        CancellationTokenSource cts = new CancellationTokenSource();
        bool LogsAreShown = false;

        public TcpServer(string adminName)
        {
            this.adminName = adminName;
        }
        public async Task Start()
        {
            _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);

            try
            {
                _socketServer.Bind(ipPoint);
                _socketServer.Listen(15);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10048) { }
                else throw;
            }

            LBX_Add($"[{adminName}]");
            ClientsListening();
        }
        private async Task LBX_Add(string name)
        {
            _allUsers.Add(name);
            UpdateUsersListBox();
        }

        private async Task LBX_Remove(string name)
        {
            _allUsers.Remove(name);
            UpdateUsersListBox();
        }

        public async Task UpdateUsersListBox()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ownerWindow = Application.Current.Windows.OfType<OwnerWindow>().FirstOrDefault();
                if (ownerWindow != null)
                {
                    ownerWindow.ListBoxItems = _allUsers;
                }
            });
        }
        private void Message_Add(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ClientWindow = Application.Current.Windows.OfType<OwnerWindow>().FirstOrDefault();
                if (ClientWindow != null)
                {
                    ClientWindow.MessageItems = message;
                }
            });
        }
        private void UpdateLogs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ownerWindow = Application.Current.Windows.OfType<OwnerWindow>().FirstOrDefault();
                if (ownerWindow != null)
                {
                    ownerWindow.LogsItems = _allLogs;
                }
            });
        }

        private async Task ClientsListening()
        {
            while (true)
            {
                var client = await _socketServer.AcceptAsync();
                _clients.Add(client);

                ReceivingMessage(client);
            }
        }
        private async Task ReceivingMessage(Socket client)
        {
            while (true)
            {
                byte[] bytes = new byte[1024];

                await client.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);

                string receivedMessage = Encoding.UTF8.GetString(bytes);
                if (receivedMessage.StartsWith("/newUser"))
                {
                    newUser = receivedMessage.Substring(9);
                    newUser = newUser.TrimEnd('\0');
                    if (!string.IsNullOrEmpty(newUser))
                    {
                        if (!_allUsers.Contains($"[{adminName}]")) _allUsers.Insert(0, $"[{adminName}]");
                        LBX_Add(newUser);
                        receivedMessage = await Send_LBX_ToUsers(receivedMessage);

                        _allLogs.Add($"  [{newUser}] - Успешное подсоединение\n\t[{DateTime.Now}]");
                        UpdateLogs();
                    }

                }
                else if (receivedMessage.StartsWith("/disconnect"))
                {
                    string userToDelete = receivedMessage.Substring(12).TrimEnd('\0');
                    _allUsers.Remove(userToDelete);
                    LBX_Remove(userToDelete);
                    receivedMessage = await Send_LBX_ToUsers(receivedMessage);

                    _allLogs.Add($"  [{userToDelete}] - Отсоединение от чата.\n\t[{DateTime.Now}]");
                    UpdateLogs();
                }
                else
                {
                    Message_Add($"[{DateTime.Now}] [{newUser}]: {receivedMessage}");
                    string str = $"/username {newUser} /message {receivedMessage}";
                    receivedMessage = str;
                }

                foreach (var cl in _clients)
                {
                    SendMessage(cl, receivedMessage);
                }
            }
        }
        private async Task<string> Send_LBX_ToUsers(string receivedMessage)
        {
            string allNames = "/listUsers";
            if (_allUsers.Count > 0)
            {
                allNames += "~" + string.Join("~", _allUsers);
            }
            receivedMessage = allNames.TrimEnd('\0');
            return receivedMessage;
        }
        public async Task SendMessage(Socket client, string receivedMessage)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(receivedMessage);
            await client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        }
        public void BroadCast(string message)
        {
            foreach (var item in _clients)
            {
                SendMessage(item, message);
            }
        }

    }
    public class TcpClient
    {
        private string[] users;
        private Socket socketClient;
        private readonly string userName, IP_address;
        private List<string> _allUsers = new List<string>();

        CancellationTokenSource cts = new CancellationTokenSource();

        public TcpClient(string userName, string IP_address)
        {
            this.userName = userName;
            this.IP_address = IP_address;
        }
        public async Task Start()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socketClient.ConnectAsync(IP_address, 8888);

            SendMessage("/newUser " + userName);
            ReceivingMessage();
        }
        private async Task LBX_Add(string name)
        {
            _allUsers.Add(name);
            UpdateUsersListBox();
        }
        private async Task LBX_Remove(string name)
        {
            _allUsers.Remove(name);
            UpdateUsersListBox();
        }
        public async Task UpdateUsersListBox()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ClientWindow = Application.Current.Windows.OfType<ClientWindow>().FirstOrDefault();
                if (ClientWindow != null)
                {
                    ClientWindow.ListBoxItems = _allUsers;
                }
            });
        }
        private void Message_Add(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ClientWindow = Application.Current.Windows.OfType<ClientWindow>().FirstOrDefault();
                if (ClientWindow != null)
                {
                    ClientWindow.MessageItems = message;
                }
            });
        }
        public async Task ReceivingMessage()
        { 
            while (true)
            {
                byte[] bytes = new byte[1024];

                await Task.Run(async () =>
                {
                    await socketClient.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                });

                string receivedMessage = Encoding.UTF8.GetString(bytes);

                if (receivedMessage.StartsWith("/newUser")) { }
                else if (receivedMessage.StartsWith("/listUsers"))
                {
                    string realNames = receivedMessage.Substring(10);

                    char[] separator = new char[] { '~' };
                    users = realNames.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    _allUsers.Clear();
                    foreach (string user in users)
                    {
                        LBX_Add(user);
                    }
                }
                else if (receivedMessage.StartsWith("/username"))
                {
                    receivedMessage = receivedMessage.TrimEnd('\0');
                    string[] lol = receivedMessage.Replace("/message ", "#").Split('#');

                    string userName = lol[0].Substring(9).Trim();
                    if (!(userName.Trim().StartsWith("[") && userName.Trim().EndsWith("]"))) userName = $"[{userName}]";
                    string message = lol[1];

                    Message_Add($"[{DateTime.Now}] {userName}: {message}");
                }
                else if (receivedMessage.StartsWith("/stopChat"))
                {
                    cts.Dispose();
                    CloseConnection();
                    ClientWindow clientWindow = Application.Current.MainWindow as ClientWindow;
                    clientWindow.isClosed = true;
                    ((ClientWindow)Application.Current.MainWindow).Close();
                }
                else
                {
                    string stringWithoutNulls = receivedMessage.Replace("\0", string.Empty);
                    if (!string.IsNullOrEmpty(stringWithoutNulls.Trim()))
                    {
                        Message_Add($"[{DateTime.Now}] [{userName}]: {stringWithoutNulls}");
                    }
                }
            }
        }
        public async Task SendMessage(string receivedMessage)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(receivedMessage);
            await socketClient.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
        }
        public void CloseConnection()
        {
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Dispose();
        }
    }
}