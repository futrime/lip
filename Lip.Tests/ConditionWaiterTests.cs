namespace Lip.Tests;

public class ConditionWaiterTests
{
    [Fact]
    public async Task WaitAsync_TrueCondition_Passes()
    {
        // Arrange.
        static bool condition() => true;

        // Act.
        await ConditionWaiter.WaitFor(condition, timeout: null);

        // No assertion is needed.
    }

    [Fact]
    public async Task WaitAsync_FalseConditionTurnsTrue_Passes()
    {
        // Arrange.
        bool innerCondition = false;
        async Task delayAndSetCondition()
        {
            await Task.Delay(500);
            innerCondition = true;
        }
        bool condition() => innerCondition;

        // Act.
        var task = Task.Run(delayAndSetCondition);
        await ConditionWaiter.WaitFor(condition, timeout: null);
        await task;

        // No assertion is needed.
    }

    [Fact]
    public async Task WaitAsync_FalseConditionWithTimeout_ThrowsTimeoutException()
    {
        // Arrange.
        static bool condition() => false;
        TimeSpan timeout = TimeSpan.FromMilliseconds(500);

        // Act.
        TimeoutException exception = await Assert.ThrowsAsync<TimeoutException>(
            () => ConditionWaiter.WaitFor(condition, timeout));

        // Assert.
        Assert.Equal("The condition was not met within the specified timeout.", exception.Message);
    }

    [Fact]
    public async Task WaitAsync_FalseConditionTurnsTrueBeforeTimeout_Passes()
    {
        // Arrange.
        bool innerCondition = false;
        async Task delayAndSetCondition()
        {
            await Task.Delay(500);
            innerCondition = true;
        }
        bool condition() => innerCondition;
        TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

        // Act
        var task = Task.Run(delayAndSetCondition);
        await ConditionWaiter.WaitFor(condition, timeout);
        await task;

        // No assertion is needed.
    }

    [Fact]
    public async Task WaitAsync_FalseConditionTurnsTrueAfterTimeout_ThrowsTimeoutException()
    {
        // Arrange.
        bool innerCondition = false;
        async Task delayAndSetCondition()
        {
            await Task.Delay(1000);
            innerCondition = true;
        }
        bool condition() => innerCondition;
        TimeSpan timeout = TimeSpan.FromMilliseconds(500);

        // Act
        var task = Task.Run(delayAndSetCondition);
        TimeoutException exception = await Assert.ThrowsAsync<TimeoutException>(
            () => ConditionWaiter.WaitFor(condition, timeout));
        await task;

        // Assert.
        Assert.Equal("The condition was not met within the specified timeout.", exception.Message);
    }
}
