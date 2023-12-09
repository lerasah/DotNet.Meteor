using System;
using System.IO;
using DotNet.Meteor.Shared;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Debug.Sdk;
using DotNet.Meteor.Debug.Sdk.Profiling;
using System.Threading;

namespace DotNet.Meteor.Debug;

public partial class DebugSession {
    private void ProfileApplication(LaunchConfiguration configuration) {
        DoSafe(() => {
            if (configuration.Device.IsAndroid)
                ProfileAndroid(configuration);

            if (configuration.Device.IsIPhone)
                ProfileApple(configuration);

            if (configuration.Device.IsMacCatalyst)
                ProfileMacCatalyst(configuration);

            if (configuration.Device.IsWindows)
                throw new NotSupportedException("Windows is not supported yet");
        });
    }

    private void ProfileApple(LaunchConfiguration configuration) {
        var applicationName = Path.GetFileNameWithoutExtension(configuration.OutputAssembly);
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationName}.bin");
    
        if (configuration.Device.IsEmulator) {
            var routerProcess = DSRouter.ServerToServer(configuration.ProfilerPort, this);
            Thread.Sleep(1000); // wait for router to start
            var traceProcess = Trace.Collect(routerProcess.Id, resultFilePath, this);

            var simProcess = MonoLaunch.ProfileSim(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, this);

            disposables.Add(() => simProcess.Kill());
            disposables.Add(() => traceProcess.Kill());
            disposables.Add(() => routerProcess.Kill());
        } else {
            // // var forwardingProcess = MonoLaunch.TcpTunnel(configuration.Device.Serial, configuration.ProfilerPort, this);
            // var routerProcess = DSRouter.ServerToClient(configuration.ProfilerPort, this);
            
            
            // MonoLaunch.InstallDev(configuration.Device.Serial, configuration.OutputAssembly, this);
            // var devProcess = MonoLaunch.ProfileDev(configuration.Device.Serial, configuration.OutputAssembly, configuration.ProfilerPort, this);
            // Thread.Sleep(10000); // wait for router to start
            // var traceProcess = Trace.Collect(routerProcess.Id, resultFilePath, this);

            // disposables.Add(() => devProcess.Kill());
            // // disposables.Add(() => forwardingProcess.Kill());
            // disposables.Add(() => traceProcess.Kill());
            // disposables.Add(() => routerProcess.Kill());
        }
    }

    private void ProfileMacCatalyst(LaunchConfiguration configuration) {
        var applicationName = Path.GetFileNameWithoutExtension(configuration.OutputAssembly);
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationName}.bin");
        var homeDirectory = RuntimeSystem.HomeDirectory;

        var tool = AppleUtilities.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(configuration.OutputAssembly));
        processRunner.SetEnvironmentVariable("DOTNET_DiagnosticPorts", $"{homeDirectory}/desktop-port,suspend");
        
        var traceProcess = Trace.Collect(Path.Combine(homeDirectory, "desktop-port"), resultFilePath, this);
        var appLaunchResult = processRunner.WaitForExit();

        if (!appLaunchResult.Success)
            throw new Exception(string.Join(Environment.NewLine, appLaunchResult.StandardError));

        disposables.Add(() => traceProcess.Kill());
    }

    private void ProfileAndroid(LaunchConfiguration configuration) {
        var applicationId = configuration.GetApplicationId();
        var resultFilePath = Path.Combine(configuration.TempDirectoryPath, $"{applicationId}.bin");
        if (configuration.Device.IsEmulator)
            configuration.Device.Serial = AndroidEmulator.Run(configuration.Device.Name).Serial;

        DeviceBridge.Reverse(configuration.Device.Serial, configuration.ProfilerPort, configuration.ProfilerPort+1);
        DeviceBridge.Shell(configuration.Device.Serial, "setprop", "debug.mono.profile", $"127.0.0.1:{configuration.ProfilerPort},suspend");
        
        var routerProcess = DSRouter.ServerToServer(configuration.ProfilerPort+1, this);
        Thread.Sleep(1000); // wait for router to start
        var traceProcess = Trace.Collect(routerProcess.Id, resultFilePath, this);
        Thread.Sleep(1000); // wait for trace to start

        if (configuration.UninstallApp)
            DeviceBridge.Uninstall(configuration.Device.Serial, applicationId, this);
        DeviceBridge.Install(configuration.Device.Serial, configuration.OutputAssembly, this);
        DeviceBridge.Launch(configuration.Device.Serial, applicationId, this);

        disposables.Add(() => DeviceBridge.Shell(configuration.Device.Serial, "am", "force-stop", applicationId));
        disposables.Add(() => Thread.Sleep(2000) /* wait for trace to make json report */);
        disposables.Add(() => DeviceBridge.RemoveForward(configuration.Device.Serial));
        disposables.Add(() => traceProcess.Kill());
        disposables.Add(() => routerProcess.Kill());
    }
}