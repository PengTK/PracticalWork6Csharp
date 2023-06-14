using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;

namespace Practical_6_WPF__Messenger_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool comeIN;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void NewChat_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!UserName_TXT.Text.Contains("~") && !UserName_TXT.Text.Contains("#") && !UserName_TXT.Text.Contains("[") && !UserName_TXT.Text.Contains("]"))
            {
                comeIN = CheckFields(true);
                if (!comeIN) return;

                OwnerWindow ownerWindow = new OwnerWindow(UserName_TXT.Text.Trim());
                ownerWindow.Show();
                Close();
            } else MessageBox.Show("В имени запрещено использовать символы ~ # [ ]");
        }
        private bool CheckFields(bool FirstOrAll)
        {
            switch (FirstOrAll)
            {
                case true:
                    if (UserName_TXT.Text.Trim() == "")
                    {
                        MessageBox.Show("Имя пользователя не введено");
                        return false;
                    }
                    break;

                case false:
                    if (UserName_TXT.Text.Trim() == "" || IPAddress_TXT.Text.Trim() == "")
                    {
                        MessageBox.Show("Не все поля заполнены");
                        return false;
                    }
                    break;
            }
            return true;
        }

        private void OpenChat_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!UserName_TXT.Text.Contains("~") && !UserName_TXT.Text.Contains("#") && !UserName_TXT.Text.Contains("[") && !UserName_TXT.Text.Contains("]"))
            {
                if (UserName_TXT.Text != "")
                {
                    CheckFields(false);
                    if (comeIN) return;

                    if (IPAddress_TXT.Text.Length > 5)
                    {
                        MessageBox.Show("Чат по данному IP не найден.");
                        return;
                    }

                    ClientWindow userWindow = new ClientWindow(UserName_TXT.Text.Trim(), IPAddress_TXT.Text.Trim());
                    userWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Имя пользователя не введено");
                }
            } else MessageBox.Show("В имени запрещено использовать символы ~ # [ ]");
        }
    }
}
