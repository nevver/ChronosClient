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

        private IBuffer decryptedAESKeyBuff;
        // Initialize a static nonce value.
        static byte[] NonceBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // RSA key pair
        CryptographicKey keyPair;

        // Initialize the symmetric encryption
        private static String strSymAlgName = SymmetricAlgorithmNames.AesGcm;
        private static UInt32 keyLength = 64;  // Length of the key, in bytes
        BinaryStringEncoding encoding  = BinaryStringEncoding.Utf8;          // Binary encoding
        IBuffer buffNonce;                      // Nonce

        // AES key
        CryptographicKey key;                   // Symmetric key
        private IBuffer encryptedMessageData;
        private IBuffer authenticationTag;
        private IBuffer keyMaterial;
        private IBuffer encryptedAESKeyBuff;
        private IBuffer buffDecrypted;
        private string keyMaterialString;
        private string encryptedMessageDataString;
        private string authenticationTagString;
        private string encryptedKeyString;
        private string nonceString;
        private string decryptedAesKeyString;

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

            // Encrypt and authenticate the message.
            AuthenticatedEncryption(
                plaintext,
                strSymAlgName,
                keyLength);
            asymemtricEncryptAESKey(strAsymmetricAlgName, keyMaterialString);
            cipher.Text = plaintext;
            plain.Text = plaintext;
            decrypt.IsEnabled = true;

        }

        // Encryption and authentication method
        private void AuthenticatedEncryption(
            String strMsg,
            String strAlgName,
            UInt32 keyLength)
        {
            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a buffer that contains the data to be encrypted.
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Generate a symmetric key.
            keyMaterial = CryptographicBuffer.GenerateRandom(keyLength);
            keyMaterialString = CryptographicBuffer.EncodeToBase64String(keyMaterial);
            key = objAlgProv.CreateSymmetricKey(keyMaterial);

            // Generate a new nonce value.
            buffNonce = GetNonce();

            // Encrypt and authenticate the message.
            EncryptedAndAuthenticatedData objEncrypted = CryptographicEngine.EncryptAndAuthenticate(
                key,
                buffMsg,
                buffNonce,
                null);

            encryptedMessageData = objEncrypted.EncryptedData;
            authenticationTag = objEncrypted.AuthenticationTag;
            encryptedMessageDataString = CryptographicBuffer.EncodeToBase64String(encryptedMessageData);
            authenticationTagString = CryptographicBuffer.EncodeToBase64String(authenticationTag);
            nonceString = CryptographicBuffer.EncodeToBase64String(buffNonce);
            plaintext = encryptedMessageDataString;
            tag.Text = authenticationTagString;
            nonce.Text = nonceString;
            
        }

        public void AuthenticatedDecryption(
            String strAlgName,
            String buffNonce)
        {

            // Open a SymmetricKeyAlgorithmProvider object for the specified algorithm.
            SymmetricKeyAlgorithmProvider objAlgProv = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            
            IBuffer encryptedAESKey = CryptographicBuffer.DecodeFromBase64String(encryptedKeyString);

            asymmetricDecryptAESKey(strAsymmetricAlgName, encryptedAESKeyBuff);

            IBuffer decryptedAESKeyBuff = CryptographicBuffer.DecodeFromBase64String(decryptedAesKeyString);
            CryptographicKey keyFromEncryptedString = objAlgProv.CreateSymmetricKey(decryptedAESKeyBuff);

            IBuffer encryptedDataFromStringBuff = CryptographicBuffer.DecodeFromBase64String(plaintext);
            IBuffer nonceFromString = CryptographicBuffer.DecodeFromBase64String(nonceString);
            IBuffer authenticationTagFromString = CryptographicBuffer.DecodeFromBase64String(authenticationTagString);

            // The input key must be securely shared between the sender of the encrypted message
            // and the recipient. The nonce must also be shared but does not need to be shared
            // in a secure manner. If the sender encodes the message string to a buffer, the
            // binary encoding method must also be shared with the recipient.
            // The recipient uses the DecryptAndAuthenticate() method as follows to decrypt the 
            // message, authenticate it, and verify that it has not been altered in transit.
            buffDecrypted = CryptographicEngine.DecryptAndAuthenticate(
                keyFromEncryptedString,
                encryptedDataFromStringBuff,
                nonceFromString,
                authenticationTagFromString,
                null);

            // Convert the decrypted buffer to a string (for display). If the sender created the
            // original message buffer from a string, the sender must tell the recipient what 
            // BinaryStringEncoding value was used. Here, BinaryStringEncoding.Utf8 is used to
            // convert the message to a buffer before encryption and to convert the decrypted
            // buffer back to the original plaintext.
            String strDecrypted = CryptographicBuffer.ConvertBinaryToString(encoding, buffDecrypted);
            plaintext = strDecrypted;
            plain.Text = plaintext;
            cipher.Text = plaintext;

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

            AuthenticatedDecryption(
            strSymAlgName,
            nonceString);
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
        public void asymemtricEncryptAESKey(
            String strAsymmetricAlgName,
            String text)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Import the public key from a buffer.
            CryptographicKey publicKey = objAlgProv.ImportPublicKey(pubKey);

            IBuffer keyMaterial = CryptographicBuffer.DecodeFromBase64String(text);

            // Encrypt aes key by using the public key.
            encryptedAESKeyBuff = CryptographicEngine.Encrypt(publicKey, keyMaterial, null);

            //convert to plaintext
            encryptedKeyString = CryptographicBuffer.EncodeToBase64String(encryptedAESKeyBuff);

            aesKey.Text = encryptedKeyString;

        }

        // method to decrypt ciphertext
        public void asymmetricDecryptAESKey(
    String strAsymmetricAlgName,
    IBuffer buffEncryptedKey)
        {
            // Open the algorithm provider for the specified asymmetric algorithm.
            AsymmetricKeyAlgorithmProvider objAsymmAlgProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(strAsymmetricAlgName);

            // Use the private key embedded in the key pair to decrypt the session key.
            decryptedAESKeyBuff = CryptographicEngine.Decrypt(keyPair, buffEncryptedKey, null);

            //convert to string
            decryptedAesKeyString = CryptographicBuffer.EncodeToBase64String(decryptedAESKeyBuff);

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

        private void aes_TextChanged(object sender, TextChangedEventArgs e)
        {
            aesKey.Text = encryptedKeyString;
        }

        private void tag_TextChanged(object sender, TextChangedEventArgs e)
        {
            tag.Text = authenticationTagString;
        }

        private void nonce_TextChanged(object sender, TextChangedEventArgs e)
        {
            nonce.Text = nonceString;
        }
    }
}
