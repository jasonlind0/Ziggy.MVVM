using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using Ziggy.Models;

namespace Ziggy.ViewModels
{
    public class ValidationState<T> : BindableBase
    {
        private Action StateChange { get; set; }
        public ValidationState(bool isValid = false, bool isEmpty = true, bool isValidIfEmptyInitial = false, T message = default(T), Action stateChange = null)
        {
            this.IsValid = isValid;
            this.IsEmpty = isEmpty;
            this.Message = message;
            this.IsValidIfEmtptyInitial = isValidIfEmptyInitial;
            this.StateChange = stateChange;
            if(this.StateChange != null)
                this.PropertyChanged += ValidationState_PropertyChanged;
        }

        void ValidationState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StateChange();
        }
        private bool isValid;
        public bool IsValid
        {
            get { return this.isValid; }
            private set
            {
                if (SetProperty(ref isValid, value))
                    this.OnPropertyChanged(() => this.IsValidOrEmptyInitial);
            }
        }
        private bool isEmpty;
        public bool IsEmpty
        {
            get { return this.isEmpty; }
            private set
            {
                if (SetProperty(ref isEmpty, value))
                    this.OnPropertyChanged(() => this.IsValidOrEmptyInitial);
            }
        }
        private bool IsValidIfEmtptyInitial { get; set; }
        private bool isInitial = true;
        public bool IsInitial
        {
            get { return IsInitial; }
            private set
            {
                if (SetProperty(ref isInitial, value))
                    this.OnPropertyChanged(() => this.IsValidOrEmptyInitial);
            }
        }
        private T message;
        public T Message
        {
            get { return this.message; }
            set
            {
                SetProperty(ref message, value);
            }
        }
        public bool IsValidOrEmptyInitial
        {
            get { return this.IsValid || (this.IsValidIfEmtptyInitial && this.IsInitial && this.IsEmpty); }
        }
        public bool IsValidOrEmpty
        {
            get { return this.IsValid || this.IsEmpty; }
        }
        public void SetValid(T message = default(T), bool isEmpty = false)
        {
            this.IsInitial = false;
            this.IsValid = true;
            this.IsEmpty = isEmpty;
            this.Message = message;
        }
        public void SetEmpty(T message = default(T), bool? isValid = null)
        {
            this.IsInitial = false;
            this.IsEmpty = true;
            if(isValid != null)
                this.IsValid = isValid.Value;
            this.Message = message;
        }
        public void SetInvalid(T message = default(T), bool isEmpty = false)
        {
            this.IsInitial = false;
            this.IsValid = false;
            this.IsEmpty = isEmpty;
            this.Message = message;
        }
    }
    public class ValidationState : ValidationState<string>
    {
        public ValidationState(bool isValid = false, bool isEmpty = true, bool isValidIfEmptyInitial = false, string message = null, Action stateChange = null) : 
            base(isValid, isEmpty, isValidIfEmptyInitial, isEmpty && message == null ? "Value not set" : message, stateChange) { }
    }
    public abstract class State : BindableBase
    {
        public State(ValidationState validationState = null)
        {
            this.IsValid = validationState;
        }
        public abstract string StringValue { get; set; }
        public ValidationState IsValid { get; private set; }
    }
    public class State<TValue> : State
    {
        public State(Action<TValue> setValue, Func<TValue> getValue, Func<string, TValue> parseValue, Action valueSet = null, ValidationState validationState = null, 
            TValue initialValue = default(TValue), Func<TValue, Tuple<bool, string>> validate = null) : base(validationState)
        {
            this.SetValue = setValue;
            this.GetValue = getValue;
            this.ParseValue = parseValue;
            this.value = initialValue;
            if (validate == null)
                validate = v => new Tuple<bool, string>(true, null);
            this.Validate = validate;
            
        }
        private TValue value;
        private Action<TValue> SetValue { get; set; }
        private Func<TValue> GetValue { get; set; }
        private Func<string, TValue> ParseValue { get; set; }
        private Action ValueSet { get; set; }
        private Func<TValue, Tuple<bool, string>> Validate { get; set; }
        public TValue Value
        {
            get { return this.value; }
            private set
            {
                var objNewVal = value as object;
                var objCurrentValue = this.value as object;
                if (objNewVal != objCurrentValue)
                {
                    try
                    {
                        var validation = this.Validate(value);
                        if (objNewVal != null && validation.Item1)
                        {
                            SetValue(value);
                            if(IsValid != null)
                                IsValid.SetValid();
                            this.value = GetValue();
                        }
                        else if (!validation.Item1)
                        {
                            IsValid.SetInvalid(validation.Item2, value != null);
                            this.value = value;
                        }
                        else if(value == null)
                        {
                            if (IsValid != null)
                                IsValid.SetEmpty();
                            this.value = default(TValue);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        if(IsValid != null)
                            IsValid.SetInvalid(ex.Message);
                    }
                    this.OnPropertyChanged(() => this.Value);
                    this.OnPropertyChanged(() => this.StringValue);
                    if(ValueSet != null)
                        ValueSet();
                }
            }
        }
        public override string StringValue
        {
            get { return this.Value != null ? this.Value.ToString() : null; }
            set
            {
                try
                {
                    this.Value = ParseValue(value);
                }
                catch (Exception ex)
                {
                    if (this.IsValid != null)
                        this.IsValid.SetInvalid(ex.Message);
                }
            }
        }
    }
    public class DoubleState : State<double?>
    {
        public DoubleState(Action<double?> setValue, Func<double?> getValue, Action valueSet = null, ValidationState validationState = null, double? initialValue = null, Func<double?, Tuple<bool, string>> validate = null) :
            base(setValue, getValue,
                str => str != null ? double.Parse(str) : null as double?, valueSet, validationState, initialValue, validate) { }
    }
    public class IntegerState : State<int?>
    {
        public IntegerState(Action<int?> setValue, Func<int?> getValue, Action valueSet = null, ValidationState validationState = null, int? initialValue = null, Func<int?, Tuple<bool, string>> validate = null) :
            base(setValue, getValue,
             str => str != null ? int.Parse(str) : null as int?, valueSet, validationState, initialValue, validate) { }
    }
    public class StringState : State<string>
    {
        public StringState(Action<string> setValue, Func<string> getValue, Action valueSet = null, ValidationState validationState = null, string initialValue = null, Func<string, Tuple<bool, string>> validate = null) :
            base(setValue, getValue, str => str, valueSet, validationState, initialValue, validate) { }
    }
}
