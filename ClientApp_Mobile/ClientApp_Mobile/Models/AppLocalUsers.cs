using ClientApp_Mobile.Services;
using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClientApp_Mobile.Models
{
    public class AppLocalUser
    {
        public int Index { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string CompanyName { get; set; }
        public bool PINAccess { get; set; }
        public bool BiometricAccess { get; set; }

        public string DisplayName { get => $"{CompanyName} - {Name} {Surname}"; }

        public void ReadAppUserPreferences(int index)
        {
            Index = index;
            Id = new Guid(Preferences.Get($"User{Index}Id", Guid.Empty.ToString()));
            Name = Preferences.Get($"User{Index}Name", "");
            Surname = Preferences.Get($"User{Index}Surname", "");
            CompanyName = Preferences.Get($"User{Index}Company", "");
            PINAccess = Preferences.Get($"User{Index}PINAccess", false);
            BiometricAccess = Preferences.Get($"User{Index}BiometricAccess", false);
        }

        public void UpdateCurrentUserPreferences(int index)
        {
            Index = index;
            var user = UserService.CurrentUser;
            Id = user.Id;
            Name = user.Name;
            Surname = user.Surname;
            CompanyName = user.Client.ShortName;
            PINAccess = !string.IsNullOrEmpty(user.PinHash);
            BiometricAccess = user.UseBiometricAccess;
            UpdateAppUserPreferences();
        }

        public void UpdateAppUserPreferences()
        {
            Preferences.Set($"User{Index}Id", Id.ToString());
            Preferences.Set($"User{Index}Name", Name);
            Preferences.Set($"User{Index}Surname", Surname);
            Preferences.Set($"User{Index}Company", CompanyName);
            Preferences.Set($"User{Index}PINAccess", PINAccess);
            Preferences.Set($"User{Index}BiometricAccess", BiometricAccess);
        }

        public AppLocalUser()
        {

        }
    }


    public class AppLocalUsers : List<AppLocalUser>
    {
        public int CurrentUserIndex { get; set; }
        public int LastEnterUserIndex { get; set; }


        public bool UserExistsInApp()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Id == UserService.CurrentUser.Id)
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
                if (this[i].Id == UserService.CurrentUser.Id)
                {
                    CurrentUserIndex = i;
                    LastEnterUserIndex = CurrentUserIndex;
                    UserService.CurrentUser.UseBiometricAccess = this[CurrentUserIndex].BiometricAccess;
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
                user.BiometricAccess = false;
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
