namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public interface IGameplayJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}

