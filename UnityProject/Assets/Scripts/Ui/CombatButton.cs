using UnityEngine;
using UnityEngine.UI; 
/// <summary>
/// Manages the state of combat buttons in the battle UI, allowing them to be shown, blocked, or hidden based on the current game state.
/// </summary>

public class CombatButton : MonoBehaviour
{
    private Button unityButton;
    public ButtonState currentState;

    // Cache the Unity UI Button component.
    void Awake()
    {
        unityButton = GetComponent<Button>();
    }

    public void SetState(ButtonState newState)
    {
        ///<summary> updates the button's state to the given new state, changing its visibility and interactability accordingly </summary>
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