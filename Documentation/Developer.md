# Developer
Orcus provides a really good plugin system. If you are a Vb.Net, C# or C++ developer, you can write your own plugin.

## Plugin Types
There are different types of plugins. Here is a full overview:

| Type  | Interface | Description | Example |
| ------------- | ------------- | ------------- | ------------- |
| Administration  | IAdministrationPlugin | Modifies the administration GUI | stress test, connection test, client nofitication |
| Audio  | IAudioPlugin | Provides audio files for playing on the client system | audio pack |
| Build  | IBuildPlugin | Modifies the build | crypter, packer, file pumper |
| Client  | IClientPlugin | Becomes injected into the client, can modify the behavior | BSoD protection |
| Command and View  | ICommandAndViewPlugin & ICommandPlugin | Two files: One is loaded into the administration and adds a new entry to the attacks list. The other one gets transfered to the client. They can communicate with each other | Webcam, Password Recovery |
| View  | IViewPlugin | Adds a new entry to the attacks list. You can access all existing commands | LiveTicker, CommandLine for the commands |

## Write a Plugin
What do you need?
- Visual Studio
- Knowlege of VB.Net, C# or C++
- The `PluginCreator` for creating the plugin

## Administration
### C# #
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `public class` and implement the interface `IAdministrationPlugin`
- Make changes in the `Initialize` void. You can add menu items with the `UiModifier` and subscribe to application events using the `AdministrationConnectionManager`

## Visual Basic .Net
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `public class` and implement the interface `IAdministrationPlugin`
- Make changes in the `Initialize` sub. You can add menu items with the `UiModifier` and subscribe to application events using the `AdministrationConnectionManager`

## Audio
### C# 
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `public class` and implement the interface `IAudioPlugin`
- Create a class which implements the interface `IAudioFile` and return instances in the property `AudioFiles` of the plugin class

### Visual Basic .Net
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `Public Class` and implement the interface `IAudioPlugin`
- Create a class which implements the interface `IAudioFile` and return instances in the property `AudioFiles` of the plugin class


## Build
### C# #
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `public class` and implement the interface `IBuildPlugin`
- Members explained:
    - `bool SettingsAvailable` - return if you want to get the `void OpenSettings(Window ownerWindow)` called
    - `BuildType BuildType` - the modification you want to do. This affects the order the plugins are executed
    - `string DoWork(string path, Action<string> logger)` - this is the main function. Using `logger.Invoke("")`, you can write something into the log. If you make changes to the path, return the new one, else just `return path`
    - `void OpenSettings(Window ownerWindow)` - if you set `SettingsAvailable` to true, this will called if the user presses the Settings-Button. Here, you can open your settings window.

### Visual Basic .Net
- Create a new class library, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `Public Class` and implement the interface `IBuildPlugin`
- Members explained:
    - `Property SettingsAvailable As Boolean` - return if you want to get the `Sub OpenSettings(ownerWindow As Window)` called
    - `Property BuildType As BuildType` - the modification you want to do. This affects the order the plugins are executed
    - `Function DoWork(path As String, logger As Action(Of String)) As String` - this is the main function. Using `logger.Invoke("")`, you can write something into the log. If you make changes to the path, return the new one, else just `return path`
    - `Sub OpenSettings(ownerWindow As Window)` - if you set `SettingsAvailable` to true, this will called if the user presses the Settings-Button. Here, you can open your settings window.


## Client
### C# #
- Create a new class library, select the .Net Framework 3.5
- Add `Orcus.Plugins.dll` to the references
- Create a `public class` and inherit from the abstract class `ClientController`
- Now you can overwrite everything you want to modify

### Visual Basic .Net
- Create a new class library, select the .Net Framework 3.5
- Add `Orcus.Plugins.dll` to the references
- Create a `Public Class` and inherit from the MustInherit class `ClientController`
- Now you can overwrite everything you want to modify
- 
### C++
- Create a C++ class library and add the methods you want to modify e. g. `Start()`, `Shutdown()`, `Install(const char *executablePath)`, `Uninstall()`, ...
- Add code to the methods in the C++ library and compile it
- Create a new c# class library, select the .Net Framework 3.5
- Create a new folder in the C# project called `costura32` for 32 bit or `costura64` for 64 bit
- Add the C++ dll to the folder and change `Build Action` to `Embedded Resource`
![CPlusPlusLibrary](http://fs5.directupload.net/images/151030/j9g8ekh2.png)
- Add `Orcus.Plugins.dll` to the references
- Create a `public class` and inherit from the abstract class `ClientController`
- Add code similar to this:
```csharp
    public class Plugin : ClientController
    {
        public Plugin()
        {
            var t = Assembly.GetExecutingAssembly().GetType("Costura.AssemblyLoader");
            if (t != null)
            {
                var method = t.GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
                method.Invoke(null, null);
            }
        }
        
        public virtual void Start()
        {
            NativeDllWrapper.Start();
        }

        public virtual void Shutdown()
        {
            NativeDllWrapper.Shutdown();
        }

        public virtual void Install(string executablePath)
        {
            NativeDllWrapper.Install(executablePath);
        }

        public virtual void Uninstall()
        {
            NativeDllWrapper.Uninstall();
        }
    }

    internal static class NativeDllWrapper
    {
        [DllImport("MyCppDll.dll")]
        public static extern void Start();

        [DllImport("MyCppDll.dll")]
        public static extern void Shutdown();

        [DllImport("MyCppDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Install(string executablePath);

        [DllImport("MyCppDll.dll")]
        public static extern void Uninstall();
    }
```
- Last, go to `Project` -> `Manage NuGet Packages...`, search `Costura.Fody` and install it
- Compile it

## Command and View
___
To send data (classes), you can use `Serializer` ([NetSerializer](https://github.com/tomba/netserializer)) found in `Orcus.Shared`. The data will be automatically compressed and decompressed
___
### C# #
**Administration Plugin**
- Create a new `WPF User Control Library`, select the .Net Framework 4.5
- Add `Orcus.Administration.Plugins.dll` to the references
- Create a `public class` and implement the interface `ICommandAndViewPlugin`
- The 3 properties are the view (View), the model (Command) and the ViewModel (CommandView)
- Create 3 new elements: 2 normal classes and a WPF UserControl
- Create the model
  - Create a new class and inherit from Command
  - The function `GetId()` should return the id of the command. It must match to the id of the client library. Please generate it with the give tool (PluginIdGenerator) and never choose an id below 1000 (they are reserved for the build-in commands)
  - `ResponseReceived` will be called if you send bytes on the other side. It is recommended that the first byte of the parameter is the token byte. To make it comfortable, you can create an enum. Just check the sample plugins created by me. An example, if we receive webcams:
  ```csharp
        public override void ResponseReceived(byte[] parameter)
        {
            switch ((WebcamCommunication) parameter[0])
            {
                case WebcamCommunication.ResponseWebcams:
                    var serializer = new Serializer(typeof (List<WebcamInfo>));
                    var webcams = serializer.Deserialize<List<WebcamInfo>>(parameter, 1);
                    Webcams = webcams;
                    WebcamsReceived?.Invoke(this, webcams);

                    LogService.Receive("We received " + Webcams.Count + " webcams");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
  ```
  - You should create methods which will send commands to the client. For example, if we want to receive the available webcams, you can write something like this:
  ```csharp
        public void GetWebcams()
        {
            LogService.Send("Get webcams");
            ConnectionInfo.SendCommand(this, new[] {(byte) WebcamCommunication.GetWebcams});
        }
  ```
- Create the view model
  - Create a new class and implement `ICommandView`
  - If you never worked with MVVM (Model-View-ViewModel) before, it might be a little bit difficult for you. The view model is like the code behind, but it knows absolutly nothing about the view (that, what you see). Check the samples to see how it should be done.
  - In the `Initialize` method, you should store the command (which we created in step 1) in a variable. You can get it with `clientController.Commander.GetCommand<YourClassNameChosenAbove>()`
  - LoadView is called when the user clicked on the menu item for the first time
- Create the view
  - Create a new `UserControl`
  - You should develop the view and the view model at the same time because they have to be synchronized
  - The DataContext will be automatically set to the view model we've created above
  - To enable IntelliSense, you can set the the design-DataContext: `d:DataContext="{d:DesignInstance local:YourViewModel}"`
- Last, set the properties from the class that implements the interface `ICommandAndViewPlugin`:
```csharp
    public class Plugin : ICommandAndViewPlugin
    {
        public Command Command { get; } = new NativePasswordRecoveryCommand();
        public ICommandView CommandView { get; } = new CommandView();
        public Type View { get; } = typeof (NativePasswordRecoveryView);
    }
```

**Client Plugin**
- Create a new class library, select the .Net Framework 3.5 (you should name it like you named the administration plugin with `.Payload`)
- Add `Orcus.Plugins.dll` to the references
- Create a new `public class` and inherit `Command`
- In `GetId()`, return the same id like you return above
- In `ProcessCommand`, you can execute the command and return results. For example
```csharp
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((WebcamCommunication) parameter[0])
            {
                case WebcamCommunication.GetWebcams:
                    var webcams =
                        new FilterInfoCollection(FilterCategory.VideoInputDevice).OfType<FilterInfo>()
                            .Select(
                                x =>
                                    new WebcamInfo
                                    {
                                        MonikerString = x.MonikerString,
                                        Name = x.Name,
                                        AvailableResolutions =
                                            new VideoCaptureDevice(x.MonikerString).VideoCapabilities.Select(
                                                y =>
                                                    new WebcamResolution
                                                    {
                                                        Width = y.FrameSize.Width,
                                                        Heigth = y.FrameSize.Height
                                                    }).ToList()
                                    })
                            .ToList();
                    var serializer = new Serializer(typeof (List<WebcamInfo>));
                    var data = new List<byte> {(byte) WebcamCommunication.ResponseWebcams};
                    data.AddRange(serializer.Serialize(webcams));
                    ResponseBytes(data.ToArray(), connectionInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
```

### Visual Basic .Net #
- You can do exactly the same in vb.net like in C#. To convert the snippets, you can use http://codeconverter.sharpdevelop.net/SnippetConverter.aspx


## Some tips
- You can use the following libraries without providing them by yourself
  - Client
    - CSCore (1.1.0-beta)
    - AForge.Video (2.2.5)
    - AForge.Video.DirectShow (2.2.5)
    - Orcus.Shared
    - Orcus.Shared.Utilities
    - Orcus.Plugins
  - Administration
    - Sorzus.Wpf.Toolkit
    - AlphaChiTech.Virtualization (1.2)
    - Be.Windows.Forms.HexBox (1.6.1)
    - CSCore (1.0.0.0)
    - Exceptionless.Extras (3.2.1424)
    - Exceptionless.Portable (3.2.1424)
    - Exceptionless.Wpf (3.2.1424)
    - ICSharpCode.AvalonEdit (5.0.1)
    - MahApps.Metro (1.2.3)
    - Mono.Cecil (0.9.6)
    - Newtonsoft.Json (8.0)
    - Ookii.Dialogs.Wpf (1.0.0)
    - Orcus.Administration.Commands
    - Orcus.Administration.Plugins
    - Orcus.Administration.StaticCommands
    - Orcus.Plugins
    - Orcus.Shared
    - Orcus.Shared.Utilities
    - Sparrow.Chart.Wpf.40 (13.1.0.118)
    - Vestris.ResourceLib (1.4.33.0)
    - Xceed.Wpf.Toolkit (2.6.0)
- If you want to use a library not listed above, you can include it. Just take a look at [Costura.Fody](https://github.com/Fody/Costura), it will automatically embed your managed libraries and you can also include unmanged
