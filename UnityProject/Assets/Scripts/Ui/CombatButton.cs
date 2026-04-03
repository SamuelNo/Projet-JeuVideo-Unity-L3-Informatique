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

    switch (newState)
    {
        case ButtonState.SHOWN:
            gameObject.SetActive(true);
            unityButton.interactable = true;
            break;

        case ButtonState.BLOCKED: 
            gameObject.SetActive(true);
            unityButton.interactable = false;
            break;

        case ButtonState.HIDDEN:
            gameObject.SetActive(false);
            break;
    }
}
}