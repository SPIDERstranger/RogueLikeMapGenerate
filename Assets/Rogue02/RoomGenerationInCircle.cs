using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
public class RoomGenerationInCircle : MonoBehaviour
{
    #region 随机房间相关的属性
    [Header("===== 随机房间属性 =====")]
    public int seed;
    private int lastSeed;

    [Range(30, 500)]
    public int RoomCount = 20;
    public float GenerateRadius = 10f;
    public float RoomRadius = 3f;
    public float mainRoomThreshold = 1.35f;
    public float maxWaitTime = 3f;
    public Sprite testSprtie;
    [Space]
    #endregion
    #region 绘制tilemap的属性
    [Header("===== 绘制tile的相关资源 =====")]
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public Tile wallTile;
    public Tile floorTile;
    #endregion
    #region 存储房间以及边的列表
    private List<RoomForCircle> roomList = new List<RoomForCircle>();
    private List<RoomForCircle> mainRoomList = new List<RoomForCircle>();
    private List<Edge> roadList = new List<Edge>();
    #endregion
    #region delegate相关属性
    // todo 使用代理让提高可配置性。
    public delegate void DrawRoom(Vector2 centerPos, Vector2 size);
    public delegate void DrawRoad(Vector2 turnPoint, Vector2 pointA, Vector2 pointB);
    public DrawRoom DrawMainRoom;
    public DrawRoom DrawSideRoom;
    public DrawRoad DrawHallway;
    #endregion
    private bool isGenerating = false;
    private void Start()
    {
        StartCoroutine(GenerationRooms());
    }
    private void Update()
    {
        if (!isGenerating && Input.anyKeyDown)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprite/UnityWatermarktrial") as Sprite;
            DesotryAllRoom();
            StartCoroutine(GenerationRooms());
        }
    }

    public void DesotryAllRoom()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }
        mainRoomList.Clear();
        roomList.Clear();
    }


    // todo 尝试使用异步完成
    public IEnumerator GenerationRooms()
    {
        UnityEngine.Random.InitState(seed);
        int currentFinishCount = 0;
        float roomAverage = 0;
        //设置当前状态
        isGenerating = true;
        for (int i = 0; i < RoomCount; i++)
        {
            // todo 将tilesize设定成参数。
            // 生成房间初始位置
            Vector2 pos = getRandomPointInEllipse(GenerateRadius, GenerateRadius, 1);
            // Debug.Log("pos : " + pos);
            // 房间大小可以使用随机也可以使用预制的房间
            Vector2 size = getRandomSizeInCircle(RoomRadius, 2);
            // Debug.Log("size : " + size);
            roomAverage += size.Sum();
            // 根据大小和初始位置生成房间
            RoomForCircle temp = GenerationRoom(this.transform, pos, size, testSprtie);
            roomList.Add(temp);
            temp.transform.localScale = new Vector3(1, 1, 1);
            // 设置物体碰撞完成时的callback回调
            temp.rigidBody.onSimualtedFinishCallback += () => { currentFinishCount++; };
            //延迟
            yield return new WaitForFixedUpdate();
        }
        // todo 可以自行调用来完成碰撞，减少随机性。可以通过随机来控制
        float timer = 0;
        while (timer < maxWaitTime && currentFinishCount < RoomCount)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }
        // 获取主要房间
        roomAverage /= roomList.Count;
        mainRoomList = GetMainRoom(roomList, roomAverage * mainRoomThreshold);
        foreach (var room in mainRoomList)
        {
            // todo 可定制的主房间，使用代理
            DrawRoomInTilemap(floorTilemap, wallTilemap, floorTile, wallTile, room.transform.position.ToVector2(), room.size);
        }
        // 通过三角剖分获取主要房间之间的联通路
        List<Vector2> mainRoomPos = mainRoomList.ToPosition();
        List<Triangle> triangles = Polygon2D.DelaunayTriangulation(mainRoomPos);
        roadList = triangles.GetEdges();
        foreach (var r in roadList)
        {
            Debug.DrawLine(mainRoomPos[r.indexA], mainRoomPos[r.indexB], Color.green, 1f);
        }
        // 使用最小生成树算法生成真实路径，
        List<Edge> mainRoadList = GetMainRoad(roadList, mainRoomPos);

        foreach (var r in mainRoadList)
        {
            Debug.DrawLine(mainRoomPos[r.indexA], mainRoomPos[r.indexB], Color.red, 1f);
        }
        // 算法思路：
        // 将两个房间确定一个矩形范围，在范围中确定一个点，用作折现点。
        foreach (var road in mainRoadList)
        {
            RoomForCircle roomA = mainRoomList[road.indexA];
            RoomForCircle roomB = mainRoomList[road.indexB];
            // todo 可定制的path 使用代理
            // 获取房间之间的路径点, 获取的路径点头和尾分别是房间A和房间B的门口
            Vector2[] PathPoints = GetPathsBetweenRoom(roomA, roomB);
            GameObject pA = new GameObject("point0" + roomA.GetHashCode());
            pA.transform.parent = this.gameObject.transform;
            pA.transform.position = PathPoints[0];
            for (int i = 0; i < PathPoints.Length - 1; i++)
            {
                Debug.DrawLine(PathPoints[i] + new Vector2(0.5f, 0.5f), PathPoints[i + 1] + new Vector2(0.5f, 0.5f), Color.white, 1f);
                GameObject temp = new GameObject("point" + i);
                temp.transform.parent = pA.transform;
                temp.transform.position = PathPoints[i + 1] + new Vector2(0.5f, 0.5f);
            }

            // 对连线之间的房间进行选取，映射到tilemap中
            // todo 可选的判断方法，间隔一个距离使用raycast 进行判断，减少性能消耗。
            // BUG 房间内有概率出现走廊。
            // BUG side房间的判断有时候有问题
            for (int i = 0; i < PathPoints.Length - 1; i++)
            {
                RaycastHit2D[] result = new RaycastHit2D[mainRoadList.Count];// todo 这里的数量可以动态调整
                int n = Physics2D.RaycastNonAlloc(
                    PathPoints[i] + new Vector2(0.5f, 0.5f),
                    (PathPoints[i + 1] - PathPoints[i]).normalized,
                    result, Vector2.Distance(PathPoints[i + 1], PathPoints[i]));
                for (int j = 0; j < n; j++)
                {
                    RaycastHit2D temp = result[j];
                    RoomForCircle room = temp.collider.gameObject.GetComponent<RoomForCircle>();
                    if (room.roomType == RoomType.NoneRoom)
                    {
                        room.roomType = RoomType.SideRoom;
                        room.sprite.color = Color.red;
                        // todo 可定制的side房间，使用代理 
                        DrawRoomInTilemap(floorTilemap, wallTilemap, floorTile, wallTile, room.transform.position, room.size);
                    }
                }
            }
            DrawHallwayInTilemap(floorTilemap, wallTilemap, floorTile, wallTile, PathPoints);
        }



        isGenerating = false;
        this.gameObject.SetActive(false);
    }

    #region 随机相关的代码
    public static Vector2 getRandomPointInEllipse(float ellipseX, float ellipseY, float tileSize = 0)
    {
        float angle = 2 * Mathf.PI * UnityEngine.Random.Range(0, 1.0f);
        float len = UnityEngine.Random.Range(0, 1.0f) + UnityEngine.Random.Range(0, 1.0f);
        len = len > 2 ? 2 - len : len;
        if (tileSize != 0)
            return new Vector2(RoundDm(len * ellipseX * Mathf.Cos(angle), tileSize), RoundDm(len * ellipseY * Mathf.Sin(angle), tileSize));
        else
            return new Vector2(len * ellipseX * Mathf.Cos(angle), len * ellipseY * Mathf.Sin(angle));
    }
    public static Vector2 getRandomPointInCircle(float circleRadius, float tileSize = 0)
    {
        return getRandomPointInEllipse(circleRadius, circleRadius, tileSize);
    }

    //房间大小生成算法，
    // BUG 生成算法有点问题，常常出现长条以及小方块
    public static Vector2 getRandomSizeInCircle(float circleRadius, float tileSize)
    {
        return getRandomSizeInEllipse(circleRadius, circleRadius, tileSize);
    }

    public static Vector2 getRandomSizeInEllipse(float ellipseX, float ellipseY, float tileSize)
    {
        Vector2 size = getRandomPointInEllipse(ellipseX, ellipseY, tileSize);
        size.x = Mathf.Clamp(RoundDm(Mathf.Abs(size.x), tileSize), tileSize * 2, float.MaxValue);
        size.y = Mathf.Clamp(RoundDm(Mathf.Abs(size.y), tileSize), tileSize * 2, float.MaxValue);
        return size;
    }

    public static Vector2 getRandomPointInBox(Vector2 size, Vector2 center)
    {
        int x = UnityEngine.Random.Range(0, (int)size.x);
        int y = UnityEngine.Random.Range(0, (int)size.y);
        return new Vector2(x + center.x - size.x / 2, y + center.y - size.y / 2);
    }
    public static float RoundDm(float value, float tileSize)
    {
        return Mathf.Floor((value + tileSize) / tileSize) * tileSize;
    }
    #endregion

    #region 房间和通路相关的代码
    // 根据长和宽找到房间大小比较大的 ，
    // todo 需要限定宽高比，否则将会选择长条状
    // todo random 使用seed；
    private static List<RoomForCircle> GetMainRoom(List<RoomForCircle> roomList, float threshold)
    {
        List<RoomForCircle> mainRoomList = new List<RoomForCircle>();
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomForCircle item = roomList[i];
            if (item.rigidBody.Simualeted != false)
            {
                roomList.Remove(item);
                Destroy(item.gameObject);
                i--;
                continue;
            }
            // Debug.Log(item.size.Sum());
            if (item.size.Sum() >= threshold)
            {
                if (item.sprite != null)
                    item.sprite.color = Color.yellow;
                mainRoomList.Add(item);
                item.roomType = RoomType.MainRoom;
            }
        }
        return mainRoomList;

    }
    private static List<Edge> GetMainRoad(List<Edge> roadList, List<Vector2> roomPoint)
    {
        List<Edge> mainRoadList = MinSpanTree.KruskalSpanTree(roadList.ToBranch(roomPoint)).ToEdge();

        int addCount = 0;
        do
        {
            int index = UnityEngine.Random.Range(0, roadList.Count);
            if (mainRoadList.Contains(roadList[index]))
                continue;
            mainRoadList.Add(roadList[index]);
            addCount++;
            // todo 提高可配置性，可以对添加数量进行配置，或者使用计算的方式控制添加数量。
        } while (addCount < roadList.Count / 6 && mainRoadList.Count != roadList.Count);
        return mainRoadList;
    }


    private static RoomForCircle GenerationRoom(Transform parent, Vector2 position, Vector2 size, Sprite testSprtie = null)
    {
        GameObject newGo = new GameObject("room");
        newGo.transform.position = position;
        newGo.transform.rotation = Quaternion.identity;
        newGo.transform.parent = parent;
        RoomForCircle temp = newGo.AddComponent<RoomForCircle>();
        temp.size = size;
        temp.boxCollider = newGo.AddComponent<BoxCollider2D>();
        temp.rigidBody = newGo.AddComponent<RoomRigidBody>();
        temp.sprite = newGo.AddComponent<SpriteRenderer>();
        if (testSprtie != null)
        {
            temp.sprite.size = size;
            temp.sprite.sprite = testSprtie;
            temp.sprite.drawMode = SpriteDrawMode.Sliced;
        }
        return temp;
    }


    // 返回的是一个ve2的数组，头和尾是房间的门口
    private static Vector2[] GetPathsBetweenRoom(RoomForCircle roomA, RoomForCircle roomB)
    {
        float AxorBx = UnityEngine.Random.value;
        List<Vector2> list = new List<Vector2>();
        Vector2 TurnPoint;
        Vector2 pointA;
        Vector2 pointB;
        // todo 这一段看上去冗余
        if (AxorBx >= 0.5f)//AXBY
        {
            Vector2 RedCenter = new Vector2(roomA.transform.position.x, roomB.transform.position.y);
            //   -2 是防止处于墙壁边缘。
            Vector2 RedSize = new Vector2(roomA.size.x - 2, roomB.size.y - 2);
            // 转折点
            TurnPoint = getRandomPointInBox(RedSize, RedCenter);
            // 判断消减方向，确定门的位置
            float dir = (TurnPoint.y - roomA.transform.position.y);// 缩小方向已经确定 正 + 负 -
            pointA = new Vector2(
                TurnPoint.x, roomA.transform.position.y + 
                (((dir > 0) ? (roomA.size.y / 2) - 1 : -(roomA.size.y / 2)))
                );
            dir = (TurnPoint.x - roomB.transform.position.x); // 正 + 负数 -
            pointB = new Vector2(
                roomB.transform.position.x + 
                (((dir > 0) ? (roomB.size.x / 2) - 1 : -(roomB.size.x / 2))), TurnPoint.y
                );
        }
        else//BXAY
        {
            Vector2 RedCenter = new Vector2(roomB.transform.position.x, roomA.transform.position.y);
            Vector2 RedSize = new Vector2(roomB.size.x - 2, roomA.size.y - 2);
            TurnPoint = getRandomPointInBox(RedSize, RedCenter);
            float dir = (TurnPoint.x - roomA.transform.position.x);
            pointA = new Vector2(roomA.transform.position.x + (((dir > 0) ? (roomA.size.x / 2) - 1 : -(roomA.size.x / 2))), TurnPoint.y);
            dir = (TurnPoint.y - roomB.transform.position.y); // 正 + 负数 -
            pointB = new Vector2(TurnPoint.x, roomB.transform.position.y + (((dir > 0) ? (roomB.size.y / 2) - 1 : -(roomB.size.y / 2))));
        }
        // 如果转折点在房间中
        if (CheckInBox(TurnPoint, roomA.transform.position, roomA.size - new Vector2(2, 2)))
        {

            Vector2 dir = (pointB - TurnPoint);
            if (dir.x != 0)
            {
                float offset = roomA.size.x / 2 -((dir.x > 0) ?1:-1)*(TurnPoint.x - roomA.transform.position.x);
                TurnPoint += ((dir.x > 0) ? offset - 1 : -offset) * Vector2.right;
            }
            else
            {
                float offset = roomA.size.y / 2 -((dir.y > 0) ?1:-1)*(TurnPoint.y - roomA.transform.position.y);
                TurnPoint += ((dir.y > 0) ? offset - 1 : -offset) * Vector2.up;
            }
            list.Add(TurnPoint);
            list.Add(pointB);
        }
        else if (CheckInBox(TurnPoint, roomB.transform.position, roomB.size - new Vector2(2, 2)))
        {
            Vector2 dir = (pointA - TurnPoint);
            if (dir.x != 0)
            {
                float offset = roomB.size.x / 2 -((dir.x > 0) ?1:-1)*(TurnPoint.x - roomB.transform.position.x);
                TurnPoint += ((dir.x > 0) ? offset - 1 : -offset) * Vector2.right;
            }
            else
            {
                float offset = roomB.size.y / 2 -((dir.y > 0) ?1:-1)*(TurnPoint.y - roomB.transform.position.y);
                TurnPoint += ((dir.y > 0) ? offset - 1 : -offset) * Vector2.up;
            }
            list.Add(pointA);
            list.Add(TurnPoint);
        }
        else// 转折点在房间外
        {
            list.Add(pointA);
            list.Add(TurnPoint);
            list.Add(pointB);
        }
        return list.ToArray();
    }

    // todo 获取相连的房间
    private static void GetSideRoom()
    {

    }
    #endregion

    #region tile相关的代码
    public static void DrawRoomInTilemap(Tilemap floorTilemap, Tilemap wallTilemap, Tile floorTile, Tile wallTile, Vector2 centerPos, Vector2 size)
    {
        Vector3Int startPos = (centerPos - size / 2).ToVector3Int();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                floorTilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, floorTile);
                if (j == 0 || i == 0 || i == size.x - 1 || j == size.y - 1)
                {
                    wallTilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, wallTile);
                }
            }
        }
    }

    // 未完成的间断检测射线方法，试图减轻性能消耗
    // public static Collider2D[] LineCast(Vector2 startPos,Vector2 endPos, float space = 1)
    // {
    //     if(space<=0)
    //     {
    //         throw new ArgumentOutOfRangeException("Space","Space must be bigger than 0");
    //     }
    //     Vector2 dir = endPos - startPos;
    //     dir = dir.normalized;
    //     float dis= Vector2.Distance(endPos,startPos);
    //     for (int i = 1;(space*i)<dis;i++)
    //     {
    //         Physics2D.Raycast(startPos+dir*space*i,Vector2.down,)
    //     }
    // }


    // public static void DrawHallwayInTilemap(Tilemap floorTilemap, Tilemap wallTilemap, 
    //     Tile floorTile, Tile wallTile, Vector2 turnPoint, Vector2 pointA, Vector2 endPoint)
    // {

    //     // 绘制pointA到Turnpoint 的道路
    //     Vector2 Adir = (turnPoint - pointA).normalized;
    //     int n = (int)(turnPoint - pointA).Sum();
    //     Vector2 side = Adir.Reversal();
    //     for (int i = 0; i <= n; i++)
    //     {
    //         Vector3Int temp = (pointA + Adir * i).ToVector3Int();
    //         wallTilemap.SetTile(temp, null);
    //         floorTilemap.SetTile(temp, floorTile);
    //         if (floorTilemap.GetTile(temp + side.ToVector3Int()) == null)
    //             wallTilemap.SetTile(temp + side.ToVector3Int(), wallTile);
    //         if (floorTilemap.GetTile(temp - side.ToVector3Int()) == null)
    //             wallTilemap.SetTile(temp - side.ToVector3Int(), wallTile);
    //     }

    //     // 绘制pointB到Turnpoint 的道路
    //     Vector2 Bdir = (turnPoint - endPoint).normalized;
    //     n = (int)(turnPoint - endPoint).Sum();
    //     side = Bdir.Reversal();
    //     for (int i = 0; i <= n; i++)
    //     {
    //         Vector3Int temp = (endPoint + Bdir * i).ToVector3Int();
    //         wallTilemap.SetTile(temp, null);
    //         floorTilemap.SetTile(temp, floorTile);
    //         if (floorTilemap.GetTile(temp + side.ToVector3Int()) == null)
    //             wallTilemap.SetTile(temp + side.ToVector3Int(), wallTile);
    //         if (floorTilemap.GetTile(temp - side.ToVector3Int()) == null)
    //             wallTilemap.SetTile(temp - side.ToVector3Int(), wallTile);
    //     }

    //     // 绘制角落的方块
    //     Vector3Int wallTurnPoint = (Adir + turnPoint + Bdir).ToVector3Int();
    //     if (floorTilemap.GetTile(wallTurnPoint) == null)
    //         wallTilemap.SetTile(wallTurnPoint, wallTile);
    // }

    public static void DrawHallwayInTilemap(Tilemap floorTilemap, Tilemap wallTilemap,
     Tile floorTile, Tile wallTile, Vector2[] pathPoints)
    {
        Vector2 nextDir = (pathPoints[1] - pathPoints[0]).normalized;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector2 dir = nextDir;
            int n = (int)(pathPoints[i] - pathPoints[i + 1]).Sum();
            Vector2 side = dir.Reversal();
            for (int j = 0; j <= n; j++)
            {
                Vector3Int temp = (pathPoints[i] + dir * j).ToVector3Int();
                wallTilemap.SetTile(temp, null);
                floorTilemap.SetTile(temp, floorTile);
                if (floorTilemap.GetTile(temp + side.ToVector3Int()) == null)
                    wallTilemap.SetTile(temp + side.ToVector3Int(), wallTile);
                if (floorTilemap.GetTile(temp - side.ToVector3Int()) == null)
                    wallTilemap.SetTile(temp - side.ToVector3Int(), wallTile);
            }
            if (i != pathPoints.Length - 2)
            {
                nextDir = (pathPoints[i + 2] - pathPoints[i + 1]).normalized;
                Vector3Int wallTurnPoint = (dir + pathPoints[i + 1] - nextDir).ToVector3Int();
                if (floorTilemap.GetTile(wallTurnPoint) == null)
                    wallTilemap.SetTile(wallTurnPoint, wallTile);
            }
        }
    }

    #endregion

    public static bool CheckInBox(Vector2 point, Vector2 BoxCenter, Vector2 BoxSize)
    {
        Vector2 pointOffset = point - BoxCenter;
        return ((BoxSize.x / 2 > pointOffset.x && -BoxSize.x / 2 <= pointOffset.x)
         && (BoxSize.y / 2 > pointOffset.y && -BoxSize.y / 2 <= pointOffset.y));
    }
}
public static class RGHelper
{
    public static List<Branch> ToBranch(this List<Edge> edges, List<Vector2> pointList)
    {
        List<Branch> result = new List<Branch>();
        foreach (var e in edges)
        {
            result.Add(new Branch(e.indexA, e.indexB, Vector2.Distance(pointList[e.indexA], pointList[e.indexB])));
        }
        return result;
    }
    public static List<Edge> ToEdge(this List<Branch> branches)
    {
        List<Edge> result = new List<Edge>();
        foreach (var branch in branches)
        {
            result.Add(new Edge(branch.indexA, branch.indexB));
        }
        return result;
    }

    public static Vector2 Reversal(this Vector2 temp)
    {
        return new Vector2(temp.y, temp.x);
    }

}