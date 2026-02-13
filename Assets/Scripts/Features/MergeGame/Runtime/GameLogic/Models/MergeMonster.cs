using System;
using MyProject.MergeGame.AI;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Models
{
    /// <summary>
    /// 경로를 따라 이동하는 몬스터입니다.
    /// </summary>
    public sealed class MergeMonster : IAbilitySystemOwner, IDisposable
    {
        /// <summary>
        /// 몬스터 고유 ID입니다.
        /// </summary>
        public long Uid { get; }

        /// <summary>
        /// 몬스터 정의 ID입니다.
        /// </summary>
        public long MonsterId { get; }

        /// <summary>
        /// 이동 경로 인덱스입니다 (복수 경로 지원).
        /// </summary>
        public int PathIndex { get; set; }

        /// <summary>
        /// 경로 진행도입니다 (0.0 = 시작, 1.0 = 도착).
        /// </summary>
        public float PathProgress { get; set; }

        /// <summary>
        /// 현재 위치입니다.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// AbilitySystemComponent입니다.
        /// </summary>
        public AbilitySystemComponent ASC { get; }

        /// <summary>
        /// 몬스터 AI입니다.
        /// </summary>
        public IMergeMonsterAI AI { get; private set; }

        /// <summary>
        /// 목적지 도달 시 플레이어에게 주는 데미지입니다.
        /// </summary>
        public int DamageToPlayer { get; }

        /// <summary>
        /// 처치 시 획득 골드입니다.
        /// </summary>
        public int GoldReward { get; }

        /// <summary>
        /// 상대 플레이어에게서 주입된 몬스터인지 여부입니다.
        /// </summary>
        public bool IsInjectedByOpponent { get; }

        /// <summary>
        /// 생존 여부입니다.
        /// </summary>
        public bool IsAlive => ASC.Get(AttributeId.Health) > 0;

        /// <summary>
        /// 목적지 도달 여부입니다.
        /// </summary>
        public bool ReachedGoal => PathProgress >= 1f;

        public MergeMonster(
            long uid,
            long monsterId,
            int pathIndex,
            Point3D startPosition,
            int damageToPlayer,
            int goldReward,
            bool isInjectedByOpponent = false)
        {
            Uid = uid;
            MonsterId = monsterId;
            PathIndex = pathIndex;
            PathProgress = 0f;
            Position = startPosition;
            DamageToPlayer = damageToPlayer;
            GoldReward = goldReward;
            IsInjectedByOpponent = isInjectedByOpponent;

            ASC = new AbilitySystemComponent();
            ASC.SetOwner(this);
        }

        /// <summary>
        /// AI를 지정합니다.
        /// </summary>
        public void SetAI(IMergeMonsterAI ai)
        {
            AI = ai;
        }

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        public void TakeDamage(float damage)
        {
            ASC.Add(AttributeId.Health, -damage);
        }
        /// <summary>
        /// Dispose 메서드입니다.
        /// </summary>

        public void Dispose()
        {
            ASC?.Dispose();
        }
    }
}

