using System.Diagnostics;

namespace Lip;

/// <summary>
/// Provides methods for waiting for a condition to be met.
/// </summary>
public static class ConditionWaiter
{
    private static readonly TimeSpan s_defaultInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Waits for the specified condition to be met.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="timeout">The timeout to wait for the condition to be met. If null, the method will wait indefinitely.</param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public static async Task WaitFor(Func<bool> condition, TimeSpan? timeout)
    {
        var sw = Stopwatch.StartNew();
        while (!condition())
        {
            if (timeout is not null && sw.Elapsed > timeout.Value)
            {
                throw new TimeoutException("The condition was not met within the specified timeout.");
            }

            await Task.Delay(s_defaultInterval);
        }
    }
}
