using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 타겟팅에 필요한 월드 조회 함수를 제공하는 컨텍스트입니다.
    /// </summary>
    public sealed class TargetContext
    {
        public TargetContext(
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getEnemies,
            Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> getAllies,
            Func<AbilitySystemComponent, Point3D> getPosition,
            Random random = null)
        {
            GetEnemies = getEnemies;
            GetAllies = getAllies;
            GetPosition = getPosition;
            Random = random ?? new Random();
        }

        /// <summary>
        /// 적 목록 조회 함수입니다.
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetEnemies { get; }

        /// <summary>
        /// 아군 목록 조회 함수입니다.
        /// </summary>
        public Func<AbilitySystemComponent, IReadOnlyList<AbilitySystemComponent>> GetAllies { get; }

        /// <summary>
        /// 대상 위치 조회 함수입니다.
        /// </summary>
        public Func<AbilitySystemComponent, Point3D> GetPosition { get; }

        /// <summary>
        /// 랜덤 소스입니다.
        /// </summary>
        public Random Random { get; }

        public IReadOnlyList<AbilitySystemComponent> ResolveEnemies(AbilitySystemComponent owner)
        {
            return GetEnemies != null ? GetEnemies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public IReadOnlyList<AbilitySystemComponent> ResolveAllies(AbilitySystemComponent owner)
        {
            return GetAllies != null ? GetAllies(owner) : Array.Empty<AbilitySystemComponent>();
        }

        public Point3D ResolvePosition(AbilitySystemComponent owner)
        {
            return GetPosition != null ? GetPosition(owner) : default;
        }
    }
}
