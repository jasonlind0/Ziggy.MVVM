using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Mvvm;
using Ziggy.Models;
using Ziggy.ViewModels;
using Ziggy.Factories;
using System.Reflection;
using System.Globalization;

namespace Ziggy.Views
{
    public class Bootstrapper
    {
        public IUnityContainer Container { get; protected set; }
        public virtual void Setup()
        {
            Container = new UnityContainer();
            Container.RegisterInstance<IDispatcher>(new WpfDispatcher());
            
            TaborArb2074Factory.Initialize(address => this.Container.Resolve<TaborArb2074>(new ParameterOverride("address", address)));
            QDMTestFactory.Initalize(taborArb => this.Container.Resolve<QDMTest>(new ParameterOverride("device", taborArb)));
            ViewModelFactory.Initialize(type => this.Container.Resolve(type));
            QDMViewModelFactory.Initalize(address => this.Container.Resolve<QDMViewModel>(new ParameterOverride("address", address)));

            ViewModelLocationProvider.SetDefaultViewModelFactory(type => this.Container.Resolve(type));
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName).Replace("Views", "ViewModels");
                
                return Type.GetType(viewModelName);
            });
        }
    }
}
