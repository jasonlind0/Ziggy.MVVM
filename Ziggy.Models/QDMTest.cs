using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ziggy.Models
{
    public class QDMTest
    {
        private QDMState _qdmState;

        public QDMAcqEnable AcqEnable { get; private set; }
        public QDMClock Clock { get; private set; }
        public QDMQuad Quad { get; private set; }

        public QDMTest(dynamic device)
        {
            if (device == null)
                throw new ArgumentNullException();

            _qdmState = new QDMState(device);
            Initialize();
        }

        private void Initialize()
        {
            AcqEnable = new QDMAcqEnable(_qdmState);
            Clock = new QDMClock(_qdmState);
            Quad = new QDMQuad(_qdmState);
            _qdmState.IdleDelayLength = 16;
        }

        public void Run()
        {
            _qdmState.Device.SetOutputOff(Enumerable.Range(1, 4).ToArray());
            //_qdmState.Device.Trigger.TriggerSource = TrigSource.Bus;
            //_qdmState.Device.SetFunctionMode(FunctionMode.Fixed);
            AcqEnable.Setup();
            Quad.Setup();
            Clock.Setup(); // Must come last. Depends on Quad length.
            //_qdmState.Device.SetFunctionMode(FunctionMode.Sequenced);
            _qdmState.Device.SetOutputOn(Enumerable.Range(1, 4).ToArray());
        }

        public double SampleRate
        {
            get { return _qdmState.SampleRate; }
            set
            {
                //_qdmState.Device.SetSampleRate(value);
                _qdmState.SampleRate = value;
            }
        }

        public void BusTrigger()
        {
            _qdmState.Device.BusTrigger();
        }
    }

    internal class QDMState
    {
        public dynamic Device { get; private set; }
        public QDMChannel Channel { get; private set; }
        public dynamic Generate { get; private set; }
        public double SampleRate { get; set; }
        public int AcqEnableLength { get; set; }
        public int IdleDelayLength { get; set; }
        public int QuadCycles { get; set; }
        public double QuadPulseDuration { get; set; }

        public QDMState(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException();
            }

            Device = device;
            Initialize();
        }

        private void Initialize()
        {
            Channel = new QDMChannel(Device);
            //Generate = new TraceGenerate(Device);
        }

    }

    internal class QDMChannel
    {
        private dynamic _device;

        public QDMChannel(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException();
            }

            _device = device;
        }

        public void Initialize(int chan, double highValue)
        {
            _device.SetActiveChannel(chan);
            _device.Trace.DeleteAllSegments();
            _device.SetLoad(1000000.0);
            _device.SetAmplitude(highValue);
            _device.SetOffset(highValue / 2.0);
            //Debug.Assert(!_device.HasError);
        }
    }

    public class QDMAcqEnable
    {
        private const int _numCycles = 2;
        private QDMState _qdmState = null;
        private double _pulseDuration = 0;
        private int _pulseSampleLength = 0;

        internal QDMAcqEnable(QDMState qdmState)
        {
            if (qdmState == null)
            {
                throw new ArgumentNullException();
            }

            _qdmState = qdmState;
        }

        internal void Setup()
        {
            const double highValue = 3.3;

            if (_pulseDuration <= 0 || _pulseSampleLength <= 1)
            {
                throw new InvalidOperationException("Invalid pulse duration applied.");
            }

            _qdmState.Channel.Initialize(1, highValue);
            _qdmState.Generate.DC(1, _qdmState.IdleDelayLength, UInt16.MaxValue);
            _qdmState.Generate.Square(2, PulseSampleLength * 2, 0.0, highValue);
            SequenceSetup();
        }

        public double PulseDuration
        {
            get { return _pulseDuration; }
            set { setPulseValue(_qdmState.Device.Trace.Round.ToValidLength(_qdmState.SampleRate, value * 2) / 2); }
        }

        public int PulseSampleLength
        {
            get { return _pulseSampleLength; }
            set { setPulseValue(_qdmState.Device.Trace.Round.ToValidLength(value * 2) / 2); }
        }

        private void setPulseValue(int pulseLength)
        {
            _pulseSampleLength = pulseLength;
            //_pulseDuration = Sampling.GetDuration(_qdmState.SampleRate, _pulseSampleLength);
            _qdmState.AcqEnableLength = pulseLength * 2 * _numCycles;
        }

        private void SequenceSetup()
        {
           // _qdmState.Device.Sequence.AdvanceMode = SeqAdvanceMode.Mixed;

            //SequenceStep ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = 1;
            //ss.Segment = 1;
            //ss.Advance = SeqAdvanceStepMode.Step;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);

            //ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = 2;
            //ss.Segment = 2;
            //ss.Repeat = _numCycles;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);
        }

    }

    public class QDMClock
    {
        private QDMState _qdmState = null;
        private double _frequency = 0;
        private double _delay = 0;
        private int _periodLength = 0;
        private int _delayLength = 0;
        private const double _highValue = 3.3;

        internal QDMClock(QDMState qdmState)
        {
            if (qdmState == null)
            {
                throw new ArgumentNullException();
            }

            _qdmState = qdmState;
        }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delayLength = _qdmState.Device.Trace.Round.ToValidLength(_qdmState.SampleRate, value);
                //_delay = Sampling.GetDuration(_qdmState.SampleRate, _delayLength);
            }
        }

        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _periodLength = _qdmState.Device.Trace.Round.ToValidLength(_qdmState.SampleRate, 1 / value);
                //_frequency = 1 / Sampling.GetDuration(_qdmState.SampleRate, _periodLength);
            }
        }

        internal void Setup()
        {
            _qdmState.Channel.Initialize(2, _highValue);
            TraceSetup();
            SequenceSetup();
        }

        private void TraceSetup()
        {
            _qdmState.Generate.DC(1, _qdmState.IdleDelayLength, UInt16.MinValue);
            _qdmState.Generate.DC(2, _delayLength + _qdmState.AcqEnableLength, UInt16.MinValue);
            _qdmState.Generate.Square(3, _periodLength, 0.0, _highValue);
        }

        private void SequenceSetup()
        {
            int segmentNum = 1;

            // Step 1
            //SequenceStep ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum++;
            //ss.Advance = SeqAdvanceStepMode.Step;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);

            //// Step 2
            //ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum++;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);

            //// Step 3
            //ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum++;
            //ss.Repeat = (int)Math.Ceiling(_qdmState.QuadPulseDuration * 2 * _qdmState.QuadCycles * _frequency);
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);
        }
    }

    public class QDMQuad
    {
        private enum PulsePhase { isLeading, isLagging };
        private const double highValue = 5;
        private QDMState _qdmState;
        private int _cycles = 0;
        private double _pulseDuration = 0;
        private int _pulseLength = 0;
        private double _delay = 0;
        private int _delayLength = 0;

        internal QDMQuad(QDMState qdmState)
        {
            if (qdmState == null)
            {
                throw new ArgumentNullException();
            }

            _qdmState = qdmState;
        }

        public int NumCycles
        {
            get { return _cycles; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _cycles = value;
                _qdmState.QuadCycles = _cycles;
            }
        }

        public double PulseDuration
        {
            get { return _pulseDuration; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _pulseLength = _qdmState.Device.Trace.Round.ToValidLength(_qdmState.SampleRate, value);
                //_pulseDuration = Sampling.GetDuration(_qdmState.SampleRate, _pulseLength);
                _qdmState.QuadPulseDuration = _pulseDuration;
            }
        }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delayLength = _qdmState.Device.Trace.Round.ToValidLength(_qdmState.SampleRate, value);
                // _delay = Sampling.GetDuration(_qdmState.SampleRate, _delayLength);
            }
        }

        internal void Setup()
        {
            _qdmState.Channel.Initialize(3, highValue);
            TraceSetup(PulsePhase.isLeading);
            _qdmState.Channel.Initialize(4, highValue);
            TraceSetup(PulsePhase.isLagging);
            SequenceTableSetup();
        }

        private void TraceSetup(PulsePhase phase)
        {
            int totalDelayLength = _delayLength;

            if (phase == PulsePhase.isLagging)
            {
                totalDelayLength += _pulseLength / 2;
            }

            // Initialization Delay
            _qdmState.Generate.DC(1, _qdmState.IdleDelayLength, UInt16.MinValue);
            // Acq Enable Synchronization Delay + User Delay
            _qdmState.Generate.DC(2, totalDelayLength + _qdmState.AcqEnableLength, UInt16.MinValue);
            // Pulse Generation
            _qdmState.Generate.Square(3, _pulseLength * 2, 0.0, highValue);
            SequenceTableSetup();
        }

        private void SequenceTableSetup()
        {
            int segmentNum = 1;

            // Step 1
            //SequenceStep ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum++;
            //ss.Advance = SeqAdvanceStepMode.Step;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);

            // Step 2
            //ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum++;
            //_qdmState.Device.Sequence.Define(ss);
            //Debug.Assert(!_qdmState.Device.HasError);

            // Step 3
            //ss = _qdmState.Device.Sequence.StepBuilder();
            //ss.Step = ss.Segment = segmentNum;
            //ss.Repeat = _cycles;
            //_qdmState.Device.Sequence.Define(ss);
           // Debug.Assert(!_qdmState.Device.HasError);
        }
    }
    public class TaborArb2074
    {
        public TaborArb2074(string address) { }
    }
}
