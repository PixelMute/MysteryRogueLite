using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HideInFog : MonoBehaviour
{
    private SpriteRenderer Sprite;
    // Start is called before the first frame update
    void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
    }

    public void Hide()
    {
        if (Sprite != null)
        {
            Sprite.enabled = false;
        }
    }

    public void Show()
    {
        if (Sprite != null)
        {
            Sprite.enabled = true;
        }
    }
}
