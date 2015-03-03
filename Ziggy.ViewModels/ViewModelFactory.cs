using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;

namespace Ziggy.ViewModels
{
    public class ViewModelFactory
    {
        protected static Func<Type, object> FactoryMethod { get; private set; }
        public static void Initialize(Func<Type, object> factoryMethod)
        {
            FactoryMethod = factoryMethod;
        }
    }
    public class ViewModelFactory<T> : ViewModelFactory
    {
        public static T Create()
        {
            if (FactoryMethod != null)
                return (T)FactoryMethod(typeof(T));
            throw new InvalidOperationException("ViewModelFactory has not been initialized");
        }
    }
    public static class QDMViewModelFactory
    {
        private static Func<string, QDMViewModel> FactoryMethod { get; set; }
        public static void Initalize(Func<string, QDMViewModel> factoryMethod)
        {
            FactoryMethod = factoryMethod;
        }
        public static QDMViewModel Create(string address)
        {
            if (FactoryMethod != null)
                return FactoryMethod(address);
            throw new InvalidOperationException("QDMViewModelFactory has not been initialized");
        }
    }
}
