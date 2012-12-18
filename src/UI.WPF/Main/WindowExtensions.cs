using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Predica.FimExplorer.UI.WPF.Main
{
    public static class WindowExtensions
    {
        public static TResult LongRunningOperation<TResult>(this Window @this, Panel loadingIndicator, Func<TResult> action)
        {
            TResult result = default(TResult);

            @this.LongRunningOperation(loadingIndicator, new Action(() => result = action()));

            return result;
        }

        public static void LongRunningOperation(this Window @this, Panel loadingIndicator, Action action)
        {
            var originalCursor = @this.Cursor;
            @this.Cursor = Cursors.Wait;
            loadingIndicator.Visibility = Visibility.Visible;

            DispatcherFrame frame = new DispatcherFrame();
            DispatcherOperation dispatcherOperation = Dispatcher.CurrentDispatcher
                .BeginInvoke(DispatcherPriority.ContextIdle,
                new Action(() =>
                    {
                        try
                        {
                            action();
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            @this.Cursor = originalCursor;
                        }
                    })
                );
            dispatcherOperation.Completed += delegate
                {
                    frame.Continue = false;
                };

            Dispatcher.PushFrame(frame);
        }
    }
}