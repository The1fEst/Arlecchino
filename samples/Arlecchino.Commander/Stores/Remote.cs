using Arlecchino.Atoms;
using Arlecchino.Commander.Files;

namespace Arlecchino.Commander.Stores;

public sealed class Remote : IArlecchinoStore
{
    public Atom<string> Scheme { get; } = new LocalAtom<string>("sftp");

    public Atom<string> Host { get; } = new LocalAtom<string>("");

    public Atom<decimal> Port { get; } = new LocalAtom<decimal>(22);

    public Atom<string> User { get; } = new LocalAtom<string>("");

    public Atom<string> Password { get; } = new LocalAtom<string>("");

    public Atom<string> Folder { get; } = new LocalAtom<string>("/");

    public Atom<string> KeyFile { get; } = new LocalAtom<string>("");

    public Atom<string> Saved { get; } = new LocalAtom<string>("");

    public Atom<bool> Connecting { get; } = new LocalAtom<bool>(false);

    public Atom<string> Failure { get; } = new LocalAtom<string>("");

    public Connection? Ssh { get; set; }

    public void Fill(SshHost host)
    {
        Scheme.Value = "sftp";
        Host.Value = host.HostName;
        Port.Value = host.Port;
        User.Value = host.User;
        KeyFile.Value = host.KeyFile;
    }

    public Connection Wanted() => new(
        Scheme.Value == "ftp" ? Protocol.Ftp : Protocol.Sftp,
        Host.Value.Trim(),
        (int)Port.Value,
        User.Value.Trim(),
        Password.Value,
        Folder.Value.Trim(),
        KeyFile.Value.Trim());
}
