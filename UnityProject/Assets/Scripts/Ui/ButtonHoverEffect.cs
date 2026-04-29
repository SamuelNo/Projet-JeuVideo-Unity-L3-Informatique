using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 tailleInitiale;
    
    [Header("Réglages")]
    public float multiplicateurSurvol = 1.1f;
    public float multiplicateurClic = 0.95f;

    void Start()
    {
        tailleInitiale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = tailleInitiale * multiplicateurSurvol;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = tailleInitiale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = tailleInitiale * multiplicateurClic;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = tailleInitiale * multiplicateurSurvol;
    }
}