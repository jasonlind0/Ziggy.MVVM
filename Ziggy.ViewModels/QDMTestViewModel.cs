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
using Ziggy.Factories;

namespace Ziggy.ViewModels
{
    public class QDMViewModel : BindableBase
    {

        private bool _isSampleRateValid;
        private bool _isAcqEnPulseDurationValid = true;
        private bool _isClockFrequencyValid = true;
        private bool _isClockDelayValid = true;
        private bool _isQuadNumCycleValid = true;
        private bool _isQuadPulseDurationValid = true;
        private bool _isQuadPulseDelayValid = true;

        private IDispatcher _dispatcher { get; set; }
        private QDMTest _model { get; set; }
        public InteractionRequest<Notification> RunErrorNotification { get; private set; }
        public string Address { get; private set; }
        public double SampleRate
        {
            get { return _model.SampleRate; }
            set
            {
                try
                {
                    _model.SampleRate = value;
                    IsSampleRateValid = true;
                    OnPropertyChanged(() => SampleRate);
                }
                catch (ArgumentException)
                {
                    IsSampleRateValid = false;
                }
            }
        }

        public bool IsSampleRateValid
        {
            get { return _isSampleRateValid; }
            set
            {
                if (SetProperty(ref _isSampleRateValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public double AcqEnPulseDuration
        {
            get { return _model.AcqEnable.PulseDuration; }
            set
            {
                try
                {
                    _model.AcqEnable.PulseDuration = value;
                    IsAcqEnPulseDurationValid = true;
                    OnPropertyChanged(() => AcqEnPulseDuration);
                }
                catch (ArgumentException)
                {
                    IsAcqEnPulseDurationValid = false;
                }
            }
        }

        public bool IsAcqEnPulseDurationValid
        {
            get { return _isAcqEnPulseDurationValid; }
            set
            {
                if (SetProperty(ref _isAcqEnPulseDurationValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public double ClockFrequency
        {
            get { return _model.Clock.Frequency; }
            set
            {
                try
                {
                    _model.Clock.Frequency = value;
                    IsClockFrequencyValid = true;
                    OnPropertyChanged(() => ClockFrequency);
                }
                catch (ArgumentException)
                {
                    IsClockFrequencyValid = false;
                }
            }
        }

        public bool IsClockFrequencyValid
        {
            get { return _isClockFrequencyValid; }
            set
            {
                if (SetProperty(ref _isClockFrequencyValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public double ClockDelay
        {
            get { return _model.Clock.Delay; }
            set
            {
                try
                {
                    _model.Clock.Delay = value;
                    IsClockDelayValid = true;
                    OnPropertyChanged(() => ClockDelay);
                }
                catch (ArgumentException)
                {
                    IsClockDelayValid = false;
                }
            }
        }

        public bool IsClockDelayValid
        {
            get { return _isClockDelayValid; }
            set
            {
                if (SetProperty(ref _isClockDelayValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public int QuadNumCycles
        {
            get { return _model.Quad.NumCycles; }
            set
            {
                try
                {
                    _model.Quad.NumCycles = value;
                    IsQuadNumCycleValid = true;
                    OnPropertyChanged(() => QuadNumCycles);
                }
                catch (ArgumentException)
                {
                    IsQuadNumCycleValid = false;
                }
            }
        }

        public bool IsQuadNumCycleValid
        {
            get { return _isQuadNumCycleValid; }
            set
            {
                if (SetProperty(ref _isQuadNumCycleValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public double QuadPulseDuration
        {
            get { return _model.Quad.PulseDuration; }
            set
            {
                try
                {
                    _model.Quad.PulseDuration = value;
                    IsQuadPulseDurationValid = true;
                    OnPropertyChanged(() => QuadPulseDuration);
                }
                catch (ArgumentException)
                {
                    IsQuadPulseDurationValid = false;
                }
            }
        }

        public bool IsQuadPulseDurationValid
        {
            get { return _isQuadPulseDurationValid; }
            set
            {
                if (SetProperty(ref _isQuadPulseDurationValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public double QuadPulseDelay
        {
            get { return _model.Quad.Delay; }
            set
            {
                try
                {
                    _model.Quad.Delay = value;
                    IsQuadPulseDelayValid = true;
                    OnPropertyChanged(() => QuadPulseDelay);
                }
                catch (ArgumentException)
                {
                    IsQuadPulseDelayValid = false;
                }
            }
        }

        public bool IsQuadPulseDelayValid
        {
            get { return _isQuadPulseDelayValid; }
            set
            {
                if (SetProperty(ref _isQuadPulseDelayValid, value))
                {
                    OnPropertyChanged(() => IsValid);
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return IsSampleRateValid &&
                    IsAcqEnPulseDurationValid &&
                    IsClockDelayValid &&
                    IsClockFrequencyValid &&
                    IsQuadNumCycleValid &&
                    IsQuadPulseDelayValid &&
                    IsQuadPulseDurationValid;
            }
        }

        public DelegateCommand Run { get; private set; }
        public DelegateCommand Trigger { get; private set; }
        public QDMViewModel(IDispatcher dispatcher, string address)
        {
            this.Address = address;
            _dispatcher = dispatcher;
            var taborArb = TaborArb2074Factory.Create(address);
            _model = QDMTestFactory.Create(taborArb);
            Run = new DelegateCommand(() => DoRun(), () => IsValid);
            Trigger = new DelegateCommand(() => _model.BusTrigger(), () => IsValid);
            RunErrorNotification = new InteractionRequest<Notification>();
            this.PropertyChanged += QDMViewModel_PropertyChanged;
        }

        void QDMViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                this.Run.RaiseCanExecuteChanged();
                this.Trigger.RaiseCanExecuteChanged();
            }
        }

        private async void DoRun()
        {
            await Task.Yield();
            try
            {
                _model.Run();
            }
            catch (Exception ex)
            {
                this.RunErrorNotification.Raise(new Notification()
                {
                    Title = "Could not Run QDM",
                    Content = ex.Message
                });
            }
        }
    }
}
