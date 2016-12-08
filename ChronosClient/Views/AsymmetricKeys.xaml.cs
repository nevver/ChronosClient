using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Text;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AsymmetricKeys : Page
    {
        //private static IBuffer buffKeyPair;
        private IBuffer buffDecryptedSessionKey;
        private static string strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaPkcs1;
        private static UInt32 asymmetricKeyLength = 2048;


        public AsymmetricKeys()
        {
            this.InitializeComponent();

            continueButton.Visibility = Visibility.Collapsed;
            continueButton.IsEnabled = false;


            //// Create a symmetric session key
            //String strSymmetricAlgName = SymmetricAlgorithmNames.AesCbc;
            //UInt32 symmetricKeyLength = 32;
            //IBuffer buffSessionKey;
            //this.createSymmetricSessionKey(
            //    strSymmetricAlgName,
            //    symmetricKeyLength,
            //    out buffSessionKey);

            ////test
            //String text = CryptographicBuffer.EncodeToBase64String(buffSessionKey);
            //Debug.WriteLine("Symmetric public Key only: " + text);


            // Create an asymmetric key pair and export the public key.
            //String strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaPkcs1;
            //UInt32 asymmetricKeyLength = 512;
            //IBuffer buffPublicKey;
            //this.createAsymmetricKeyPair(
            //    strAsymmetricAlgName,
            //    asymmetricKeyLength,
            //    out buffPublicKey);

            ////test
            //String text2 = CryptographicBuffer.EncodeToBase64String(buffPublicKey);
            //Debug.WriteLine("Asymmetric public Key only:" + text2);


            //// Encrypt the symmetric session key by using the asymmetric public key.
            //IBuffer buffEncryptedSessionKey;
            //this.asymmetricEncryptSessionKey(
            //    strAsymmetricAlgName,
            //    buffSessionKey,
            //    buffPublicKey,
            //    out buffEncryptedSessionKey);

            ////test
            //String text3 = CryptographicBuffer.EncodeToBase64String(buffEncryptedSessionKey);
            //Debug.WriteLine("Encrypt symmetric Key by using asymmetric public key:" + text3);


            //// Decrypt the symmetric session key by using the asymmetric private key
            //// that corresponds to the public key used to encrypt the session key.
            //this.asymmetricDecryptSessionKey(
            //    strAsymmetricAlgName,
            //    strSymmetricAlgName,
            //    buffEncryptedSessionKey);

            ////test 
            //String text4 = CryptographicBuffer.EncodeToBase64String(buffDecryptedSessionKey);
            //Debug.WriteLine("Decrypt symmetric Key by using asymmetric private key that corresponds to the public key we used to encrypt:" + text4);

        }

        /// <summary>
        /// Directory selector dor public key export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dirSelector_Click(object sender, RoutedEventArgs e)
        {
            dirSelectorButton.IsEnabled = false;
            dirSelectorButton.Visibility = Visibility.Collapsed;
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".PublicKey");

            // Picks folder to write to
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                 FutureAccessList.AddOrReplace("PickedFolderToken", folder);

               
               // create pubKeyExport file in chosen directory 
                Windows.Storage.StorageFile pubKeyFile =
                    await folder.CreateFileAsync(DataContainer.User + ".PublicKey",
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // generate asymmetric keys
               // IBuffer buffPublicKey;
                this.createAsymmetricKeyPair(
                    strAsymmetricAlgName,
                    asymmetricKeyLength,
                    out DataContainer.senderPublicKey);

                // encode buffer into a ASCII string 
                string publicKeyEncded = encodeBuffTo64BaseString(DataContainer.senderPublicKey);


                // write to the folder selected by user
                await Windows.Storage.FileIO.WriteTextAsync(pubKeyFile, publicKeyEncded);

                // create keypair file for application use only for the user that is logged in
                string keyPairName = DataContainer.User + ".KeyPair";
                DataContainer.KeyPairFileName = keyPairName;
                Windows.Storage.StorageFolder localFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile keyPair =
                    await localFolder.CreateFileAsync((DataContainer.KeyPairFileName),
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // encode buffer into a ASCII string 
                string keyPairEncod = encodeBuffTo64BaseString(DataContainer.senderKeyPair);

                // write to the file the app uses
                await Windows.Storage.FileIO.WriteTextAsync(keyPair, keyPairEncod);

            }

            continueButton.Visibility = Visibility.Visible;
            continueButton.IsEnabled = true;

        }

        public IBuffer decode64BaseString (string str)
        {
            IBuffer buffFromBase64String = CryptographicBuffer.DecodeFromBase64String(str);
            return buffFromBase64String;
        }

        public string encodeBuffTo64BaseString(IBuffer buff)
        {
            string strBase64 = CryptographicBuffer.EncodeToBase64String(buff);
            return strBase64;
        }

        public void createSymmetricSessionKey(
    string strSymmetricAlgName,
    UInt32 keyLength,
    out IBuffer buffSessionKey)
        {
            // Open a symmetric algorithm provider for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strSymmetricAlgName);

            // Create a symmetric session key.
            IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(keyLength);
            CryptographicKey sessionKey = objAlg.CreateSymmetricKey(keyMaterial);

            buffSessionKey = keyMaterial;
        }

        public void createAsymmetricKeyPair(
            String strAsymmetricAlgName,
            UInt32 keyLength,
            out IBuffer buffPublicKey)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Demonstrate use of the AlgorithmName property (not necessary to create a key pair).
            String strAlgName = objAlgProv.AlgorithmName;

            // Create an asymmetric key pair.
            CryptographicKey keyPair = objAlgProv.CreateKeyPair(keyLength);

            // Export the public key to a buffer for use by others.
            buffPublicKey = keyPair.ExportPublicKey();

            //public key to internal buffer
            DataContainer.senderPublicKey = buffPublicKey;

            // You should keep your private key (embedded in the key pair) secure. For  
            // the purposes of this example, however, we're just copying it into a
            // static class variable for later use during decryption.
            DataContainer.senderKeyPair = keyPair.Export();
        }

        // method to encrypt plain text
        public void asymemtricEncryptMessageBody(
            String strAsymmetricAlgName,
            IBuffer buffMessageToEncrypt,
            IBuffer buffPublicKey,
            out IBuffer buffEncryptedMessage)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(buffPublicKey);

            // Encrypt the session key by using the public key.
            buffEncryptedMessage = CryptographicEngine.Encrypt(publicKey, buffMessageToEncrypt, null);

        }

        // method to decrypt ciphertext
        public void asymmetricDecryptMessageBody(
    String strAsymmetricAlgName,
    String strSymmetricAlgName,
    IBuffer buffEncryptedMessageBody)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer. You should keep your private key
            // secure. For the purposes of this example, however, the private key is
            // just stored in a static class variable.
            CryptographicKey keyPair = objAsymmAlgProv.ImportKeyPair(DataContainer.senderKeyPair);

            // Use the private key embedded in the key pair to decrypt the session key.
            buffDecryptedSessionKey = CryptographicEngine.Decrypt(keyPair, buffEncryptedMessageBody, null);

            // Convert the decrypted session key into a CryptographicKey object that
            // can be used to decrypt the message that it previously encrypted (not shown).
            SymmetricKeyAlgorithmProvider objSymmAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strSymmetricAlgName);
            CryptographicKey sessionKey = objSymmAlgProv.CreateSymmetricKey(buffDecryptedSessionKey);
        }

        public void asymmetricEncryptSessionKey(
            String strAsymmetricAlgName,
            IBuffer buffSessionKeyToEncrypt,
            IBuffer buffPublicKey,
            out IBuffer buffEncryptedSessionKey)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(buffPublicKey);

            // Encrypt the session key by using the public key.
            buffEncryptedSessionKey = CryptographicEngine.Encrypt(publicKey, buffSessionKeyToEncrypt, null);
        }

        public void asymmetricDecryptSessionKey(
            String strAsymmetricAlgName,
            String strSymmetricAlgName,
            IBuffer buffEncryptedSessionKey)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer. You should keep your private key
            // secure. For the purposes of this example, however, the private key is
            // just stored in a static class variable.
            CryptographicKey keyPair = objAsymmAlgProv.ImportKeyPair(DataContainer.senderKeyPair);

            // Use the private key embedded in the key pair to decrypt the session key.
            buffDecryptedSessionKey = CryptographicEngine.Decrypt(keyPair, buffEncryptedSessionKey, null);

            // Convert the decrypted session key into a CryptographicKey object that
            // can be used to decrypt the message that it previously encrypted (not shown).
            SymmetricKeyAlgorithmProvider objSymmAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strSymmetricAlgName);
            CryptographicKey sessionKey = objSymmAlgProv.CreateSymmetricKey(buffDecryptedSessionKey);
        }

        private void continue_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ListUsers));
        }

        private void Logout_Checked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }
    }


}
