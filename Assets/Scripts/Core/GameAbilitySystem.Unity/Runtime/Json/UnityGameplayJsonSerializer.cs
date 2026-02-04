using Noname.GameAbilitySystem.Json;
using UnityEngine;

namespace Noname.GameCore.Helper.Json
{
    /// <summary>
    /// Unity JsonUtility 기반 직렬화 구현.
    /// </summary>
    public sealed class UnityGameplayJsonSerializer : IGameplayJsonSerializer
    {
        public string Serialize<T>(T value)
        {
            return JsonUtility.ToJson(value, true);
        }

        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}

