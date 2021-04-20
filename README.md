# Orcus 1.9.1 Source Code 
# Credits snyke

# This repository doesn't contain my code. I have uploaded it to GitHub for those want to analyse the code.
I'm not associated with the development and production of this program are not responsible for any actions and or damages caused by this software. You bear the full responsibility of your actions and acknowledge that this software was created for educational purposes only. This software's intended purpose is NOT to be used maliciously, or on any system that you do not have own or have explicit permission to operate and use this program on. By using this software, you automatically agree to the above.

https://cracked.to/Thread-Leak-C-Orcus-1-9-1-RAT-Full-Source-Code-Leaked

## Build
- Set the build option to `Release`
- Press Ctrl + Shift + B to build the complete solution (do this until it does nothing if you build)
- Set the build option back to `Debug`
- Press Ctrl+ Shift + B again

C# RAT with lots of features.

![Schnorchels](http://fs5.directupload.net/images/151117/xvvb5oey.png)

If you were searching for the perfect RAT, I have a good message for you: Congratulation, you found it. Schnorchel is the perfect RAT for everyone. It provides all standard features like Registry Editor, Webcam, Remote Desktop,... (a full list of all features is linked below).

But what makes it special? We implemented lots of features which make it unique and easy to use. With one of the best GUI framework, WPF, the GUI is really clean and just beautiful. Remote Desktop and webcam are rendered with Direct X for the maximum frame rate. For developers, we implemented a plugin system (plugins can be written in vb.net, C# and C++). With an exception view, you can see what goes wrong with the Client.
With a patcher, you can add and remove plugins, change settings or just update the client. If you build the Client with a service, it will install a Windows service on the victims System. The service can do lots of operations which need administrator privileges, for example change the registry or trigger a bluescreen.

One of our goals is to make everything as fast as possible. To reach that, we use a lot of tricks. To reduce the size of the packages to a minimum, we use the zero-overhead-princip. Every Byte counts. Most of the time, we dont use serialisation, we just send plain bytes and interpret them on the other side. But If we really need serialisation, we use a really good serializer, [NetSerializer](https://github.com/tomba/netserializer). It is one of the fastest and only outputs what is really necessary. If we need to send large data, we use a fast compression algorithm, [QuickLZ](http://www.quicklz.com/). For sure we use a small video codec for the webcam and remote desktop. To gurantee a really good performance for the server, we use a smart way of receiving packages - the server was successfully tested with 10000 clients.

The server is an extra application. For sure, you can run it on the same system like the attacker client, but we also wrote one which will execute on linux systems, for example on a raspberry pi. To successfully execute it, you need to install the mono runtime.

All attacker clients are perfectly synchronized. If one changes for example the group of a client, all other will see the change in realtime.
The server will store all data in a SQL database. Key logs, passwords and computer information of a client will also be saved there.

The fun aspect is not missing at all. Beside the standard features like hiding the clock or taskbar, blocking keyboard and mouse or trigger a real bluescreen, we were also creative and implemented things like changing the playback and Mikrophone volume, a very cool combo which will the User make crazy and playing audios like a Skype call or Steam message to fool the user.

Error handling is a really important aspect of every RAT. Because of really clean and OOP coding, the possible exceptions are reduced to a minimum. Almost every action will be executed in a seperate thread - if something goes wrong, it wont take down the application. In this case, a message with the status fatal will be send to the attacker and an exception report will be send to the server.
In the worst case scenario, if an unhandled exception occurres, nothing will be lost. With a global event, every unhandled exception will be caught, create an error report and make a safe restart of the client application.

Just take a look at the [complete feature list](Features.md).





