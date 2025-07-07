namespace Mashkoor.Core.Test.Common;

public class MutableTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
