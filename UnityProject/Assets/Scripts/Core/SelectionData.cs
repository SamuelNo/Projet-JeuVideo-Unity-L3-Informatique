using UnityEngine;

/// <summary>
/// Stores selection data across scenes using a singleton instance.
/// </summary>
public class SelectionData : MonoBehaviour {
    public static SelectionData Instance;
    public bool isPvP;
    public int[] team1 = new int[2]; 
    public int[] team2 = new int[2];
    public int selectedStage;
    public int unlockedStage;

    /// <summary>
    /// Keeps a single persistent selection container alive across scene loads.
    /// </summary>
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        } else {
            Destroy(gameObject);
        }
    }
}