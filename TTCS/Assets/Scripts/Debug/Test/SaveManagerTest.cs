using UnityEngine;
using TTCS.Core.Save;

public class SaveManagerTest : MonoBehaviour
{
    private void Start()
    {
        // Test tạo save mới
        SaveManager.Instance.NewGame();
        SaveManager.Instance.CurrentSave.gold = 9999;
        SaveManager.Instance.Save(0);

        // Test load lại
        var loaded = SaveManager.Instance.Load(0);
        if (loaded != null)
            Debug.Log($"Save OK! Gold: {loaded.gold}"); // Phải ra 9999
        else
            Debug.LogError("FAILED to load save!");
    }
}