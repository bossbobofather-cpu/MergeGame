namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// 게임 데이터 JSON 직렬화 인터페이스.
    /// </summary>
    public interface IGameplayJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}

