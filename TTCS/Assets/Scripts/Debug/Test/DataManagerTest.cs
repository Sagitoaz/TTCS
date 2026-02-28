using UnityEngine;
using TTCS.Core.Data;

public class DataManagerTest : MonoBehaviour
{
    private void Start()
    {
        var warrior = DataManager.Instance.LoadCharacter("char_warrior");
        if (warrior != null)
            Debug.Log($"Loaded: {warrior.nameKey} | HP: {warrior.baseStats.hp}");
        else
            Debug.LogError("FAILED to load char_warrior!");
    }
}