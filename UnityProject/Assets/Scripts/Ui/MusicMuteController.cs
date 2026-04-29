using UnityEngine;
using UnityEngine.UI; 

public class MusicMuteController : MonoBehaviour
{
    [Tooltip("Glisse ici ton bouton (pour modifier son composant Image)")]
    public Image buttonImage;

    [Tooltip("Couleur quand le son est allumé (Blanc = couleurs d'origine du sprite)")]
    public Color couleurAllume = Color.white;

    [Tooltip("Couleur quand le son est coupé (Gris = assombri)")]
    public Color couleurMute = Color.gray;

    void Start()
    {
   
        MettreAJourVisuel();
    }

    /// <summary>
    /// Fonction à assigner au OnClick() de ton bouton Mute
    /// </summary>
    public void ToggleMusicMute()
    {
        if (MusicManager.instance != null)
        {
            AudioSource musicSource = MusicManager.instance.GetComponent<AudioSource>();
            
            if (musicSource != null)
            {
  
                musicSource.mute = !musicSource.mute;

         
                MettreAJourVisuel();
            }
        }
    }

    /// <summary>
    /// Met à jour la couleur du bouton selon l'état du MusicManager
    /// </summary>
    private void MettreAJourVisuel()
    {
        if (MusicManager.instance != null && buttonImage != null)
        {
            AudioSource musicSource = MusicManager.instance.GetComponent<AudioSource>();
            
            if (musicSource.mute == true)
            {
               
                buttonImage.color = couleurMute;
            }
            else
            {
    
                buttonImage.color = couleurAllume;
            }
        }
    }
}