using Core.DBModels;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public class AppSettings
    {
        public static CurrentUserInfo CurrentUser;

        public static AppLocalUsers AppLocalUsers = new AppLocalUsers();
        public static int CurrentUserAppLocalUsersIndex;

        public static List<ArchivedRequestStatusType> ArchivedOrderStatusTypes;
    }



    public class AppLocalUser
    {
        public int Index { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string CompanyName { get; set; }
        public bool UsePINAccess { get; set; }
        public bool UseBiometricAccess { get; set; }

        public string DisplayName { get => $"{CompanyName} - {Name} {Surname}"; }

        public void ReadAppUserPreferences(int index)
        {
            Index = index;
            Login = Preferences.Get($"User{Index}Login", "");
            Name = Preferences.Get($"User{Index}Name", "");
            Surname = Preferences.Get($"User{Index}Surname", "");
            CompanyName = Preferences.Get($"User{Index}Company", "");
            UsePINAccess = Preferences.Get($"User{Index}PINAccess", false);
            UseBiometricAccess = Preferences.Get($"User{Index}BiometricAccess", false);
        }

        public void UpdateCurrentUserPreferences(int index)
        {
            Index = index;
            var user = AppSettings.CurrentUser;
            Login = user.Login;
            Name = user.Name;
            Surname = user.Surname;
            CompanyName = user.Client.ShortName;
            UsePINAccess = !string.IsNullOrEmpty(user.PinHash);
            UseBiometricAccess = user.UseBiometricAccess;
            UpdateAppUserPreferences();
        }

        public void UpdateAppUserPreferences()
        {
            Preferences.Set($"User{Index}Login", Login);
            Preferences.Set($"User{Index}Name", Name);
            Preferences.Set($"User{Index}Surname", Surname);
            Preferences.Set($"User{Index}Company", CompanyName);
            Preferences.Set($"User{Index}PINAccess", UsePINAccess);
            Preferences.Set($"User{Index}BiometricAccess", UseBiometricAccess);
        }

        public AppLocalUser()
        {

        }
    }


    public class AppLocalUsers : List<AppLocalUser>
    {
        private int CurrentUserIndex { get; set; }
        public int LastEnterUserIndex { get; set; }


        public AppLocalUser CurrentUser
        {
            get => this[CurrentUserIndex];
        }

        public bool UserExistsInApp()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Login == AppSettings.CurrentUser.Login)
                {
                    return true;
                }
            }
            return false;
        }

        public void RegisterNewUser()
        {
            Add(new AppLocalUser());
            Preferences.Set("UsersCount", Count);
            CurrentUserIndex = Count - 1;
            LastEnterUserIndex = CurrentUserIndex;
            UpdateCurrentUserPreferences();
            Preferences.Set("LastEnterUserIndex", CurrentUserIndex);
        }

        public void RegisterExistingUser()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Login == AppSettings.CurrentUser.Login)
                {
                    CurrentUserIndex = i;
                    LastEnterUserIndex = CurrentUserIndex;
                    AppSettings.CurrentUser.UseBiometricAccess = this[CurrentUserIndex].UseBiometricAccess;
                    UpdateCurrentUserPreferences();
                    Preferences.Set("LastEnterUserIndex", CurrentUserIndex);
                    break;
                }
            }

        }

        public void DisableAllUsersBiometricAccess()
        {
            foreach (var user in this)
            {
                user.UseBiometricAccess = false;
                user.UpdateAppUserPreferences();
            }
        }

        public void RemoveAppUser(AppLocalUser user)
        {
            Remove(user);
            Preferences.Clear();
            Preferences.Set("UsersCount", Count);
            Preferences.Set("LastEnterUserIndex", 0);
            int i = 0;
            foreach (var luser in this)
            {
                luser.Index = i;
                luser.UpdateAppUserPreferences();
                i++;
            }
        }

        public void UpdateCurrentUserPreferences()
        {
            this[CurrentUserIndex].UpdateCurrentUserPreferences(CurrentUserIndex);
        }

        public void ReadAppUserPreferences()
        {
            Clear();
            if (Preferences.ContainsKey("UsersCount"))
            {
                for (int i = 0; i < Preferences.Get("UsersCount", 0); i++)
                {
                    Add(new AppLocalUser());
                    this[i].ReadAppUserPreferences(i);
                }
            }
            else
            {
                Preferences.Set("UsersCount", 0);
            }
            LastEnterUserIndex = Preferences.Get("LastEnterUserIndex", 0);
        }
    }
}

