
**Chronos Client  ![icon](https://github.com/nevver/ChronosClient/blob/master/ChronosClient/Assets/Square44x44Logo.targetsize-24_altform-unplated.png?raw=true)**
====

 A E2EE Universal Windows Platform chat application.<br>
 The API can be found https://github.com/nevver/chronosapi. <br>
  Symmetric encryption algorithm for messages and the authentication tag of the messages is the AES_GCM library referenced <a href="https://msdn.microsoft.com/en-us/library/windows.security.cryptography.core.encryptedandauthenticateddata.aspx">here </a> with a key size of 64 bytes. <br>
  Asymmetric encryption algorithm for AES keys is the RsaOaepSha512 library referenced <a href="https://msdn.microsoft.com/en-us/library/windows.security.cryptography.core.asymmetricalgorithmnames.rsaoaepsha512.aspx">here </a> with a key size of 2048 bits. <br>



**Login.xaml - Login.xaml.cs**
----

Login page for the application. You can login, register, or check out the encryption test page we used to debug the encryption. <br /> <br />
 ![Login](https://github.com/nevver/ChronosClient/blob/master/demo/login.png?raw=true)
  

**AsymmetricKeys.xaml - AsymmetricKeys.xaml.cs**
----
 
Create an asymmetric key pair for RSA encryption. Export the public key to a directory for personal key exchange with a potential recipient. <br /> <br />
  ![Asymmetric Keys](https://github.com/nevver/ChronosClient/blob/master/demo/asymmetric-keys.png?raw=true)


**ListUsers.xaml - ListUsers.xaml.cs**
----
  
List all users available for chat. To start a conversation you will need the public key of the recipient to continue. <br /> <br />
   ![List all users](https://github.com/nevver/ChronosClient/blob/master/demo/list-all-users.png?raw=true)

**ImportRecipientsPublicKey.xaml - ImportRecipientsPublicKey.xaml.cs**
----
   
Import the recipients public key to properly encrypt all messages between you and the recipient. <br /> <br />
    ![Import recipients public key](https://github.com/nevver/ChronosClient/blob/master/demo/import-public-key.png?raw=true)


**Messages.xaml - Messages.xaml.cs**
----
    
Send and recieve messages! The client encrypts the messages before communicating with the API and decrypts the thread for the rendering of the conversation. <br /> <br />
    ![Messages](https://github.com/nevver/ChronosClient/blob/master/demo/messages.png?raw=true)
