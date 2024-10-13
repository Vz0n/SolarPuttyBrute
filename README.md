# SolarPuTTYBrute (Fork of SolarPuttyDecrypt)
A post-exploitation/forensics tool to decrypt SolarPuTTY's sessions files (Now with an option to use a wordlist for bruteforce purposes)

I changed stuff on the code to make the program work better and also added an option to use a wordlist to bruteforce the password. Here is an run with that:

![Bruteforce](/img/test.png)

*Original Author:* Paolo Stagno ([@Void_Sec](https://twitter.com/Void_Sec) - [voidsec.com](https://voidsec.com))

## Intro:

In September 2019 I found some bad design choices (vulnerability?) in SolarWinds [SolarPuTTY](https://www.solarwinds.com/free-tools/solar-putty) software. It allows an attacker to recover SolarPuTTY's stored sessions from a compromised system.

This vulnerability was leveraged to targets all SolarPuTTY versions <= 4.0.0.47

I've made this detailed [blog post](https://voidsec.com/solarputtydecrypt/) explaining the "vulnerability".

## Usage:
The tool can be pointed to an arbitrary exported sessions file in the following way (leave second argument empty for empty password):
```
SolarPuttyDecrypt.exe C:\Users\test\session.dat Pwd123!
```

Or you can specify a wordlist and the program will try to bruteforce the encryption key

```
SolarPuttyDecrypt.exe .\session.dat .\magical_wordlist.txt
```

Sessions will be outputted on screen and saved into working directory if you only specify a password.

### Help Needed

Searching for someone interested into helping me adding the decryption routine to the [Metasploit post-exploitation module](solar_putty.rb).