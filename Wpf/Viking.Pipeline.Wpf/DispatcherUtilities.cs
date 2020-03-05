using System.Windows;
using System.Windows.Threading;

namespace Viking.Pipeline.Wpf
{
    public static class DispatcherUtilities
    {

        public static Dispatcher DefaultDispatcher => Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
}
