# unt
Unity networking project via sockets & TCP.

### Troubleshooting:

1. Voice_Speaker.dll library missing.
   * Add Voice_Speaker.dll (proprietary?) to SysWOW64 on windows 64 bit.
2. XInput1_3.dll missing.
   * Install DirectX runtime.
3. Can't connect with client.
   * Change config file IP address and port num to match partner's server.
   * Change color to be black or white, whichever is opposite of partner's color.
4. Can't connect with network speech connection to speech-recognition python server.
   * Change the "Address" in NetworkConnection.cs to your local ip (on Windows, type "ipconfig" in command prompt)
5. Python speech-recognition server doesn't run
   * Be sure to install all necessary modules via "pip install".
