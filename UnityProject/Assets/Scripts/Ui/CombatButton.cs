using UnityEngine;
using UnityEngine.UI; 


public class CombatButton : MonoBehaviour
{
    private Button unityButton;
    public ButtonState currentState;

    // Cache the Unity UI Button component.
    void Awake()
    {
        unityButton = GetComponent<Button>();
    }

    // Update the button visibility and interactability from the given state.
    public void SetState(ButtonState newState)
    {
    currentState = newState;
    
    
    Image buttonImage = GetComponent<Image>();

        switch (newState)
        {
            case ButtonState.SHOWN:
                gameObject.SetActive(true);
                unityButton.interactable = true;
                buttonImage.color = Color.white; 
                break;

            case ButtonState.BLOCKED: 
                gameObject.SetActive(true);
                unityButton.interactable = false;
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.7f); 
                break;

            case ButtonState.HIDDEN:
                gameObject.SetActive(false);
                break;
        }
    }
}