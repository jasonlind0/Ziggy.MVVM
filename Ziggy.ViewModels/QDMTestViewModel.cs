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
        private IDispatcher _dispatcher { get; set; }
        private QDMTest _model { get; set; }
        public InteractionRequest<Notification> RunErrorNotification { get; private set; }
        public string Address { get; private set; }
        public DoubleState SampleRate { get; private set; }

        public ValidationState IsSampleRateValid
        {
            get;
            private set;
        }

        public DoubleState AcqEnPulseDuration { get; private set; }

        public ValidationState IsAcqEnPulseDurationValid
        {
            get;
            private set;
        }

        public DoubleState ClockFrequency { get; private set; }

        public ValidationState IsClockFrequencyValid
        {
            get;
            private set;
        }
        public DoubleState ClockDelay { get; private set; }

        public ValidationState IsClockDelayValid
        {
            get;
            private set;
        }
        public IntegerState QuadNumCycles { get; private set; }

        public ValidationState IsQuadNumCycleValid
        {
            get;
            private set;
        }
        public DoubleState QuadPulseDuration { get; private set; }

        public ValidationState IsQuadPulseDurationValid
        {
            get;
            private set;
        }
        public DoubleState QuadPulseDelay { get; private set; }

        public ValidationState IsQuadPulseDelayValid
        {
            get;
            private set;
        }

        public bool IsValid
        {
            get
            {
                return IsSampleRateValid.IsValid &&
                    IsAcqEnPulseDurationValid.IsValid &&
                    IsClockDelayValid.IsValid &&
                    IsClockFrequencyValid.IsValid &&
                    IsQuadNumCycleValid.IsValid &&
                    IsQuadPulseDelayValid.IsValid &&
                    IsQuadPulseDurationValid.IsValid;
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
            Action raisePropertyValid = () => this.OnPropertyChanged(() => this.IsValid);
            this.IsSampleRateValid = new ValidationState(true, false, stateChange: raisePropertyValid);
            this.SampleRate = new DoubleState(v => this._model.SampleRate = v.Value, () => this._model.SampleRate, validationState: this.IsSampleRateValid, initialValue: this._model.SampleRate);
            this.IsAcqEnPulseDurationValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.AcqEnPulseDuration = new DoubleState(v => this._model.AcqEnable.PulseDuration = v.Value, () => this._model.AcqEnable.PulseDuration, validationState: this.IsAcqEnPulseDurationValid);
            this.IsClockDelayValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.ClockDelay = new DoubleState(v => this._model.Clock.Delay = v.Value, () => this._model.Clock.Delay, validationState: this.IsClockDelayValid);
            this.IsClockFrequencyValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.ClockFrequency = new DoubleState(v => this._model.Clock.Frequency = v.Value, () => this._model.Clock.Frequency, validationState: this.IsClockFrequencyValid);
            this.IsQuadNumCycleValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.QuadNumCycles = new IntegerState(v => this._model.Quad.NumCycles = v.Value, () => this._model.Quad.NumCycles, validationState: this.IsQuadNumCycleValid);
            this.IsQuadPulseDelayValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.QuadPulseDelay = new DoubleState(v => this._model.Quad.Delay = v.Value, () => this._model.Quad.Delay, validationState: this.IsQuadPulseDelayValid);
            this.IsQuadPulseDurationValid = new ValidationState(false, isValidIfEmptyInitial: true, stateChange: raisePropertyValid);
            this.QuadPulseDuration = new DoubleState(v => this._model.Quad.PulseDuration = v.Value, () => this._model.Quad.PulseDuration, validationState: this.IsQuadPulseDurationValid);
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
