using System;
using System.Collections.Concurrent;
using System.Threading;
using Diagnostics = System.Diagnostics;

namespace Noname.GameHost
{
    /// <summary>
    /// 寃뚯엫 ?몄뒪??湲곕낯 ?대옒?ㅼ엯?덈떎.
    /// ?쒕쾭/?몄뒪???쒕??덉씠?섏쓣 愿由ы븯硫??ㅻ젅??湲곕컲 而ㅻ㎤??泥섎━,
    /// ?대깽???붿뒪?⑥튂, ?ㅻ깄???앹꽦???쒓났?⑸땲??
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
        /// 泥섎━ ?湲?以묒씤 而ㅻ㎤???먯엯?덈떎.
        /// </summary>
        private readonly ConcurrentQueue<TCommand> _pendingCommands = new();

        /// <summary>
        /// 硫붿씤 ?ㅻ젅?쒕줈 ?꾨떖???붿뒪?⑥튂 ?먯엯?덈떎.
        /// </summary>
        private readonly ConcurrentQueue<DispatchItem> _dispatchQueue = new();

        /// <summary>
        /// ?ㅻ깄???먯엯?덈떎.
        /// </summary>
        private readonly ConcurrentQueue<TSnapshot> _snapshotQueue = new();

        /// <summary>
        /// ?쇱씠?꾩궗?댄겢 ?숆린?붿슜 ?쎌엯?덈떎.
        /// </summary>
        private readonly object _lifecycleLock = new();

        /// <summary>
        /// ?쒕??덉씠??猷⑦봽 ?ㅻ젅?쒖엯?덈떎.
        /// </summary>
        private Thread _loopThread;

        /// <summary>
        /// ?ㅽ뻾 ?щ??낅땲??
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Dispose ?щ??낅땲??
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 怨좎젙 ?ㅽ뀦 ?쒓컙?낅땲??
        /// </summary>
        private float _fixedStep = 1f / 30f;

        /// <summary>
        /// ?깅떦 理쒕? ?ㅽ뀦 ?섏엯?덈떎.
        /// </summary>
        private int _maxStepsPerTick = 8;

        /// <summary>
        /// 猷⑦봽 ?湲??쒓컙(ms)?낅땲??
        /// </summary>
        private int _sleepMilliseconds = 1;

        /// <summary>
        /// 醫낅즺 ?湲??쒗븳(ms)?낅땲??
        /// </summary>
        private int _stopTimeoutMilliseconds = 5000;

        /// <summary>
        /// ?ㅻ깄???앹꽦 媛꾧꺽(珥??낅땲?? 0?대㈃ 留????앹꽦?⑸땲??
        /// </summary>
        private float _snapshotInterval = 0f;

        /// <summary>
        /// ?ㅻ깄???꾩쟻 ?쒓컙?낅땲??
        /// </summary>
        private double _snapshotAccumulator;

        /// <summary>
        /// 留덉?留됱쑝濡??앹꽦???ㅻ깄?룹엯?덈떎.
        /// </summary>
        private TSnapshot _latestSnapshot;

        /// <summary>
        /// 猷⑦봽 ?덉쇅 罹먯떆?낅땲??
        /// </summary>
        private Exception _loopException;

        /// <summary>
        /// ?꾩옱 ?몄뒪???깆엯?덈떎.
        /// </summary>
        public long Tick { get; private set; }

        /// <summary>
        /// ?ㅽ뻾 ?곹깭?낅땲??
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 猷⑦봽?먯꽌 諛쒖깮???덉쇅?낅땲??
        /// </summary>
        public Exception LoopException => _loopException;

        /// <summary>
        /// 怨좎젙 ?ㅽ뀦 ?쒓컙?낅땲??
        /// </summary>
        public float FixedStep
        {
            get => _fixedStep;
            set => _fixedStep = value > 0f ? value : 1f / 30f;
        }

        /// <summary>
        /// ?깅떦 理쒕? ?ㅽ뀦 ?섏엯?덈떎. Spiral of Death 諛⑹?瑜??꾪븳 ?쒗븳?낅땲??
        /// </summary>
        public int MaxStepsPerTick
        {
            get => _maxStepsPerTick;
            set => _maxStepsPerTick = value > 0 ? value : 1;
        }

        /// <summary>
        /// 猷⑦봽 ?湲??쒓컙(ms)?낅땲??
        /// </summary>
        public int SleepMilliseconds
        {
            get => _sleepMilliseconds;
            set => _sleepMilliseconds = value < 0 ? 0 : value;
        }

        /// <summary>
        /// 醫낅즺 ?湲??쒗븳(ms)?낅땲??
        /// </summary>
        public int StopTimeoutMilliseconds
        {
            get => _stopTimeoutMilliseconds;
            set => _stopTimeoutMilliseconds = value < 0 ? 5000 : value;
        }

        /// <summary>
        /// ?ㅻ깄???앹꽦 媛꾧꺽(珥??낅땲?? 0?대㈃ 留????앹꽦?⑸땲??
        /// </summary>
        public float SnapshotInterval
        {
            get => _snapshotInterval;
            set => _snapshotInterval = value < 0f ? 0f : value;
        }

        public event Action<TResult> ResultProduced;
        public event Action<TEvent> EventRaised;
        /// <summary>
        /// SendCommand 함수를 처리합니다.
        /// </summary>

        public void SendCommand(TCommand command)
        {
            // 핵심 로직을 처리합니다.
            if (command == null)
            {
                return;
            }

            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// 諛깃렇?쇱슫???ㅻ젅?쒖뿉???쒕??덉씠??猷⑦봽瑜??쒖옉?⑸땲??
        /// </summary>
        public void StartSimulation()
        {
            // 핵심 로직을 처리합니다.
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
        /// ?쒕??덉씠??猷⑦봽瑜?以묒??섍퀬 ?ㅻ젅??醫낅즺瑜??湲고빀?덈떎.
        /// </summary>
        public void StopSimulation()
        {
            // 핵심 로직을 처리합니다.
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

            // 理쒖떊 ?ㅻ깄?룹씠 ?덉쑝硫?諛섑솚?섍퀬, ?놁쑝硫?罹먯떆瑜?諛섑솚?⑸땲??
            if (latest != null)
            {
                return latest;
            }

            return Volatile.Read(ref _latestSnapshot);
        }

        /// <summary>
        /// 而ㅻ㎤?쒕? 泥섎━?섍퀬 寃곌낵/?대깽?몃? 諛섑솚?⑸땲??
        /// </summary>
        protected abstract GameCommandOutcome<TResult, TEvent> HandleCommand(TCommand command);

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Submit(TCommand command)
        {
            SendCommand(command);
        }

        void IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>.Advance(float deltaSeconds)
        {
            Tick++;

            // ?湲?以묒씤 而ㅻ㎤?쒕? 泥섎━?섍퀬 寃곌낵/?대깽?몃? ?먯뿉 ?깅줉?⑸땲??
            while (_pendingCommands.TryDequeue(out var command))
            {
                try
                {
                    // 而ㅻ㎤?쒖뿉 ????좏뻾 ?대깽?멸? ?덈떎硫?諛쒗뻾?⑸땲??
                    var outcome = HandleCommand(command);
                    if (outcome.PreEvents != null)
                    {
                        foreach (var preEvent in outcome.PreEvents)
                        {
                            PublishEvent(preEvent);
                        }
                    }

                    // 而ㅻ㎤?쒖뿉 ???寃곌낵瑜?諛쒗뻾?⑸땲??
                    if (outcome.Result != null)
                    {
                        PublishResult(outcome.Result);
                    }

                    // 而ㅻ㎤?쒖뿉 ????꾪뻾 ?대깽?멸? ?덈떎硫?諛쒗뻾?⑸땲??
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
        /// 留??ㅽ뀦 ?몄텧?섏뼱 寃뚯엫 ?곹깭瑜?媛깆떊?⑸땲??
        /// </summary>
        protected abstract void OnTick(float deltaSeconds);

        /// <summary>
        /// ?꾩옱 寃뚯엫 ?곹깭???ㅻ깄?룹쓣 ?앹꽦?⑸땲??猷⑦봽 ?ㅻ젅?쒖뿉???몄텧).
        /// </summary>
        protected abstract TSnapshot BuildSnapshotInternal();

        /// <summary>
        /// 而ㅻ㎤??泥섎━ ?ㅽ뙣 ??湲곕낯 ?먮윭 ?대깽?몃? ?앹꽦?⑸땲??
        /// 湲곕낯 援ы쁽? default(TEvent)瑜?諛섑솚?섎?濡? ?먮윭 ?대깽?몃? ?ㅼ젣濡?諛쒗뻾?섎젮硫?        /// ?뚯깮 ?대옒?ㅼ뿉????硫붿꽌?쒕? ?ъ젙?섑빐???⑸땲??
        /// </summary>
        protected virtual TEvent CreateErrorEvent(TCommand command, Exception exception)
        {
            // 핵심 로직을 처리합니다.
            return default;
        }
        /// <summary>
        /// PublishResult 함수를 처리합니다.
        /// </summary>

        protected void PublishResult(TResult result)
        {
            // 핵심 로직을 처리합니다.
            if (result == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForResult(result));
        }
        /// <summary>
        /// PublishEvent 함수를 처리합니다.
        /// </summary>

        protected void PublishEvent(TEvent eventData)
        {
            // 핵심 로직을 처리합니다.
            if (eventData == null)
            {
                return;
            }

            _dispatchQueue.Enqueue(DispatchItem.ForEvent(eventData));
        }

        /// <summary>
        /// 硫붿씤 ?ㅻ젅?쒖뿉??寃곌낵/?대깽?몃? ?붿뒪?⑥튂?⑸땲??
        /// Unity Update?먯꽌 ?몄텧?댁빞 ?⑸땲??
        /// </summary>
        public void FlushEvents()
        {
            // 핵심 로직을 처리합니다.
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
        /// 理쒖떊 ?ㅻ깄?룹쓣 諛섑솚?⑸땲??
        /// Host 猷⑦봽媛 理쒖떊 ?ㅻ깄??罹먯떆瑜?媛깆떊?⑸땲??
        /// </summary>
        public TSnapshot GetLatestSnapshot()
        {
            // 핵심 로직을 처리합니다.
            return Volatile.Read(ref _latestSnapshot);
        }
        /// <summary>
        /// TryBuildSnapshot 함수를 처리합니다.
        /// </summary>

        private void TryBuildSnapshot(float deltaSeconds)
        {
            // 핵심 로직을 처리합니다.
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
        /// EnqueueSnapshot 함수를 처리합니다.
        /// </summary>

        private void EnqueueSnapshot()
        {
            // 핵심 로직을 처리합니다.
            try
            {
                // ?꾩옱 ?곹깭瑜?蹂듭젣???ㅻ깄???앹꽦
                var snapshot = BuildSnapshotInternal();
                if (snapshot == null)
                {
                    return;
                }

                // ?먯뿉 ?ｌ뼱??硫붿씤 ?ㅻ젅?쒓? ?뚮퉬?????덇쾶 ??                _snapshotQueue.Enqueue(snapshot);

                // 理쒖떊 ?ㅻ깄?룹쓣 罹먯떆?????                Volatile.Write(ref _latestSnapshot, snapshot);
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
            /// ForResult 함수를 처리합니다.
            /// </summary>

            public static DispatchItem ForResult(TResult result)
            {
                // 핵심 로직을 처리합니다.
                return new DispatchItem(result, default, true);
            }
            /// <summary>
            /// ForEvent 함수를 처리합니다.
            /// </summary>

            public static DispatchItem ForEvent(TEvent eventData)
            {
                // 핵심 로직을 처리합니다.
                return new DispatchItem(default, eventData, false);
            }
        }
        /// <summary>
        /// RunLoop 함수를 처리합니다.
        /// </summary>

        private void RunLoop()
        {
            // 핵심 로직을 처리합니다.
            try
            {
                var stopwatch = Diagnostics.Stopwatch.StartNew();
                var lastTime = stopwatch.Elapsed;
                var accumulator = 0.0;
                var host = (IGameHostInternal<TCommand, TResult, TEvent, TSnapshot>)this;

                while (_isRunning)
                {
                    // ?꾩옱 ?쒓컙 湲곗? deltaTime 怨꾩궛
                    var now = stopwatch.Elapsed;
                    var deltaSeconds = (now - lastTime).TotalSeconds;
                    lastTime = now;

                    // ?덉젙???쒗븳 : ?덈Т ??delta瑜??섎씪????＜ 諛⑹?
                    // ???뺤? ??deltaSeconds媛 媛묒옄湲?而ㅼ???寃쎌슦
                    // ?쒕쾲???볦씤 留롮? Tick??泥섎━?섍쾶 ?섎뒗 ?댁뒋 諛⑹?
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

                    // 怨쇰룄??吏??蹂댁젙 (?꾩쟻 ?쒓컙 由ъ뀑)
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
        /// ThrowIfDisposed 함수를 처리합니다.
        /// </summary>

        protected void ThrowIfDisposed()
        {
            // 핵심 로직을 처리합니다.
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
        /// <summary>
        /// Dispose 함수를 처리합니다.
        /// </summary>

        public virtual void Dispose()
        {
            // 핵심 로직을 처리합니다.
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // ?ㅻ젅?쒓? ?먯뿉 ?묎렐?????덉쑝誘濡?癒쇱? 以묒??⑸땲??
            StopSimulation();

            while (_pendingCommands.TryDequeue(out _)) { }
            while (_dispatchQueue.TryDequeue(out _)) { }
            while (_snapshotQueue.TryDequeue(out _)) { }
        }
    }
}
