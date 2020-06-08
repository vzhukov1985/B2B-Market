using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Core.Services
{
    public interface IRelayCommand: ICommand
    {
        void Create(Action<Object> execute, Func<object, bool> canExecute = null);
    }
}
