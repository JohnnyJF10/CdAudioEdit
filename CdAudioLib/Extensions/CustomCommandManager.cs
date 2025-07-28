namespace CdAudioLib.Extensions
{
    public static class CustomCommandManager
    {
        private static readonly List<EventHandler> Handlers = new List<EventHandler>();

        public static event EventHandler? RequerySuggested;

        public static void AddCanExecuteChangedHandler(EventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Handlers.Add(handler);
            RequerySuggested += handler;
        }

        public static void RemoveCanExecuteChangedHandler(EventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Handlers.Remove(handler);
            RequerySuggested -= handler;
        }

        public static void InvalidateRequerySuggested()
        {
            foreach (var handler in Handlers.ToArray())
            {
                handler?.Invoke(null, EventArgs.Empty);
            }
            RequerySuggested?.Invoke(null, EventArgs.Empty);
        }
    }
}
