using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Practical_6_WPF__Messenger_
{
    public static class AllUsers
    {
        public static List<string> allUsers = new List<string>();

        public static  async Task AddUser(string userName)
        {
            allUsers.Add(userName);
            UpdateListBox();
        }

        public static async Task RemoveUser(string userName)
        {
            allUsers.Remove(userName);
            UpdateListBox();
        }

        public static async Task UpdateListBox()
        {
            await Task.Run(() =>
            {
                foreach (var window in Application.Current.Windows)
                {
                    if (window is UserWindow userWindow)
                    {
                        userWindow.Users_LBX.ItemsSource = allUsers;
                    }
                    else if (window is OwnerWindow ownerWindow)
                    {
                        ownerWindow.Users_LBX.ItemsSource = allUsers;
                    }
                }
            });
        }
    }

    /* public async static Task UpdateListBox(string item)
     {
         await Task.Run(() =>
         {
             allUsers.Add(item);
         });

         *//*allUsers.Add(item);*/
    /*Application.Current.Dispatcher.InvokeAsync(() =>
    {
        allUsers.Add(item);
    });*//*
}*/
}
