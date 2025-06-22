using System.Windows.Input;

namespace Desktop.Commands;

public abstract class AsyncCommandBase : ICommand
{
    protected abstract Task ExecuteAsync();
    protected abstract bool CanExecute();

    public bool CanExecute(object? parameter)
    {
        return CanExecute();
    }

    public void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public event EventHandler? CanExecuteChanged;

    protected void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}