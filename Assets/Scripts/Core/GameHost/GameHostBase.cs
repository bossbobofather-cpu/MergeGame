using System;
using System.Collections.Concurrent;
using System.Threading;
using Diagnostics = System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public abstract class GameHostBase<TCommand, TResult, TEvent, TSnapshot>
        : IGameHost<TCommand, TResult, TEvent, TSnapshot>,
          IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>,
          IDisposable
        where TCommand : GameCommandBase
        where TResult : GameCommandResultBase
        where TEvent : GameEventBase
        where TSnapshot : GameSnapshotBase
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private readonly ConcurrentQueue<TCommand> _pendingCommands = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private readonly ConcurrentQueue<DispatchItem> _dispatchQueue = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private readonly ConcurrentQueue<TSnapshot> _snapshotQueue = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private readonly object _lifecycleLock = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private Thread _loopThread;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private float _fixedStep = 1f / 30f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private int _maxStepsPerTick = 8;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private int _sleepMilliseconds = 1;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private int _stopTimeoutMilliseconds = 5000;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private float _snapshotInterval = 0f;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private double _snapshotAccumulator;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private TSnapshot _latestSnapshot;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        private Exception _loopException;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public long Tick { get; private set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public Exception LoopException => _loopException;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float FixedStep
        {
            get => _fixedStep;
            set => _fixedStep = value > 0f ? value : 1f / 30f;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int MaxStepsPerTick
        {
            get => _maxStepsPerTick;
            set => _maxStepsPerTick = value > 0 ? value : 1;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int SleepMilliseconds
        {
            get => _sleepMilliseconds;
            set => _sleepMilliseconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int StopTimeoutMilliseconds
        {
            get => _stopTimeoutMilliseconds;
            set => _stopTimeoutMilliseconds = value < 0 ? 5000 : value;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float SnapshotInterval
        {
            get => _snapshotInterval;
            set => _snapshotInterval = value < 0f ? 0f : value;
        }

        public event Action<TResult> ResultProduced;
        public event Action<TEvent> EventRaised;
        /// <summary>
        /// SendCommand 메서드입니다.
        /// </summary>

        public void SendCommand(TCommand command)
        {
            if (command == null)
            {
                return;
            }

            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void StartSimulation()
        {
            ThrowIfDisposed();

            lock (_lifecycleLock)
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;
                _loopException = null;
                _loopThread = new Thread(RunLoop)
                {
                    IsBackground = true,
                    Name = $"{GetType().Name}-Loop"
                };
                _loopThread.Start();
            }
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void StopSimulation()
        {
            lock (_lifecycleLock)
            {
                if (!_isRunning)
                {
                    return;
                }

                _isRunning = false;
            }

            if (_loopThread != null)
            {
                var joined = _loopThread.Join(_stopTimeoutMilliseconds);
                if (!joined)
                {
                    GameHostLog.LogWarning(
                        $"[{GetType().Name}] ?쒕??덉씠???ㅻ젅?쒓? ?쒗븳 ?쒓컙 ??醫낅즺?섏? ?딆븯?듬땲??({_stopTimeoutMilliseconds}ms)");
                }
                _loopThread = null;
            }
        }

        TSnapshot IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.BuildSnapshot()
        {
            var latest = default(TSnapshot);
            while (_snapshotQueue.TryDequeue(out var snapshot))
            {
                latest = snapshot;
            }
            if (latest != null)
            {
                return latest;
            }

            return Volatile.Read(ref _latestSnapshot);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        protected abstract GameCommandOutcome<TResult, TEvent> HandleCommand(TCommand command);

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Submit(TCommand command)
        {
            SendCommand(command);
        }

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Advance(float deltaSeconds)
        {
            Tick++;
            while (_pendingCommands.TryDequeue(out var command))
            {
                try
                {
                    var outcome = HandleCommand(command);
                    if (outcome.PreEvents != null)
                    {
                        foreach (var preEvent in outcome.PreEvents)
                        {
                            PublishEvent(preEvent);
                        }
                    }
                    if (outcome.Result != null)
                    {
                        PublishResult(outcome.Result);
                    }
                    if (outcome.PostEvents != null)
                    {
                        foreach (var postEvent in outcome.PostEvents)
                        {
                            PublishEvent(postEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameHostLog.LogError($"[{GetType().Name}] 而ㅻ㎤??泥섎━ ?ㅻ쪟 {command?.GetType().Name}: {ex}");
                    PublishEvent(CreateErrorEvent(command, ex));
                }
            }

            try
            {
                OnTick(deltaSeconds);
            }
            catch (Exception ex)
            {
                GameHostLog.LogError($"[{GetType().Name}] OnTick ?ㅻ쪟: {ex}");
            }

            TryBuildSnapshot(deltaSeconds);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        protected abstract void OnTick(float deltaSeconds);

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        protected abstract TSnapshot BuildSnapshotInternal();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        protected virtual TEvent CreateErrorEvent(TCommand command, Exception exception)
        {
            return default;
        }
        /// <summary>
        /// PublishResult 메서드입니다.
        /// </summary>

        protected void PublishResult(TResult result)
        {
            if (result == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForResult(result));
        }
        /// <summary>
        /// PublishEvent 메서드입니다.
        /// </summary>

        protected void PublishEvent(TEvent eventData)
        {
            if (eventData == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForEvent(eventData));
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void FlushEvents()
        {
            while (_dispatchQueue.TryDequeue(out var item))
            {
                try
                {
                    if (item.IsResult)
                    {
                        ResultProduced?.Invoke(item.Result);
                    }
                    else
                    {
                        EventRaised?.Invoke(item.EventData);
                    }
                }
                catch (Exception ex)
                {
                    GameHostLog.LogError($"[{GetType().Name}] ?대깽???붿뒪?⑥튂 ?ㅻ쪟: {ex}");
                }
            }
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public TSnapshot GetLatestSnapshot()
        {
            return Volatile.Read(ref _latestSnapshot);
        }
        /// <summary>
        /// TryBuildSnapshot 메서드입니다.
        /// </summary>

        private void TryBuildSnapshot(float deltaSeconds)
        {
            if (_snapshotInterval <= 0f)
            {
                EnqueueSnapshot();
                return;
            }

            _snapshotAccumulator += deltaSeconds;
            if (_snapshotAccumulator < _snapshotInterval)
            {
                return;
            }

            _snapshotAccumulator -= _snapshotInterval;
            EnqueueSnapshot();
        }
        /// <summary>
        /// EnqueueSnapshot 메서드입니다.
        /// </summary>

        private void EnqueueSnapshot()
        {
            try
            {
                var snapshot = BuildSnapshotInternal();
                if (snapshot == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GameHostLog.LogError($"[{GetType().Name}] ?ㅻ깄???앹꽦 ?ㅻ쪟: {ex}");
            }
        }

        private readonly struct DispatchItem
        {
            public bool IsResult { get; }
            public TResult Result { get; }
            public TEvent EventData { get; }

            private DispatchItem(TResult result, TEvent eventData, bool isResult)
            {
                Result = result;
                EventData = eventData;
                IsResult = isResult;
            }
            /// <summary>
            /// ForResult 메서드입니다.
            /// </summary>

            public static DispatchItem ForResult(TResult result)
            {
                return new DispatchItem(result, default, true);
            }
            /// <summary>
            /// ForEvent 메서드입니다.
            /// </summary>

            public static DispatchItem ForEvent(TEvent eventData)
            {
                return new DispatchItem(default, eventData, false);
            }
        }
        /// <summary>
        /// RunLoop 메서드입니다.
        /// </summary>

        private void RunLoop()
        {
            try
            {
                var stopwatch = Diagnostics.Stopwatch.StartNew();
                var lastTime = stopwatch.Elapsed;
                var accumulator = 0.0;
                var host = (IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>)this;

                while (_isRunning)
                {
                    var now = stopwatch.Elapsed;
                    var deltaSeconds = (now - lastTime).TotalSeconds;
                    lastTime = now;
                    if (deltaSeconds > 0.25) deltaSeconds = 0.25;

                    accumulator += deltaSeconds;
                    var step = _fixedStep;
                    var steps = 0;

                    while (accumulator >= step && steps < _maxStepsPerTick)
                    {
                        host.Advance(step);
                        accumulator -= step;
                        steps++;
                    }
                    // 과도한 지연이 누적되면 다음 프레임으로 누적 시간을 넘기지 않고 초기화합니다.
                    if (steps >= _maxStepsPerTick) accumulator = 0.0;

                    if (_sleepMilliseconds > 0) Thread.Sleep(_sleepMilliseconds);
                    else Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                _loopException = ex;
                GameHostLog.LogError($"[{GetType().Name}] ?쒕??덉씠??猷⑦봽 ?ㅻ쪟: {ex}");
            }
        }
        /// <summary>
        /// ThrowIfDisposed 메서드입니다.
        /// </summary>

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
        /// <summary>
        /// Dispose 메서드입니다.
        /// </summary>

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopSimulation();

            while (_pendingCommands.TryDequeue(out _)) { }
            while (_dispatchQueue.TryDequeue(out _)) { }
            while (_snapshotQueue.TryDequeue(out _)) { }
        }
    }
}
