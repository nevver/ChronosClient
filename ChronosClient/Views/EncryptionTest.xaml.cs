using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChronosClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EncryptionTest : Page
    {
        private string plaintext; // Message to encrypt and authenticate.
        private string strAsymmetricAlgName = AsymmetricAlgorithmNames.RsaOaepSha512;
        private UInt32 asymmetricKeyLength = 2048;
        private IBuffer pubKey;
        private IBuffer keys;
        private IBuffer encryptedMessage;
        private IBuffer decryptedMessage;
        // Initialize a static nonce value.
        static byte[] NonceBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // RSA key pair
        CryptographicKey keyPair;

        // Initialize the symmetric encryption
        private static String strSymAlgName = SymmetricAlgorithmNames.AesGcm;
        private static UInt32 keyLength = 256;  // Length of the key, in bytes
        BinaryStringEncoding encoding;          // Binary encoding
        IBuffer buffNonce;                      // Nonce

        // AES key
        CryptographicKey key;                   // Symmetric key

        public EncryptionTest()
        {
            this.InitializeComponent();
            encrypt.IsEnabled = false;
            decrypt.IsEnabled = false;
        }

        private void plain_TextChanged(object sender, TextChangedEventArgs e)
        {
            plaintext = plain.Text;
        }

        private void cipher_TextChanged(object sender, TextChangedEventArgs e)
        {
            plaintext = cipher.Text;
        }


        private void createKeys_Click(object sender, RoutedEventArgs e)
        {

            if (keyPair == null)
            {
                createAsymmetricKeyPair(strAsymmetricAlgName, asymmetricKeyLength, out pubKey);
                update_StatusBar("blue");
                status_Text.Text = "Asymmetric keys created.";
                encrypt.IsEnabled = true;

            }
            else
            {
                update_StatusBar("green");
                status_Text.Text = "Asymmetric keys already exist.";
            }

            
            
        }

        private void encrypt_Click(object sender, RoutedEventArgs e)
        {
            encrypt.IsEnabled = false;
            if (keyPair == null)
            {
                update_StatusBar("red");
                status_Text.Text = "No key pair. Create a key pair before testing encryption.";
            }
            else if (plaintext == null)
            {
                update_StatusBar("red");
                status_Text.Text = "No plain text to encrypt.";
            }

            //// Encrypt and authenticate the message.
            //EncryptedAndAuthenticatedData objEncrypted = this.AuthenticatedEncryption(
            //    plaintext,
            //    strSymAlgName,
            //    keyLength,
            //    out buffNonce);
            asymemtricEncryptMessageBody(strAsymmetricAlgName, plaintext);
            cipher.Text = plaintext;
            plain.Text = plaintext;
            decrypt.IsEnabled = true;

        }

        // Encryption and authentication method
        public EncryptedAndAuthenticatedData AuthenticatedEncryption(
            String strMsg,
            String strAlgName,
            UInt32 keyLength,
            out IBuffer buffNonce)
        {
            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a buffer that contains the data to be encrypted.
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Generate a symmetric key.
            IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(keyLength);
            key = objAlgProv.CreateSymmetricKey(keyMaterial);

            // Generate a new nonce value.
            buffNonce = GetNonce();

            // Encrypt and authenticate the message.
            EncryptedAndAuthenticatedData objEncrypted = CryptographicEngine.EncryptAndAuthenticate(
                key,
                buffMsg,
                buffNonce,
                null);

            return objEncrypted;

        }

        IBuffer GetNonce()
        {
            // Security best practises require that an ecryption operation not
            // be called more than once with the same nonce for the same key.
            // A nonce value can be predictable, but must be unique for each
            // secure session.

            NonceBytes[0]++;
            for (int i = 0; i < NonceBytes.Length - 1; i++)
            {
                if (NonceBytes[i] == 255)
                {
                    NonceBytes[i + 1]++;
                }
            }

            return CryptographicBuffer.CreateFromByteArray(NonceBytes);
        }

        private void decrypt_Click(object sender, RoutedEventArgs e)
        {
            decrypt.IsEnabled = false;
            
            if (plaintext == null)
            {
                update_StatusBar("red");
                status_Text.Text = "No cipher text to decrypt.";
            }
            asymmetricDecryptMessageBody(strAsymmetricAlgName, encryptedMessage);
            plain.Text = plaintext;
            cipher.Text = plaintext;
            encrypt.IsEnabled = true;

        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
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
            keyPair = objAlgProv.CreateKeyPair(keyLength);

            // Export the public key to a buffer for use by others.
            buffPublicKey = keyPair.ExportPublicKey();

            keys = keyPair.Export();
            
        }

        // method to encrypt plain text
        public void asymemtricEncryptMessageBody(
            String strAsymmetricAlgName,
            String text)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(pubKey);

            IBuffer plain = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);

            // Encrypt message by using the public key.
            encryptedMessage = CryptographicEngine.Encrypt(publicKey, plain, null);

            //convert to plaintext
            plaintext = CryptographicBuffer.EncodeToBase64String(encryptedMessage);

        }

        // method to decrypt ciphertext
        public void asymmetricDecryptMessageBody(
    String strAsymmetricAlgName,
    IBuffer buffEncryptedMessageBody)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Use the private key embedded in the key pair to decrypt the session key.
            decryptedMessage = CryptographicEngine.Decrypt(keyPair, buffEncryptedMessageBody, null);

            //convert to string
            plaintext = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedMessage);

        }

        /// <summary>
        /// Updates status bar background color
        /// </summary>
        /// <param name="color"></param>
        private void update_StatusBar(string color)
        {
            if (color == "blue")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
            }

            if (color == "red")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 20, 0));
            }

            if (color == "green")
            {
                status_Bar.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 177, 76));
            }

        }
    }
}
