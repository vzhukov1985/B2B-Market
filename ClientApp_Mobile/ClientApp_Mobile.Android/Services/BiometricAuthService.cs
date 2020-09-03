using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Hardware.Biometrics;
using Android.OS;
using Android.Security.Keystore;
using Android.Support.V4.App;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Util;
using Android.Widget;
using ClientApp_Mobile.Droid.Services;
using ClientApp_Mobile.Services;
using Java.Security;
using Javax.Crypto;

[assembly: Xamarin.Forms.Dependency(typeof(BiometricAuthService))]
namespace ClientApp_Mobile.Droid.Services
{
    public class BiometricAuthService : IBiometricAuthenticateService
    {
        readonly Context context = Android.App.Application.Context;
        private KeyStore keyStore;
        private Cipher cipher;
        private readonly string KEY_NAME = "B2BMarket";
        public static bool IsAutSucess;

        public string GetAuthenticationType()
        {
            return "";
        }

        public Task<bool> AuthenticateUserIDWithTouchID()
        {
            var tcs = new TaskCompletionSource<bool>();//used to wait the mainUI to get the response of the touchId

            KeyguardManager keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
            FingerprintManagerCompat fingerprintManager = FingerprintManagerCompat.From(context);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                if (ActivityCompat.CheckSelfPermission(context, Manifest.Permission.UseBiometric)
                    != (int)Android.Content.PM.Permission.Granted)
                    return tcs.Task;
                if (!fingerprintManager.IsHardwareDetected)
                    Toast.MakeText(context, "Разрешение на вход по отпечатку пальца не дано", ToastLength.Short).Show();
                else
                {
                    if (!fingerprintManager.HasEnrolledFingerprints)
                        Toast.MakeText(context, "Установите хотя бы один отпечаток пальца в настройках Android", ToastLength.Short).Show();
                    else
                    {
                        if (!keyguardManager.IsKeyguardSecure)
                            Toast.MakeText(context, "Экран блокировки не установлен в настройках Android", ToastLength.Short).Show();
                        else
                            GenKey();
                        if (CipherInit())
                        {
                            FingerprintManagerCompat.CryptoObject cryptoObject = new FingerprintManagerCompat.CryptoObject(cipher);
                            BiometricHandler handler = new BiometricHandler();
                            handler.StartAuthentication(fingerprintManager, cryptoObject);
                        }
                    }
                }
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (ActivityCompat.CheckSelfPermission(context, Manifest.Permission.UseFingerprint)
#pragma warning restore CS0618 // Type or member is obsolete
                    != (int)Android.Content.PM.Permission.Granted)
                    return tcs.Task;
                if (!fingerprintManager.IsHardwareDetected)
                    Toast.MakeText(context, "Разрешение на вход по отпечатку пальца не дано", ToastLength.Short).Show();
                else
                {
                    if (!fingerprintManager.HasEnrolledFingerprints)
                        Toast.MakeText(context, "Установите хотя бы один отпечаток пальца в настройках Android", ToastLength.Short).Show();
                    else
                    {
                        if (!keyguardManager.IsKeyguardSecure)
                            Toast.MakeText(context, "Экран блокировки не установлен в настройках Android", ToastLength.Short).Show();
                        else
                            GenKey();
                        if (CipherInit())
                        {
                            FingerprintManagerCompat.CryptoObject cryptoObject = new FingerprintManagerCompat.CryptoObject(cipher);
                            BiometricHandler handler = new BiometricHandler();
                            handler.StartAuthentication(fingerprintManager, cryptoObject);
                        }
                    }
                }
            }
            else
            {
                return tcs.Task;
            }
            tcs.SetResult(IsAutSucess);
            return tcs.Task;
        }

        private bool CipherInit()
        {
            try
            {
                cipher = Cipher.GetInstance(KeyProperties.KeyAlgorithmAes
                    + "/"
                    + KeyProperties.BlockModeCbc
                    + "/"
                    + KeyProperties.EncryptionPaddingPkcs7);
                keyStore.Load(null);
                IKey key = (IKey)keyStore.GetKey(KEY_NAME, null);
                cipher.Init(CipherMode.EncryptMode, key);
                return true;
            }
            catch { return false; }
        }
        private void GenKey()
        {
            keyStore = KeyStore.GetInstance("AndroidKeyStore");
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            KeyGenerator keyGenerator = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
            keyStore.Load(null);
            keyGenerator.Init(new KeyGenParameterSpec.Builder(KEY_NAME, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeCbc)
                .SetUserAuthenticationRequired(true)
                .SetEncryptionPaddings(KeyProperties
                .EncryptionPaddingPkcs7).Build());
            keyGenerator.GenerateKey();
        }

        public bool fingerprintEnabled()
        {
            Activity activity = MainActivity.FormsContext;
            KeyguardManager keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
            FingerprintManagerCompat fingerprintManager = FingerprintManagerCompat.From(context);

            /*
             *Condition I : Check if the andoid version is device is greater than
             *Pie, since Biometrics is supported by greater devices
             *no fingerprint manager from Android >9.0
             */

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                if (ActivityCompat.CheckSelfPermission(context, Manifest.Permission.UseBiometric) == Android.Content.PM.Permission.Granted)
                {
                    if (fingerprintManager != null && fingerprintManager.IsHardwareDetected)
                    {
                        if (keyguardManager.IsKeyguardSecure)
                        {
                            if (fingerprintManager.HasEnrolledFingerprints)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.UseBiometric }, 200);
                    return false;
                }
            }

            /*
             *Condition II: check if the android device version is greater than Marshmallow, 
             *since fingerprint authenticatio is only supported from Android 6.0
             */

            else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {

#pragma warning disable CS0618 // Type or member is obsolete
                if (ActivityCompat.CheckSelfPermission(context, Manifest.Permission.UseFingerprint) == Android.Content.PM.Permission.Granted)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    if (fingerprintManager != null && fingerprintManager.IsHardwareDetected)
                    {
                        if (keyguardManager.IsKeyguardSecure)
                        {
                            if (fingerprintManager.HasEnrolledFingerprints)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.UseFingerprint }, 200);
#pragma warning restore CS0618 // Type or member is obsolete
                    return false;
                }
            }

            /*
             *Lower version don't support for biometric authentication
             */
            else
            {
                return false;
            }


        }
    }
}