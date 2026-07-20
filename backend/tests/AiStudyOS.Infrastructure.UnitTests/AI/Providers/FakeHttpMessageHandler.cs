namespace AiStudyOS.Infrastructure.UnitTests.AI.Providers;

/// <summary>Routes HttpClient calls to a test-supplied responder instead of a real socket.</summary>
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        responder(request, cancellationToken);
}

/// <summary>
/// Yields bytes from a fixed buffer with a delay before each read, honoring cancellation — used to
/// simulate a slow/streaming response body so cancellation-mid-stream and timeout tests can actually
/// observe a token firing while a read is in flight, rather than the whole body arriving instantly.
/// </summary>
internal sealed class SlowStream(byte[] data, TimeSpan delayPerChunk, int chunkSize = 24) : Stream
{
    private int _position;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => data.Length;
    public override long Position { get => _position; set => throw new NotSupportedException(); }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position >= data.Length) return 0;

        await Task.Delay(delayPerChunk, cancellationToken);

        var toCopy = Math.Min(Math.Min(buffer.Length, chunkSize), data.Length - _position);
        data.AsSpan(_position, toCopy).CopyTo(buffer.Span);
        _position += toCopy;
        return toCopy;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>A stream that reads a fixed number of bytes normally, then throws IOException — simulates a provider connection dropping mid-response.</summary>
internal sealed class DisconnectingStream(byte[] data, int bytesBeforeDisconnect) : Stream
{
    private int _position;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => data.Length;
    public override long Position { get => _position; set => throw new NotSupportedException(); }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_position >= bytesBeforeDisconnect)
            throw new IOException("The response ended prematurely, as if the connection was reset.");

        var toCopy = Math.Min(buffer.Length, bytesBeforeDisconnect - _position);
        toCopy = Math.Min(toCopy, data.Length - _position);
        data.AsSpan(_position, toCopy).CopyTo(buffer.Span);
        _position += toCopy;
        return ValueTask.FromResult(toCopy);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
