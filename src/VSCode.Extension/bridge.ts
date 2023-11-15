import { spawnSync, exec } from 'child_process';
import { IProject, IDevice } from './models';
import * as res from './resources';
import * as vscode from 'vscode';
import * as path from 'path';


export class CommandController {
    private static toolPath: string;

    public static activate(context: vscode.ExtensionContext): boolean {
        const extensionPath = vscode.extensions.getExtension(`${res.extensionPublisher}.${res.extensionId}`)?.extensionPath ?? '';
        const executableExtension = process.platform === 'win32' ? '.exe' : '';
        CommandController.toolPath = path.join(extensionPath, "extension", "bin", "DotNet.Meteor.Workspace" + executableExtension);
        return true;
    }

    public static androidSdk(): string | undefined {
        return ProcessRunner.runSync(CommandController.toolPath, "--android-sdk-path");
    }
    public static async getDevices(): Promise<IDevice[]> {
        return await ProcessRunner.runAsync<IDevice[]>(new ProcessArgumentBuilder(CommandController.toolPath)
            .append("--all-devices"));
    }
    public static async getProjects(folders: string[]): Promise<IProject[]> {
        return await ProcessRunner.runAsync<IProject[]>(new ProcessArgumentBuilder(CommandController.toolPath)
            .append("--analyze-workspace")
            .appendQuoted(...folders));
    }
    public static async xamlSchema(path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder(CommandController.toolPath)
            .append("--xaml")
            .appendQuoted(path));
    }
    public static async xamlReload(port: number, path: string): Promise<boolean>  {
        return await ProcessRunner.runAsync<boolean>(new ProcessArgumentBuilder(CommandController.toolPath)
            .append("--xaml-reload")
            .append(port.toString())
            .appendQuoted(path));
    }
}

class ProcessRunner {
    public static runSync(command: string, ...args: string[]): string | undefined {
        const result = spawnSync(command, args);
        if (result.error) {
            console.error(result.error);
            return undefined;
        }
        return result.stdout.toString().trimEnd();
    }
    public static async runAsync<TModel>(builder: ProcessArgumentBuilder): Promise<TModel> {
        return new Promise<TModel>((resolve, reject) => {
            exec(builder.build(), (error, stdout, stderr) => {
                if (error) {
                    console.error(stderr);
                    reject(stderr);
                }

                resolve(JSON.parse(stdout.toString()));
            })
        });
    }
}

export class ProcessArgumentBuilder {
    private arguments: string[] = [];

    public constructor(command: string) {
        this.arguments.push(command);
    }

    public append(arg: string): ProcessArgumentBuilder {
        this.arguments.push(arg);
        return this;
    }
    public appendQuoted(...args: string[]): ProcessArgumentBuilder {
        args.forEach(a => this.arguments.push(`"${a}"`));
        return this;
    }
    public override(arg: string): ProcessArgumentBuilder {
        const argName = arg.split("=")[0];
        const index = this.arguments.findIndex(a => a.startsWith(argName));
        if (index > -1) 
            this.arguments.splice(index, 1);
        this.arguments.push(arg);
        return this;
    }
    public build(): string {
        return this.arguments.join(" ");
    }
}
  