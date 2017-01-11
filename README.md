# unt
Unity networking via sockets & TCP.

Potential problems:

1. Voice_Speaker.dll library missing.
  * Add Voice_Speaker.dll (proprietary?) to SysWOW64 on windows 64 bit.
2. XInput1_3.dll missing.
  * Install DirectX runtime.
3. Can't connect.
  * Change config file IP address and port num to match partner's server.
  * Change color to be black or white, whichever is opposite of partner's color.
