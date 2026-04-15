using UnityEngine;

public class SelectionData : MonoBehaviour {
    public static SelectionData Instance;
    public bool isPvP;
    public int[] team1 = new int[2]; 
    public int[] team2 = new int[2];

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        } else {
            Destroy(gameObject);
        }
    }
}