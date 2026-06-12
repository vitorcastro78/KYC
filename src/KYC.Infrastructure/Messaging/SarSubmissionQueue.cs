using System.Threading.Channels;
using KYC.Application.Interfaces;

namespace KYC.Infrastructure.Messaging;

public sealed class SarSubmissionQueue : ISarSubmissionQueue
{
    private readonly Channel<SarSubmissionWork> _channel = Channel.CreateUnbounded<SarSubmissionWork>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ChannelReader<SarSubmissionWork> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(SarSubmissionWork work, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(work, ct);
}
