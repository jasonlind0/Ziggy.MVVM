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
        public StringState Address { get; private set; }
        public ValidationState IsAddressValid { get; private set; }
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
            IsAddressValid = new ValidationState(true, false);
            Address = new StringState(str => address = str, () => address, validationState: this.IsAddressValid, initialValue: address, validate: str =>
            {
                int v;
                var isValid = (str.Length == 2 && int.TryParse(str, out v) && v >= 1 && v <= 32);
                if (!isValid)
                    return new Tuple<bool, string>(false, "Channel out of range");
                if (AddressesOpen.Contains(str))
                    return new Tuple<bool, string>(false, "Channel is already open");
                return new Tuple<bool, string>(true, null);
            });
            AddressesOpen = new HashSet<string>();
            this.Dispatcher = dispatcher;
            Connect = new DelegateCommand(() => DoConnect(), () => IsAddressValid.IsValid);
            QDMConnections = new ObservableCollection<QDMViewModel>();
            Disconnect = new DelegateCommand<QDMViewModel>(qdm =>
            {
                this.Dispatcher.Invoke(() => this.QDMConnections.Remove(qdm));
                this.SelectedQDMViewModel = this.QDMConnections.LastOrDefault();
                this.AddressesOpen.Remove(qdm.Address);
                this.OnPropertyChanged(() => this.IsAddressValid);
            });
            this.IsAddressValid.PropertyChanged += IsAddressValid_PropertyChanged;
            this.ConnectError = new InteractionRequest<INotification>();
        }

        void IsAddressValid_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
                this.Connect.RaiseCanExecuteChanged();
        }

        private async void DoConnect()
        {
            var address = this.Address.Value;
            this.AddressesOpen.Add(address);
            this.IsAddressValid.SetInvalid("Channel connected");
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
