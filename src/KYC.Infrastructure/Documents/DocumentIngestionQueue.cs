using System.Threading.Channels;
using KYC.Application.Interfaces;

namespace KYC.Infrastructure.Documents;

public sealed class DocumentIngestionQueue : IDocumentIngestionQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ChannelReader<Guid> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(Guid documentId, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(documentId, ct);
}
