using System;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample.Views;
using Arlecchino.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Sample.Frames;

internal static class Frame
{
    public static void Draw(string view, string size)
    {
        var services = new ServiceCollection();

        services
            .AddArlecchino(options =>
            {
                options.MinimumWidth = 60;
                options.MinimumHeight = 16;
            })
            .AddGeneratedViews()
            .AddGeneratedStores()
            .AddCommand<AboutCommand>()
            .AddCommand<QuitCommand>()
            .WithoutHostedService();

        services.AddSingleton<IHostApplicationLifetime, NullLifetime>();

        using var provider = services.BuildServiceProvider();

        var (width, height) = Measure(size);

        provider.GetRequiredService<Surface>().SetFixedSize(width, height);

        var state = provider.GetRequiredService<ArlecchinoState>();
        var navigator = provider.GetRequiredService<Navigator>();

        FrameCatalog.For(view).Arrange(state, navigator);

        provider.GetRequiredService<Screen>().DrawOnce();

        Console.WriteLine();
    }

    private static (int Width, int Height) Measure(string size)
    {
        var parts = size.Split('x');

        if (parts.Length != 2 || !int.TryParse(parts[0], out var width) || !int.TryParse(parts[1], out var height))
        {
            return (120, 34);
        }

        return (width, height);
    }
}
