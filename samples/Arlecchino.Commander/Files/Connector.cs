using System;
using System.IO;
using System.Threading.Tasks;

namespace Arlecchino.Commander.Files;

public static class Connector
{
    /// <summary>
    /// Opens a connection off the drawing thread and answers on it. The failure callback is told
    /// whether the server turned the credentials down rather than being unreachable, because only then
    /// is asking for a password worth anything.
    /// </summary>
    /// <param name="wanted">Where to connect and with what.</param>
    /// <param name="landed">Called with the source and the folder to open.</param>
    /// <param name="failed">Called with what went wrong and whether the credentials were refused.</param>
    public static void Start(Connection wanted, Action<IFileSource, string> landed, Action<string, bool> failed)
    {
        Task.Run(() =>
        {
            try
            {
                var source = wanted.Protocol == Protocol.Sftp
                    ? SftpSource.Connect(wanted)
                    : (IFileSource)FtpSource.Connect(wanted);

                var folder = wanted.Path.Length > 0 ? wanted.Path : source.Home;

                FrameThread.Post(() => landed(source, folder));
            }
            catch (UnauthorizedAccessException error)
            {
                FrameThread.Post(() => failed(error.Message, true));
            }
            catch (Exception error) when (error is IOException or InvalidOperationException)
            {
                FrameThread.Post(() => failed(error.Message, false));
            }
        });
    }
}
