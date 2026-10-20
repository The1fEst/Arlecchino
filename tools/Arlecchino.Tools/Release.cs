using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino.Tools;

/// <summary>
/// Cuts a release, offering the tag that publishes the packages only once the build on GitHub has passed
/// for the commit it would go on.
/// </summary>
internal static class Release
{
    private const string Branch = "master";
    private const string StartOverHint = "git checkout -- . puts back what ship changed.";
    private static readonly TimeSpan RunAppearTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan RunPollInterval = TimeSpan.FromSeconds(5);

    /// <summary>Releases the repository, stopping at the first step that does not succeed.</summary>
    /// <param name="args">The version to release, or nothing to take the next build of this month.</param>
    /// <returns>Zero when the release went out, or when the tag was declined after a green build.</returns>
    internal static async Task<int> Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Usage(0);
        }

        if (args.Length > 1)
        {
            return Usage(1);
        }

        var root = Program.Root();

        if (!IsReadyToRelease(root))
        {
            return 1;
        }

        if (await Ship.Run(args) != 0)
        {
            return 1;
        }

        var version = Ship.GetVersion();

        if (ReadOutput(root, "git", "diff", "--quiet", "--", "Directory.Build.props").ExitCode == 0)
        {
            Console.WriteLine($"The version stayed {version}, so there is nothing new to tag. {StartOverHint}");

            return 1;
        }

        if (RunCommand(root, "dotnet", "build", "Arlecchino.slnx", "--configuration", "Release") != 0 ||
            RunCommand(root, "dotnet", "test", "Arlecchino.slnx", "--configuration", "Release", "--no-build") != 0)
        {
            Console.WriteLine($"The build or the tests failed here; nothing was committed. {StartOverHint}");

            return 1;
        }

        RunCommand(root, "git", "--no-pager", "diff", "--stat");

        if (!Confirm($"Commit and push {version} to {Branch}?"))
        {
            Console.WriteLine($"Left uncommitted. {StartOverHint}");

            return 1;
        }

        var message = $"Set the version to {version} and write down the public API it carries";

        if (RunCommand(root, "git", "commit", "--all", "--message", message) != 0 ||
            RunCommand(root, "git", "push", "origin", Branch) != 0)
        {
            return 1;
        }

        var commit = ReadOutput(root, "git", "rev-parse", "HEAD").Output;

        if (!Watch(root, "build.yml", commit))
        {
            Console.WriteLine($"build.yml did not pass on {commit[..7]}; no tag was made.");

            return 1;
        }

        var tag = $"v{version}";

        if (!Confirm($"The build passed on {commit[..7]}. Tag {tag} and push it, which publishes the packages?"))
        {
            Console.WriteLine($"No tag. To publish later: git tag {tag} {commit[..7]} && git push origin {tag}");

            return 0;
        }

        if (RunCommand(root, "git", "tag", tag, commit) != 0 || RunCommand(root, "git", "push", "origin", tag) != 0)
        {
            return 1;
        }

        return Watch(root, "release.yml", commit) ? 0 : 1;
    }

    private static int Usage(int code)
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- release [version]");
        Console.WriteLine();
        Console.WriteLine("Runs ship, builds and tests, commits and pushes the version, waits for build.yml on");
        Console.WriteLine("GitHub, and when it passes offers to push the tag that publishes the packages.");
        Console.WriteLine();
        Console.WriteLine($"Needs a clean {Branch} that is not behind origin, gh signed in, and a paragraph");
        Console.WriteLine("under ## Unreleased in CHANGELOG.md saying what the release is about.");

        return code;
    }

    /// <summary>Whether everything a release needs is in place, saying what is missing when it is not.</summary>
    /// <param name="root">The repository.</param>
    /// <returns><c>true</c> when the release can start.</returns>
    private static bool IsReadyToRelease(string root)
    {
        if (ReadOutput(root, "gh", "auth", "status").ExitCode != 0)
        {
            Console.WriteLine("gh is not signed in: run gh auth login");

            return false;
        }

        if (ReadOutput(root, "git", "status", "--porcelain").Output.Length > 0)
        {
            Console.WriteLine("The working tree has changes; commit or stash them first.");

            return false;
        }

        if (ReadOutput(root, "git", "rev-parse", "--abbrev-ref", "HEAD").Output != Branch)
        {
            Console.WriteLine($"Not on {Branch}.");

            return false;
        }

        if (RunCommand(root, "git", "fetch", "origin", Branch) != 0)
        {
            return false;
        }

        if (ReadOutput(root, "git", "rev-list", "--count", $"HEAD..origin/{Branch}").Output != "0")
        {
            Console.WriteLine($"{Branch} is behind origin/{Branch}; pull first.");

            return false;
        }

        var changelog = File.ReadAllText(Path.Combine(root, "CHANGELOG.md"));
        var intro = Regex.Match(changelog, @"^## Unreleased\r?\n\s*(?<first>[^\r\n]*)", RegexOptions.Multiline);

        if (!intro.Success)
        {
            Console.WriteLine("CHANGELOG.md has no ## Unreleased section.");

            return false;
        }

        if (!intro.Groups["first"].Value.StartsWith('#'))
        {
            return true;
        }

        Console.WriteLine("## Unreleased has no paragraph saying what the release is about; write one first.");

        return false;
    }

    /// <summary>Waits for a workflow's run on a commit to finish, showing its progress as it goes.</summary>
    /// <param name="root">The repository.</param>
    /// <param name="workflow">The workflow file, as <c>build.yml</c>.</param>
    /// <param name="commit">The commit the run is for.</param>
    /// <returns><c>true</c> when the run succeeded.</returns>
    private static bool Watch(string root, string workflow, string commit)
    {
        var deadline = DateTime.UtcNow + RunAppearTimeout;
        var runId = FindRun(root, workflow, commit);

        while (runId.Length == 0)
        {
            if (DateTime.UtcNow > deadline)
            {
                Console.WriteLine($"No {workflow} run appeared for {commit[..7]}.");

                return false;
            }

            Thread.Sleep(RunPollInterval);
            runId = FindRun(root, workflow, commit);
        }

        Console.WriteLine($"Watching {workflow} run {runId}");

        if (RunCommand(root, "gh", "run", "watch", runId, "--exit-status") == 0)
        {
            return true;
        }

        RunCommand(root, "gh", "run", "view", runId, "--log-failed");

        return false;
    }

    private static string FindRun(string root, string workflow, string commit) =>
        ReadOutput(root,
                "gh",
                "run",
                "list",
                "--workflow",
                workflow,
                "--commit",
                commit,
                "--limit",
                "1",
                "--json",
                "databaseId",
                "--jq",
                ".[0].databaseId // empty")
            .Output;

    private static bool Confirm(string question)
    {
        Console.Write($"{question} [y/N] ");

        return Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>Runs a command with the console handed to it, so its output goes straight to the screen.</summary>
    /// <param name="root">Where it runs.</param>
    /// <param name="program">The program.</param>
    /// <param name="arguments">Its arguments.</param>
    /// <returns>Its exit code.</returns>
    private static int RunCommand(string root, string program, params string[] arguments)
    {
        using var process = Process.Start(CreateStartInfo(root, program, arguments, redirects: false)) ??
                            throw new InvalidOperationException($"{program} would not start");

        process.WaitForExit();

        return process.ExitCode;
    }

    /// <summary>Runs a command quietly and keeps what it printed.</summary>
    /// <param name="root">Where it runs.</param>
    /// <param name="program">The program.</param>
    /// <param name="arguments">Its arguments.</param>
    /// <returns>Its exit code and its output, trimmed.</returns>
    private static CommandResult ReadOutput(string root, string program, params string[] arguments)
    {
        using var process = Process.Start(CreateStartInfo(root, program, arguments, redirects: true)) ??
                            throw new InvalidOperationException($"{program} would not start");

        var output = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new(process.ExitCode, output.Trim());
    }

    private static ProcessStartInfo CreateStartInfo(string root, string program, string[] arguments, bool redirects)
    {
        var startInfo = new ProcessStartInfo(program)
        {
            WorkingDirectory = root,
            RedirectStandardOutput = redirects,
            RedirectStandardError = redirects,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private readonly record struct CommandResult(int ExitCode, string Output);
}
