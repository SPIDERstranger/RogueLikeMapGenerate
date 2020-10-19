using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class RoomRigidBody : MonoBehaviour
{
    public bool Simualeted = true;
    public Vector2 size;
    public float stopTime = 3f;
    private float stopTimer = 0;
    BoxCollider2D myCollider;
    public Action onSimualtedFinishCallback;
    
    // Use this for initialization
    void Start()
    {
        myCollider = this.GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        Simualeting();
    }

    // BUG 性能消耗过大。
    private void Simualeting()
    {
        if (!Simualeted)
            return;
        // 这里减去0.5是为了防止检测到其他房间的边缘
        Collider2D[] colArray = Physics2D.OverlapBoxAll(this.transform.position, size - new Vector2(0.5f,0.5f), 0);
        if(colArray==null||colArray.Length ==1)
        stopTimer+=Time.fixedDeltaTime;
        else{
            stopTimer = 0;
        }
        if(stopTimer>stopTime)
        {
            Simualeted = false;
            if(onSimualtedFinishCallback!=null)
                onSimualtedFinishCallback.Invoke();
        }
        for (int i = 0; i < colArray.Length; i++)
        {
            if (colArray[i] == myCollider)
                continue;
            CollideWithOneCollider((BoxCollider2D)colArray[i]);
            break;
        }
    }

    public void CollideWithOneCollider(BoxCollider2D Col)
    {
        float disX = transform.position.x - Col.transform.position.x;
        short dirX = (short)((disX > 0) ? 1 : -1);
        disX = Mathf.Abs(disX);
        float needX = Col.size.x / 2 + myCollider.size.x / 2;
        float moveX = (needX - disX);

        float disY = transform.position.y - Col.transform.position.y;
        short dirY = (short)((disY > 0) ? 1 : -1);
        disY = Mathf.Abs(disY);
        float needY = Col.size.y / 2 + myCollider.size.y / 2;
        float moveY = (needY - disY);

        if (moveX <= moveY)
        {
            //Debug.Log("move in X" + moveX * dirX);
            this.transform.position += Vector3.right * moveX * dirX;
        }
        else
        {
            // Debug.Log("move in Y" + moveY * dirY);
            this.transform.position += Vector3.up * moveY * dirY;
        }
    }
}




