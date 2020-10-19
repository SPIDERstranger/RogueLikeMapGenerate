using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Polygon2D
{

    public static List<Triangle> DelaunayTriangulation(List<Vector2> points)
    {
        List<Vector2> pointList = new List<Vector2>();
        pointList.AddRange(points);
        // 索引 列表顺序的索引
        List<int> indices = new List<int>(pointList.Count);
        // 缓存三角形列表，内部的三角形是不确定的三角形
        List<Triangle> tempTriList = new List<Triangle>();
        // Delaunay三角形列表
        List<Triangle> DelaunayTriList = new List<Triangle>();
        // 获取最大值
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        for (int i = 0; i < pointList.Count; i++)
        {
            max.x = max.x > pointList[i].x ? max.x : pointList[i].x;
            max.y = max.y > pointList[i].y ? max.y : pointList[i].y;
            min.x = min.x < pointList[i].x ? min.x : pointList[i].x;
            min.y = min.y < pointList[i].y ? min.y : pointList[i].y;
            indices.Add(i);
        }

        // 构建超级三角形
        Triangle superTriangle = GenerateSuperTriangle(pointList, max, min);
        tempTriList.Add(superTriangle);
        // DelaunayTriList.Add(superTriangle);
        // 对pointlist 的x 进行排序，但是将排序顺序存储在indices列表中作为索引。
        indices.Sort((left, right) =>
        {
            if (pointList[left].x > pointList[right].x)
                return 1;
            else if (pointList[left].x == pointList[right].x)
                return 0;
            else
                return -1;
        });

        // 遍历所有的点
        for (int j = 0; j < indices.Count; j++)
        {
            int index = indices[j];

            var point = pointList[index];
            // Debug.Log("NowPoint:" + index);
            // 边的缓存列表
            HashSet<Edge> edgeBuffer = new HashSet<Edge>();
            // 缓存列表中需要删除的列表
            List<Edge> edgeRemove = new List<Edge>();
            for (int i = 0; i < tempTriList.Count; i++)
            {
                // Debug.Log(tempTriList[i]);
                var triangle = tempTriList[i];
                // 如果点在右侧 即所有点都在右侧，故当前三角形是Delaunay三角形
                if (point.x > triangle.CirclePoint.x + triangle.radius)
                {
                    // Debug.Log(triangle + " is Delaunay");
                    DelaunayTriList.Add(triangle);
                    tempTriList.Remove(triangle);
                    //移除后count发生变化，这时候需要将i跟着变化
                    i--;
                    continue;
                }
                // 如果点在外部，但是不在右侧，有可能出现别的点在内部,不做任何处理
                else if (Vector2.Distance(point, triangle.CirclePoint) > triangle.radius)
                {
                    continue;
                }
                // 如果点在内部
                else if (Vector2.Distance(point, triangle.CirclePoint) <= triangle.radius)
                {
                    foreach (var e in triangle.GetEdges())
                    {
                        // Debug.Log(e);
                        if (!edgeBuffer.Add(e))
                        {
                            // Debug.Log("Contians " + e);
                            edgeRemove.Add(e);
                        }
                    }
                    tempTriList.Remove(triangle);
                    //移除后count发生变化，这时候需要将i跟着变化
                    i--;
                }
            }
            // 将重复的部分进行删除
            foreach (Edge e in edgeRemove)
            {
                // Debug.Log("EdgeBuffer contains :" + e.ToString() + edgeBuffer.Contains(e));
                edgeBuffer.Remove(e);
            }
            // 将当前点和缓冲边进行组合，生成新的三角形
            foreach (Edge e in edgeBuffer)
            {
                // Debug.Log("Gen Triangle " + index + "," + e.indexA + "," + e.indexB);
                tempTriList.Add(new Triangle(pointList, index, e.indexA, e.indexB));
            }
            // // todo 删除
            // List<Triangle> temp = new List<Triangle>();
            // temp.AddRange(tempTriList);
            // temp.AddRange(DelaunayTriList);
            // List<Edge> tempEdge = new List<Edge>();
            // tempEdge.AddRange(edgeBuffer);
            // testUnit.instance.GetTriangle(temp, tempEdge);
            // // todo
            // Debug.Log("Clear");
            edgeRemove.Clear();
            edgeBuffer.Clear();
        }
        // 合并所有的三角形
        DelaunayTriList.AddRange(tempTriList);
        //将超级三角形相关的三角形进行删除
        for (int i = DelaunayTriList.Count - 1; i >= 0; i--)
        {
            var d = DelaunayTriList[i];
            if (d.indexA >= points.Count)
            {
                DelaunayTriList.Remove(d);
                continue;
            }
            if (d.indexB >= points.Count)
            {
                DelaunayTriList.Remove(d);
                continue;
            }
            if (d.indexC >= points.Count)
            {
                DelaunayTriList.Remove(d);
                continue;
            }
        }
        //返回所有的边
        return DelaunayTriList;
    }

    private static Triangle GenerateSuperTriangle(List<Vector2> pointList, Vector2 max, Vector2 min)
    {
        // 构建超级三角形，即括住所有的点的三角形。   //用max min 生成四边形  //四边形的外接圆
        // 获取三角形 三角形内切圆是四边形的外接圆。 //通过证明正三角形的内切圆和外接圆的半径比为1：2 且圆心相同
        Vector2 superPoint = new Vector2((max.x + min.x) / 2, (max.y + min.y) / 2);
        float superRadius = 2 * Vector2.Distance(superPoint, new Vector2(max.x, max.y));
        // 构建出三个点
        int indexSuperA = pointList.Count;
        pointList.Add(new Vector2(superPoint.x, superPoint.y + superRadius));
        int indexSuperB = pointList.Count;
        pointList.Add(new Vector2(superPoint.x - Mathf.Sqrt(3) * superRadius, superPoint.y - superRadius / 2));
        int indexSuperC = pointList.Count;
        pointList.Add(new Vector2(superPoint.x + Mathf.Sqrt(3) * superRadius, superPoint.y - superRadius / 2));
        return new Triangle(pointList, indexSuperA, indexSuperB, indexSuperC);
    }

}

[Serializable]
public struct Edge
{
    public int indexA;
    public int indexB;
    public Edge(int a, int b)
    {
        if (a <= b)
        {
            indexA = a;
            indexB = b;
        }
        else
        {
            indexA = b;
            indexB = a;
        }
    }

    public override string ToString()
    {
        return "Edge(" + indexA + "," + indexB + ")";
    }
    public override bool Equals(object obj)
    {
        Edge temp = (Edge)obj;
        bool flag = temp.indexA == this.indexA && temp.indexB == this.indexB;
        return flag;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
public class Triangle
{
    public Vector2 pointA { private set; get; }
    public Vector2 pointB { private set; get; }
    public Vector2 pointC { private set; get; }

    public readonly int indexA = -1;
    public readonly int indexB = -1;
    public readonly int indexC = -1;
    public float radius { private set; get; }

    public Vector2 CirclePoint { private set; get; }

    private Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        pointA = a;
        pointB = b;
        pointC = c;

        // 计算圆心半径
        var a1 = 2 * (b.x - a.x);
        var b1 = 2 * (b.y - a.y);
        var c1 = b.x * b.x + b.y * b.y - a.x * a.x - a.y * a.y;
        var a2 = 2 * (c.x - b.x);
        var b2 = 2 * (c.y - b.y);
        var c2 = c.x * c.x + c.y * c.y - b.x * b.x - b.y * b.y;

        CirclePoint = new Vector2(((c1 * b2) - (c2 * b1)) / ((a1 * b2) - (a2 * b1)), ((a1 * c2) - (a2 * c1)) / ((a1 * b2) - (a2 * b1)));
        radius = Vector2.Distance(CirclePoint, pointA);
    }

    public Triangle(List<Vector2> pointList, int indexA, int indexB, int indexC)
    : this(pointList[indexA], pointList[indexB], pointList[indexC])
    {
        this.indexA = indexA;
        this.indexB = indexB;
        this.indexC = indexC;
    }

    public Triangle(Triangle a)
    {
        pointA = a.pointA;
        pointB = a.pointB;
        pointC = a.pointC;
        indexA = a.indexA;
        indexB = a.indexB;
        indexC = a.indexC;
        radius = a.radius;
        CirclePoint = a.CirclePoint;
    }
    public Edge[] GetEdges()
    {
        Edge[] edges = new Edge[3];
        edges[0] = new Edge(indexA, indexB);
        edges[1] = new Edge(indexA, indexC);
        edges[2] = new Edge(indexB, indexC);
        return edges;
    }

    public override string ToString()
    {
        return "Triangle(" + indexA + "," + indexB + "," + indexC + ")";
    }

}
public static class PolygonHelper
{

    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> edges)
    {
        foreach (T e in edges)
        {
            set.Add(e);
        }
    }
    public static List<Edge> GetEdges(this List<Triangle> triangles)
    {
        HashSet<Edge> result = new HashSet<Edge>();
        foreach (var t in triangles)
        {
            result.AddRange(t.GetEdges());
        }
        return new List<Edge>(result);
    }

}