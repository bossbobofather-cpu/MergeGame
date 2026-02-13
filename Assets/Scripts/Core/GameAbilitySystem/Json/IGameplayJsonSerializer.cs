namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public interface IGameplayJsonSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}

