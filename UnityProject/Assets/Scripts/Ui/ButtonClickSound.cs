using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Plays a sound effect when a UI button is clicked, providing auditory feedback to the player.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Tooltip("Glissez un son fichier son ici")]
    public AudioClip soundClip;

    public static float globalVolume = 1f;

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