using UnityEngine;

public class PlayerChangeColliderSize : MonoBehaviour
{
    public float width, height;
    public float xOffset, yOffset;

    private BoxCollider2D boxCollider;
    void Start()
    {
        boxCollider = transform.parent.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (boxCollider.size.x != width)
        {
            boxCollider.size = new Vector2(width, boxCollider.size.y);
        }
        if (boxCollider.size.y != height)
        {
            boxCollider.size = new Vector2(boxCollider.size.x, height);
        }
        if (boxCollider.offset.x != xOffset)
        {
            boxCollider.offset = new Vector2(xOffset, boxCollider.offset.y);
        }
        if (boxCollider.offset.y != yOffset)
        {
            boxCollider.offset = new Vector2(boxCollider.offset.x, yOffset);
        }
    }
}
