using System.Collections.Generic;

namespace MyProject.MergeGame.Modules
{
    /// <summary>
    /// 웨이브 정보입니다.
    /// </summary>
    public sealed class WaveInfo
    {
        /// <summary>
        /// 웨이브 번호입니다.
        /// </summary>
        public int WaveNumber { get; set; }

        /// <summary>
        /// 스폰할 몬스터 ID 목록입니다.
        /// </summary>
        public List<string> MonsterIds { get; set; } = new();

        /// <summary>
        /// 몬스터 스폰 간격 (초)입니다.
        /// </summary>
        public float SpawnInterval { get; set; } = 1f;

        /// <summary>
        /// 사용할 경로 인덱스입니다.
        /// </summary>
        public int PathIndex { get; set; }

        /// <summary>
        /// 웨이브 시작 대기 시간 (초)입니다.
        /// </summary>
        public float StartDelay { get; set; }
    }

    /// <summary>
    /// WaveModule 설정입니다.
    /// </summary>
    public sealed class WaveModuleConfig
    {
        /// <summary>
        /// 웨이브 시작 전 대기 시간 (초)입니다.
        /// </summary>
        public float WaveStartDelay { get; set; } = 3f;

        /// <summary>
        /// 웨이브 간 대기 시간 (초)입니다.
        /// </summary>
        public float WaveIntervalDelay { get; set; } = 5f;

        /// <summary>
        /// 기본 몬스터 스폰 간격 (초)입니다.
        /// </summary>
        public float DefaultSpawnInterval { get; set; } = 1f;

        /// <summary>
        /// 자동 웨이브 시작 여부입니다.
        /// </summary>
        public bool AutoStartWaves { get; set; } = true;

        /// <summary>
        /// 최대 웨이브 수입니다. (0 = 무한)
        /// </summary>
        public int MaxWaveCount { get; set; } = 10;

        /// <summary>
        /// 웨이브당 기본 몬스터 수입니다.
        /// </summary>
        public int BaseMonsterCount { get; set; } = 5;

        /// <summary>
        /// 웨이브당 추가 몬스터 수입니다.
        /// </summary>
        public int MonstersPerWaveIncrease { get; set; } = 2;

        /// <summary>
        /// 사전 정의된 웨이브 목록입니다.
        /// </summary>
        public List<WaveInfo> PredefinedWaves { get; set; } = new();
    }
}
