namespace Arlecchino.Commander.Files;

public enum Protocol
{
    Sftp,
    Ftp,
}

public sealed record Connection(
    Protocol Protocol,
    string Host,
    int Port,
    string User,
    string Password,
    string Path,
    string KeyFile = "")
{
    public static Connection Empty { get; } = new(Protocol.Sftp, "", 22, "", "", "/");

    public string Scheme => Protocol == Protocol.Sftp ? "sftp" : "ftp";

    public string Label => $"{Scheme} {User}@{Host}";

    public static int PortFor(Protocol protocol) => protocol == Protocol.Sftp ? 22 : 21;
}
