namespace Lip.GUI;

public static class ViewExtensions
{
    public static async ValueTask ExecuteInUIThreadAsync(this View view, Func<ValueTask> valueTask)
    {
        await Task.Run(() =>
        {
            AutoResetEvent resetEvent = new(false);
            view.Dispatcher.Dispatch(async () =>
            {
                await valueTask();
                resetEvent.Set();
            });
            resetEvent.WaitOne();
        });
    }

    public static async ValueTask ExecuteInUIThreadAsync(this View view, Func<Task> task)
    {
        await Task.Run(() =>
        {
            AutoResetEvent resetEvent = new(false);
            view.Dispatcher.Dispatch(async () =>
            {
                await task();
                resetEvent.Set();
            });
            resetEvent.WaitOne();
        });
    }

    public static async ValueTask ExecuteInUIThreadAsync(this Page view, Func<Task> task)
    {
        await Task.Run(() =>
        {
            AutoResetEvent resetEvent = new(false);
            view.Dispatcher.Dispatch(async () =>
            {
                await task();
                resetEvent.Set();
            });
            resetEvent.WaitOne();
        });
    }


    public static async ValueTask ExecuteInUIThreadAsync(this Page view, Func<ValueTask> valueTask)
    {
        await Task.Run(() =>
        {
            AutoResetEvent resetEvent = new(false);
            view.Dispatcher.Dispatch(async () =>
            {
                await valueTask();
                resetEvent.Set();
            });
            resetEvent.WaitOne();
        });
    }


}
