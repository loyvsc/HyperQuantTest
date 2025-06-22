namespace Desktop.Commands;

public class AsyncCommand : AsyncCommandBase
{
    private bool _isExecuting;
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    protected override bool CanExecute()
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    protected override async Task ExecuteAsync()
    {
        if (CanExecute())
        {
            try
            {
                RaiseCanExecuteChanged();
                _isExecuting = true;
                
                await _execute();
            }
            finally
            {
                _isExecuting = false;

                RaiseCanExecuteChanged();
            }
        }
    }
}