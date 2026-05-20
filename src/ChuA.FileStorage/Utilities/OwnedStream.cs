// Copyright (c) 2026 Alvin Wilsen Chan Chua
// GitHub: chuaalvw-y
// Licensed under the Alvin Wilsen Chan Chua Proprietary Use-Only License.
// See LICENSE.txt in the project root for full license information.

namespace ChuA.FileStorage.Utilities;

/// <summary>
/// Wraps a stream with additional owned resources that should be disposed with it.
/// </summary>
internal sealed class OwnedStream : Stream
{
    private readonly Stream inner;
    private readonly IDisposable[] owners;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnedStream"/> class.
    /// </summary>
    /// <param name="inner">The stream to wrap.</param>
    /// <param name="owners">Additional resources owned by the stream.</param>
    public OwnedStream(Stream inner, params IDisposable[] owners)
    {
        this.inner = inner;
        this.owners = owners;
    }

    /// <inheritdoc />
    public override bool CanRead => inner.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => inner.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => inner.CanWrite;

    /// <inheritdoc />
    public override long Length => inner.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    /// <inheritdoc />
    public override void Flush() => inner.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => inner.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await inner.DisposeAsync().ConfigureAwait(false);
        foreach (var owner in owners)
        {
            owner.Dispose();
        }

        await base.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            inner.Dispose();
            foreach (var owner in owners)
            {
                owner.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
