using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Sample.Frames;

internal sealed class NullLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => CancellationToken.None;

    public CancellationToken ApplicationStopping => CancellationToken.None;

    public CancellationToken ApplicationStopped => CancellationToken.None;

    public void StopApplication() { }
}
