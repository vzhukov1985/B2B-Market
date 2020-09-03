using System;
using Android.Content;
using Android.Hardware.Fingerprints;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V4.OS;
using Android.Widget;
using Xamarin.Forms;

namespace ClientApp_Mobile.Droid.Services
{
    public class BiometricHandler : FingerprintManagerCompat.AuthenticationCallback
    {
        private CancellationSignal cancellationSignal;
        public BiometricHandler()
        {

        }
        internal void StartAuthentication(FingerprintManagerCompat fingerprintManager, FingerprintManagerCompat.CryptoObject cryptoObject)
        {
            cancellationSignal = new CancellationSignal();
            fingerprintManager.Authenticate(cryptoObject, 0, cancellationSignal, this, null);
            MessagingCenter.Unsubscribe<string>("AndroidAuth", "Cancel");
            MessagingCenter.Subscribe<string>("AndroidAuth", "Cancel", _ => StopAuthentication());
        }
        public override void OnAuthenticationFailed()
        {
            BiometricAuthService.IsAutSucess = false;
            MessagingCenter.Send<string>("AndroidAuth", "Fail");
        }
        public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        {
            BiometricAuthService.IsAutSucess = true;
            MessagingCenter.Send<string>("AndroidAuth", "Success");
        }

        internal void StopAuthentication()
        {
            cancellationSignal?.Cancel();
        }
    }
}