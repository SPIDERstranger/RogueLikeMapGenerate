using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RoomType{
    MainRoom,
    SideRoom,
    NoneRoom
}
public class RoomForCircle : MonoBehaviour
{

    public RoomType roomType = RoomType.NoneRoom;
    public Vector2 size;
    public RoomRigidBody rigidBody;
    public BoxCollider2D boxCollider;
	public SpriteRenderer sprite;
    private void Start()
    {
        if (boxCollider == null)
            boxCollider = this.gameObject.AddComponent<BoxCollider2D>();
        if (rigidBody == null)
            rigidBody = this.gameObject.AddComponent<RoomRigidBody>();
		sprite.size = size;
        boxCollider.size = size;
        rigidBody.size = size;
    }
}
