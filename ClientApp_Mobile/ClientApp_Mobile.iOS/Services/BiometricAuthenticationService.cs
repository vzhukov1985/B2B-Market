using System;
using System.Threading.Tasks;
using ClientApp_Mobile.iOS.Services;
using ClientApp_Mobile.Services;
using Foundation;
using LocalAuthentication;
using UIKit;
using Xamarin.Forms;


[assembly: Xamarin.Forms.Dependency(typeof(BiometricAuthenticationService))]
namespace ClientApp_Mobile.iOS.Services
{
    public class BiometricAuthenticationService : IBiometricAuthenticateService
    {
        public BiometricAuthenticationService()
        {
        }

        public Task<bool> AuthenticateUserIDWithTouchID()
        {
            bool outcome = false;
            var tcs = new TaskCompletionSource<bool>();

            var context = new LAContext();
            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out NSError AuthError))
            {


                var replyHandler = new LAContextReplyHandler((success, error) => {

                    Device.BeginInvokeOnMainThread(() => {
                        if (success)
                        {
                            outcome = true;
                        }
                        else
                        {
                            outcome = false;
                        }
                        tcs.SetResult(outcome);
                    });

                });
                //This will call both TouchID and FaceId 
                context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, "Войдите с использованием Touch ID", replyHandler);
            };
            return tcs.Task;
        }

        public bool fingerprintEnabled()
        {
            throw new NotImplementedException();
        }

        private static int GetOsMajorVersion()
        {
            return int.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
        }

        public string GetAuthenticationType()
        {
            var localAuthContext = new LAContext();
#pragma warning disable IDE0018 // Inline variable declaration
            NSError AuthError;
#pragma warning restore IDE0018 // Inline variable declaration

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out AuthError))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out AuthError))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                {
                    if (GetOsMajorVersion() >= 11 && localAuthContext.BiometryType == LABiometryType.FaceId)
                    {
                        return "FaceId";
                    }

                    return "TouchId";
                }

                return "PassCode";
            }

            return "None";
        }
    }
}