using System;
using System.Collections.Generic;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 내부 상태를 관리합니다.
    /// </summary>
    public sealed class MergeHostState : IDisposable
    {
        /// <summary>
        /// 슬롯 정보입니다.
        /// </summary>
        public sealed class SlotInfo
        {
            /// <summary>
            /// 슬롯 인덱스입니다.
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// 유닛 UID입니다. 비어있으면 0입니다.
            /// </summary>
            public long UnitUid { get; set; }

            /// <summary>
            /// 유닛 등급입니다.
            /// </summary>
            public int UnitGrade { get; set; }

            /// <summary>
            /// 슬롯이 비어있는지 여부입니다.
            /// </summary>
            public bool IsEmpty => UnitUid == 0;

            public SlotInfo(int index)
            {
                Index = index;
                UnitUid = 0;
                UnitGrade = 0;
            }

            public void Clear()
            {
                UnitUid = 0;
                UnitGrade = 0;
            }

            public void SetUnit(long unitUid, int grade)
            {
                UnitUid = unitUid;
                UnitGrade = grade;
            }
        }

        private MergeSessionPhase _sessionPhase = MergeSessionPhase.None;
        private readonly List<SlotInfo> _slots = new();
        private int _score;
        private int _maxGrade;
        private float _elapsedTime;
        private long _nextUnitUid = 1;

        /// <summary>
        /// 현재 세션 단계입니다.
        /// </summary>
        public MergeSessionPhase SessionPhase => _sessionPhase;

        /// <summary>
        /// 슬롯 목록입니다.
        /// </summary>
        public IReadOnlyList<SlotInfo> Slots => _slots;

        /// <summary>
        /// 현재 점수입니다.
        /// </summary>
        public int Score => _score;

        /// <summary>
        /// 도달한 최고 등급입니다.
        /// </summary>
        public int MaxGrade => _maxGrade;

        /// <summary>
        /// 경과 시간(초)입니다.
        /// </summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>
        /// 사용 중인 슬롯 개수입니다.
        /// </summary>
        public int UsedSlotCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _slots.Count; i++)
                {
                    if (!_slots[i].IsEmpty)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// 슬롯을 초기화합니다.
        /// </summary>
        public void InitializeSlots(int slotCount)
        {
            _slots.Clear();
            for (var i = 0; i < slotCount; i++)
            {
                _slots.Add(new SlotInfo(i));
            }
        }

        /// <summary>
        /// 세션 단계를 설정합니다.
        /// </summary>
        public void SetSessionPhase(MergeSessionPhase phase)
        {
            _sessionPhase = phase;
        }

        /// <summary>
        /// 점수를 추가합니다.
        /// </summary>
        public void AddScore(int amount)
        {
            _score += amount;
        }

        /// <summary>
        /// 최고 등급을 갱신합니다.
        /// </summary>
        public void UpdateMaxGrade(int grade)
        {
            if (grade > _maxGrade)
            {
                _maxGrade = grade;
            }
        }

        /// <summary>
        /// 경과 시간을 갱신합니다.
        /// </summary>
        public void AddElapsedTime(float deltaTime)
        {
            _elapsedTime += deltaTime;
        }

        /// <summary>
        /// 새 유닛 UID를 생성합니다.
        /// </summary>
        public long GenerateUnitUid()
        {
            return _nextUnitUid++;
        }

        /// <summary>
        /// 빈 슬롯 인덱스를 반환합니다. 없으면 -1입니다.
        /// </summary>
        public int FindEmptySlotIndex()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 슬롯 정보를 가져옵니다.
        /// </summary>
        public SlotInfo GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return null;
            }
            return _slots[index];
        }

        /// <summary>
        /// 상태를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            _sessionPhase = MergeSessionPhase.None;
            _slots.Clear();
            _score = 0;
            _maxGrade = 0;
            _elapsedTime = 0;
            _nextUnitUid = 1;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}
