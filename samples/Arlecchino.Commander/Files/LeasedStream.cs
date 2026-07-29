using System.IO;

namespace Arlecchino.Commander.Files;

/// <summary>
/// A stream that holds an SFTP session for as long as it is open and gives it back when it closes. A
/// transfer occupies its session from the first byte to the last, so the session cannot go back to
/// the pool when the call that opened the stream returns.
/// </summary>
public sealed class LeasedStream : Stream
{
    private readonly Stream _inner;
    private readonly SftpPool.Lease _lease;

    /// <summary>Wraps the stream.</summary>
    /// <param name="inner">The stream to read or write through.</param>
    /// <param name="lease">The session it belongs to.</param>
    public LeasedStream(Stream inner, SftpPool.Lease lease)
    {
        _inner = inner;
        _lease = lease;
    }

    /// <inheritdoc />
    public override bool CanRead => _inner.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _inner.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _inner.CanWrite;

    /// <inheritdoc />
    public override long Length => _inner.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    /// <inheritdoc />
    public override void Flush() => _inner.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => _inner.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _lease.Dispose();
        }

        base.Dispose(disposing);
    }
}
