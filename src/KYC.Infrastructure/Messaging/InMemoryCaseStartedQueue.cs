using System.Threading.Channels;

namespace KYC.Infrastructure.Messaging;

public sealed class InMemoryCaseStartedQueue
{
    private readonly Channel<CaseStartedWork> _channel = Channel.CreateUnbounded<CaseStartedWork>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ChannelWriter<CaseStartedWork> Writer => _channel.Writer;

    public ChannelReader<CaseStartedWork> Reader => _channel.Reader;
}

public readonly record struct CaseStartedWork(Guid CaseId, string Nif);
