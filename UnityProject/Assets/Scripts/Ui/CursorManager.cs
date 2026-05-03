using UnityEngine;
/// <summary>
/// Manages the custom cursor across scenes, ensuring it persists and is set correctly on game start.
/// </summary>
public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
    }
}