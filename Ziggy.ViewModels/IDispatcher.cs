using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ziggy.ViewModels
{
    public interface IDispatcher
    {
        void Invoke(Action action);
    }
}
