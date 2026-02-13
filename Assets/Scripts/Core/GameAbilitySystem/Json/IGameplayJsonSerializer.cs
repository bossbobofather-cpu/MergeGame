namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public interface IGameplayJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}

