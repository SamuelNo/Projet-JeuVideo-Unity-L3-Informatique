using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {
    public static MusicManager instance;
    private AudioSource audioSource;
    
    [Header("Configuration des musiques")]
    public string nomDeLaPremiereScene = "Menu_Scene";
    public AudioClip musiqueScene1;
    public AudioClip musiqueScene2;

    void Awake(){
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else{
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == nomDeLaPremiereScene) {
            SwitchTrack(musiqueScene1);
        }
        else {
            SwitchTrack(musiqueScene2);
        }
    }

    public void SwitchTrack(AudioClip newClip) {
        if(audioSource.clip == newClip)
            return;
            
        audioSource.clip = newClip;
        audioSource.Play();
    }
}