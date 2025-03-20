namespace Lip.GUI;

public static class ViewExtensions
{
    public static async ValueTask ExecuteInUIThreadAsync(this View view, Func<ValueTask> valueTask)
    {
        var tcs = new TaskCompletionSource();
        view.Dispatcher.Dispatch(async () =>
        {
            try
            {
                await valueTask();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        await tcs.Task;
    }

    public static async ValueTask ExecuteInUIThreadAsync(this View view, Func<Task> task)
    {
        var tcs = new TaskCompletionSource();
        view.Dispatcher.Dispatch(async () =>
        {
            try
            {
                await task();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        await tcs.Task;
    }

    public static async ValueTask ExecuteInUIThreadAsync(this Page view, Func<Task> task)
    {
        var tcs = new TaskCompletionSource();
        view.Dispatcher.Dispatch(async () =>
        {
            try
            {
                await task();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        await tcs.Task;
    }

    public static async ValueTask ExecuteInUIThreadAsync(this Page view, Func<ValueTask> valueTask)
    {
        var tcs = new TaskCompletionSource();
        view.Dispatcher.Dispatch(async () =>
        {
            try
            {
                await valueTask();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        await tcs.Task;
    }
}
