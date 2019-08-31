using System.Windows.Threading;
using System.Windows;

namespace Viking.Pipeline.Wpf
{
    public static class DispatcherUtilities
    {

        public static Dispatcher DefaultDispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}
