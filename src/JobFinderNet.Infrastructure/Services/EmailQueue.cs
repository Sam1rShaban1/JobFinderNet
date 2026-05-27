using System.Threading.Channels;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Infrastructure.Services;

public class EmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public EmailQueue(int capacity = 100)
    {
        _channel = Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });
    }

    public async Task EnqueueAsync(EmailMessage message, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(message, ct);
    }

    public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
