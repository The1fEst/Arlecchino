using System.Linq;
using Arlecchino.Commands;
using Arlecchino.Tests.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Arlecchino.Tests.Support;
using Arlecchino.Tests.Input;

namespace Arlecchino.Tests.Navigation;

public sealed class CommandRegistrationTests
{
    [Fact]
    public void CommandsAreRegisteredWithoutBeingListedByHand()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedCommands());

        var commands = app.Services.GetRequiredService<CommandRegistry>().Commands;

        Assert.Contains(commands, static command => command is ProbeCommand);
        Assert.Contains(commands, static command => command is SaveCommand);
    }

    [Fact]
    public void AGeneratedCommandGetsItsConstructorParametersFromTheContainer()
    {
        using var app = new TestApplication(configure: static builder => builder.AddGeneratedCommands());

        var probe = app.Services.GetRequiredService<CommandRegistry>().Commands.OfType<ProbeCommand>().Single();
        probe.Execute();

        Assert.Equal("probe command", app.State.Output);
    }

    [Fact]
    public void CommandsAreNotRegisteredUntilAddGeneratedCommandsIsCalled()
    {
        using var app = new TestApplication();

        Assert.Empty(app.Services.GetRequiredService<CommandRegistry>().Commands);
    }
}
