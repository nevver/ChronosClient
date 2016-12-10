**Chronos Client**
====
 A Universal Windows Platform application.<br>
 The API can be found https://github.com/nevver/chronosapi. <br>
 Currently hosted at https://chronoschat.co. <br />
 The server is currently graded A+ on <a href="https://www.ssllabs.com/ssltest/analyze.html?d=chronoschat.co">Qualys SSL Labs</a>. <br>



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
    
Decrypt thread of messages and render them for display. <br /> <br />
    ![Import recipients public key](https://github.com/nevver/ChronosClient/blob/master/demo/messages.png?raw=true)