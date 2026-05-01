using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Tooltip("Glisse ton fichier son ici")]
    public AudioClip soundClip;

    public static float globalVolume = 0.1f;

    void Start()
    {
        Button btn = GetComponent<Button>();
        
        btn.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        if (soundClip != null)
        {
            AudioSource.PlayClipAtPoint(soundClip, Camera.main.transform.position, globalVolume);
        }
        else
        {
            Debug.LogWarning("Aucun son assigné sur le bouton " + gameObject.name);
        }
    }
}