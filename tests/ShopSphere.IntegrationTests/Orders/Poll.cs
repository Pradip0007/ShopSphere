namespace ShopSphere.IntegrationTests.Orders;

internal static class Poll
{
    public static async Task<T> UntilAsync<T>(
        Func<Task<T>> probe,
        Func<T, bool> predicate,
        TimeSpan timeout,
        TimeSpan interval,
        string description)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        T last = default!;

        while (DateTimeOffset.UtcNow < deadline)
        {
            last = await probe();
            if (predicate(last))
                return last;

            await Task.Delay(interval);
        }

        throw new TimeoutException(
            $"Timed out waiting for {description}. Last value: {last}");
    }
}
