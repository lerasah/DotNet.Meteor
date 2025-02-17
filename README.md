<p align="center">
<img src="https://github.com/JaneySprings/DotNet.Meteor/raw/main/img/header.jpg" width="1180px" alt=".NET Meteor" />
<a href="https://dev.to/nromanov/boost-net-maui-development-productivity-6-powerful-features-of-net-meteor-for-vs-code-in-windows-mac-linux-d0b">Features</a> | <a href="https://github.com/JaneySprings/DotNet.Meteor/wiki">Documentation</a> | <a href="https://github.com/JaneySprings/DotNet.Meteor/issues">Support</a>

---

## Overview

&emsp;The .NET Meteor allows you to build, debug `.NET` apps and deploy them to devices or emulators.

- **Cross-Platform** </br>
You can use this extension in the `Windows`, `MacOS`, and `Linux` operation systems.

- **XAML IntelliSense** </br>
The extension provides you with a basic `XAML` syntax highlighting and shows snippets for .NET MAUI / third-party controls (it's necessary to build your project first).

- **XAML Hot Reload** </br>
Meteor support XAML Hot Reload for any platform. See the instruction below to enable Hot Reload in your project.

- **MAUI Blazor Support** </br>
The extension allows you to build and debug `MAUI Blazor` apps (including the `.razor` files).

- **Multiple Folders in a Workspace** </br>
You can use muliple folders in your workspace and change the current running project.

- **F# support** </br>
Your can build and debug projects, written in the `F#` language.

---

## Run the Application

1. Open a project's folder.
2. Open the Run and Debug VSCode tab and click the `create a launch.json file`.
3. In the opened panel, select the `.NET Meteor Debugger`.
4. In the status bar, select a project (if your opened folder contains several projects) and a configuration (the debug is the default).
5. In the status bar, click the device name and select a target device/emulator from the opened panel.
6. Press `F5` to debug the application or `ctrl + F5` to launch the application without debugging.
7. Enjoy!

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/img/demo_dbg.gif)

---

## Enable XAML Hot Reload

1. Open the `.csproj` file of your project and add the following package reference:

```xml
<ItemGroup>
	<PackageReference Include="DotNetMeteor.HotReload.Plugin" Version="3.*"/>
</ItemGroup>
```

2. Enable Hot Reload Server in your `MauiProgram.cs`:
```cs
using DotNet.Meteor.HotReload.Plugin;
...
    .UseMauiApp<App>()
#if DEBUG
    .EnableHotReload()
#endif
```
3. Now you can run your project, update XAML and see updates in real-time!

![image](https://github.com/JaneySprings/DotNet.Meteor/raw/main/img/demo_hr.gif)

---

## Compatibility

&emsp;The following table lists supported .NET target platforms and their capabilities:

| Application Type | Build and Run | Hot Reload | Debugging |
|-|:-:|:-:|:-:|
| **WinUI** | ✅ | ✅ | ❌ |
| **Android** | ✅ | ✅ | ✅ |
| **iOS** | ✅ | ✅ | ✅ |
| **MacCatalyst** | ✅ | ✅ | ✅ |

&emsp;*You can debug WinUI apps using the C# VSCode extension with attaching the .NET Core Debugger.*

---

## About the Author

&emsp;I'm Nikita Romanov, a passionate programming enthusiast with a focus on .NET MAUI. I work with an amazing team at `DevExpress` to make the lives of developers around us easier. Our team is dedicated to creating a comprehensive [mobile component suite](https://www.devexpress.com/maui) for .NET MAUI which is currently available `free-of-charge`. In my free time, I work on my hobby project, `DotNet.Meteor`, which is always open to feedback and contributions. Feel free to share your thoughts with me, and **let's make the .NET MAUI community even better together!**
