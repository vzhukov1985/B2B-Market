using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp_Mobile.Services
{
    public interface IBiometricAuthenticateService
    {
        String GetAuthenticationType();
        Task<bool> AuthenticateUserIDWithTouchID();
        bool fingerprintEnabled();
    }
}
