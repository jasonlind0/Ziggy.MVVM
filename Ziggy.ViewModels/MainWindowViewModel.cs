using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using Ziggy.Models;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.ObjectModel;

namespace Ziggy.ViewModels
{
    
    public class MainWindowViewModel : BindableBase
    {
        private HashSet<string> AddressesOpen { get; set; }
        private string address = "01";
        public string Address
        {
            get { return this.address; }
            set
            {
                int v;
                if (value.Length == 2 && int.TryParse(value, out v) && v >= 1 && v <= 32)
                {
                    SetProperty(ref this.address, value);
                    IsAddressValid = true;
                }
                else
                    IsAddressValid = false;
            }
        }
        private bool isAddressValid = true;
        public bool IsAddressValid
        {
            get { return this.isAddressValid && !AddressesOpen.Contains(this.Address); }
            set
            {
                this.isAddressValid = value;
                this.OnPropertyChanged(() => this.IsAddressValid);//always call because we're also checking the return value against the hashset
            }
        }
        public ObservableCollection<QDMViewModel> QDMConnections { get; private set; }
        private IDispatcher Dispatcher { get; set; }
        public DelegateCommand Connect { get; private set; }
        public DelegateCommand<QDMViewModel> Disconnect { get; private set; }
        public InteractionRequest<INotification> ConnectError { get; private set; }
        private QDMViewModel selectedQDMViewModel;
        public QDMViewModel SelectedQDMViewModel
        {
            get { return selectedQDMViewModel; }
            set { SetProperty(ref selectedQDMViewModel, value); }
        }
        public MainWindowViewModel(IDispatcher dispatcher)
        {
            AddressesOpen = new HashSet<string>();
            this.Dispatcher = dispatcher;
            Connect = new DelegateCommand(() => DoConnect(), () => IsAddressValid);
            QDMConnections = new ObservableCollection<QDMViewModel>();
            Disconnect = new DelegateCommand<QDMViewModel>(qdm =>
            {
                this.Dispatcher.Invoke(() => this.QDMConnections.Remove(qdm));
                this.SelectedQDMViewModel = this.QDMConnections.LastOrDefault();
                this.AddressesOpen.Remove(qdm.Address);
                this.OnPropertyChanged(() => this.IsAddressValid);
            });
            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
            this.ConnectError = new InteractionRequest<INotification>();
        }

        void MainWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsAddressValid")
                Connect.RaiseCanExecuteChanged();
        }

        private async void DoConnect()
        {
            this.AddressesOpen.Add(this.Address);
            this.OnPropertyChanged(() => this.IsAddressValid);
            var address = this.Address;
            await Task.Yield();
            try
            {
                var qdmVm = QDMViewModelFactory.Create(address);
                Dispatcher.Invoke(() => this.QDMConnections.Add(qdmVm));
                this.SelectedQDMViewModel = qdmVm;
            }
            catch (Exception ex)
            {
                this.ConnectError.Raise(new Notification()
                {
                    Title = "Could not connect to Address: "+ address,
                    Content = ex.Message
                });
            }
        }
    }
}
