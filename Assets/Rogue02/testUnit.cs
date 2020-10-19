using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testUnit : MonoBehaviour
{

    public ViewTriangle ViewTriangle;
    public static testUnit instance;
    // Use this for initialization

    List<ViewTriangle> trianglesListInView = new List<ViewTriangle>();
    Queue<List<Triangle>> pool = new Queue<List<Triangle>>();
    Queue<List<Edge>> Edges = new Queue<List<Edge>>();

    List<Triangle> triangles;
    List<Vector2> tempList;
    public void GetTriangle(List<Triangle> temp, List<Edge> set)
    {
        pool.Enqueue(temp);
        Edges.Enqueue(set);
    }
    void Start()
    {
        instance = this;
        tempList = new List<Vector2>();
        for (int i = 0; i < 10; i++)
            tempList.Add(RoomGenerationInCircle.getRandomPointInCircle(100, 1));
		// tempList.Add(new Vector2(-123,73));
		// tempList.Add(new Vector2(108,-63));
		// tempList.Add(new Vector2(-107,-46));
		// tempList.Add(new Vector2(-39,-8));
		// tempList.Add(new Vector2(-54,110));
        triangles = Polygon2D.DelaunayTriangulation(tempList);

        // foreach (var item in triangles)
        // {
        //     Debug.DrawLine(item.pointA, item.pointB);
        //     Debug.DrawLine(item.pointA, item.pointC);
        //     Debug.DrawLine(item.pointC, item.pointB);
        // }
        // StartCoroutine(Draw());
    }

    private void Update()
    {
        foreach (var item in tempList)
        {
            Debug.DrawLine(item + Vector2.up, item + Vector2.down);
            Debug.DrawLine(item + Vector2.left, item + Vector2.right);

        }
        foreach (var item in triangles)
        {
            Debug.DrawLine(item.pointA, item.pointB);
            Debug.DrawLine(item.pointA, item.pointC);
            Debug.DrawLine(item.pointC, item.pointB);
        }
		if(Input.GetKeyDown("space"))
		{
			tempList.RemoveRange(0,tempList.Count);
			       for (int i = 0; i < 10; i++)
            tempList.Add(RoomGenerationInCircle.getRandomPointInCircle(100, 1));
			        triangles = Polygon2D.DelaunayTriangulation(tempList);
		}
    }

    IEnumerator Draw()
    {
        List<Triangle> temp;
        List<Edge> e;
        do
        {
            DestoryAllTriangle();
            temp = pool.Dequeue();
            foreach (var item in temp)
            {
                var view = Instantiate(ViewTriangle, Vector2.zero, Quaternion.identity);
                trianglesListInView.Add(view);
                view.triangle = new Triangle(item);
                view.transform.parent = this.transform;
            }
            e = Edges.Dequeue();

            Debug.Log("draw " + temp.Count + " triangle");
            yield return new WaitForSeconds(1f);
            foreach (var item in e)
            {
                Debug.Log("edge :" + item.indexA + "   " + item.indexB);
                // Debug.DrawLine(tempList[item.indexA],tempList[item.indexB],Color.red,1f);
            }
        } while (pool.Count > 0);
    }

    public void DestoryAllTriangle()
    {
        for (int i = 0; i < trianglesListInView.Count; i++)
        {
            Destroy(trianglesListInView[i].gameObject);
        }
        trianglesListInView.Clear();
    }
}
