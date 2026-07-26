using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino.Packages.Scanning;

public sealed record DotnetResult(int ExitCode, string Output, string Error)
{
    public bool Failed => ExitCode != 0;
}

public static class Dotnet
{
    public static async Task<DotnetResult> RunAsync(
        IReadOnlyList<string> arguments,
        string workingDirectory,
        CancellationToken token)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
        };

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        using var process = Process.Start(info) ?? throw new InvalidOperationException("dotnet could not be started");

        var output = process.StandardOutput.ReadToEndAsync(token);
        var error = process.StandardError.ReadToEndAsync(token);

        await process.WaitForExitAsync(token).ConfigureAwait(false);

        return new(process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
    }

    public static string Describe(IReadOnlyList<string> arguments) => "dotnet " + string.Join(' ', arguments);
}
