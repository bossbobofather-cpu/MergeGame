namespace MyProject.MergeGame
{
    /// <summary>
    /// 타워 정의 데이터 소스(하드코딩/SO/JSON/서버 등) 추상화 인터페이스입니다.
    /// Host는 데이터 저장소를 모르고 이 인터페이스로만 접근합니다.
    /// </summary>
    public interface ITowerDatabase
    {
        /// <summary>
        /// 타워 ID로 정의 데이터를 조회합니다.
        /// </summary>
        TowerDefinition GetDefinition(long towerId);

        /// <summary>
        /// 특정 등급에 대해 스폰/머지 결과로 사용할 타워 ID를 선택합니다.
        /// </summary>
        long GetRandomIdForGrade(int grade);
    }
}
