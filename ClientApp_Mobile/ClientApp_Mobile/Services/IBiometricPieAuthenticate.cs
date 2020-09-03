using System;

namespace ClientApp_Mobile.Services
{
    public interface IBiometricPieAuthenticate
    {
        void RegisterOrAuthenticate();

        bool CheckSDKGreater29();
    }
}
