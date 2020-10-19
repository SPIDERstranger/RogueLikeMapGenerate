using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoomGeneration : MonoBehaviour
{

    public int roomCount;
    public GameObject roomPrefab;
    public Vector2 roomModifyOffset;
    private int roomMapLength;
    private bool[][] roomMap;
    private Vector2Int startMapPos = new Vector2Int();
    private Vector2Int[] dirModify = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

    private Vector2Int roomMapMax;
    private List<GameObject> roomList = new List<GameObject>();
    // Use this for initialization
    private void Start()
    {
        Init();

        GenerationOnewayRooms(roomCount, startMapPos);
        UpdateText();
    }
    private void Init()
    {
        roomMapLength = (roomCount >> 1) * 2 + 1;
        startMapPos.x = startMapPos.y = roomCount >> 1 + 1;
        if (roomMap == null)
        {
            roomMap = new bool[roomMapLength][];
            for (int i = 0; i < roomMapLength; i++)
            {
                roomMap[i] = new bool[roomMapLength];
            }
        }
        else
        {
            for (int i = 0; i < roomMap.Length; i++)
            {
                for (int j = 0; j < roomMap[i].Length; j++)
                {
                    roomMap[i][j] = false;
                }
            }
        }
        for (int i = roomList.Count; i > 0; i--)
        {
            Destroy(roomList[i - 1]);
        }
        roomList.Clear();
        roomMapMax = new Vector2Int(roomMapLength - 1, roomMapLength - 1);

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 100), "OneWay"))
        {
            Init();
            GenerationOnewayRooms(roomCount, startMapPos);
            UpdateText();
        }

        if (GUI.Button(new Rect(0, 100, 200, 100), "Ways"))
        {
            Init();
            GenerationRoomsByWays(roomCount);
            UpdateText();
        }

    }

    public void GenerationOnewayRooms(int roomCount, Vector2Int startPos)
    {
        // 初始化
        Queue<Direction> dirTadu = new Queue<Direction>();
        Vector2Int currentPoint = new Vector2Int(startPos.x, startPos.y);
        Vector2Int nextPoint = new Vector2Int();

        // 生成首个房间
        if (!roomMap[startPos.x][startPos.y])
        {
            Vector2 worldPos = (startPos - startMapPos);
            worldPos *= roomModifyOffset;
            GameObject firstRoom = Instantiate(roomPrefab, (Vector2)(startPos - startMapPos) * roomModifyOffset, Quaternion.identity);
            roomList.Add(firstRoom);
            firstRoom.GetComponent<SpriteRenderer>().color = Color.green;
            roomMap[currentPoint.x][currentPoint.y] = true;
        }

        for (int i = 0; i < roomCount;)
        {
            // 获取下一个方向
            Direction nowDir;
            do
            {
                nowDir = (Direction)Random.Range(0, 4);
            } while (dirTadu.Contains(nowDir));

            // 删除禁忌表中时长最长的那个
            if (dirTadu.Count >= 2)
            {
                dirTadu.Dequeue();
            }
            //获取下一个路径点
            nextPoint = currentPoint + dirModify[(int)nowDir];
            //防止路径点超出地图范围
            nextPoint.Clamp(Vector2Int.zero, roomMapMax);
            //确定当前路径点是否已经存在房间
            if (!roomMap[nextPoint.x][nextPoint.y])
            {
                //将当前路径点设定为房间
                roomMap[nextPoint.x][nextPoint.y] = true;
                currentPoint = nextPoint;
                //计算当前路径点在世界空间中的位置
                Vector2 temp = (nextPoint - startMapPos);
                temp *= roomModifyOffset;
                //添加到列表中方便清理
                roomList.Add(Instantiate(roomPrefab, temp, Quaternion.identity));

                //更新禁忌表。
                switch (nowDir)
                {
                    case Direction.up:
                        dirTadu.Enqueue(Direction.down);
                        break;
                    case Direction.down:
                        dirTadu.Enqueue(Direction.up);
                        break;
                    case Direction.left:
                        dirTadu.Enqueue(Direction.right);
                        break;
                    case Direction.right:
                        dirTadu.Enqueue(Direction.left);
                        break;
                }
                i++;
            }
            // 防止走进死胡同
            if (!CheckArround(roomMap, currentPoint))
                break;
        }
        //RoomMapView(roomMap, roomMapLength, startMapPos);
    }

    public void GenerationRoomsByWays(int roomCount)
    {
        Vector2Int currentPoint = new Vector2Int(startMapPos.x, startMapPos.y);
        GameObject firstRoom = Instantiate(roomPrefab, Vector2.zero, Quaternion.identity);
        firstRoom.GetComponent<SpriteRenderer>().color = Color.red;
        roomList.Add(firstRoom);
        roomMap[startMapPos.x][startMapPos.y] = true;
        Direction nowDir;
        List<Direction> dirTadu = new List<Direction>();
        int lastRoom = roomCount - 2;
        for (int i = 0; i < 4; i++)
        {
            do
            {
                nowDir = (Direction)Random.Range(0, 4);
            } while (dirTadu.Contains(nowDir));
            dirTadu.Add(nowDir);
            int thisCount = Random.Range(0, roomCount - roomList.Count);
            if(thisCount!=0)
            GenerationOnewayRooms(thisCount, startMapPos + dirModify[(int)nowDir]);
            lastRoom -= thisCount;
        }
    }

    public void NewGenerationRoomsByWays(int roomCount)
    {
        Vector2Int currentPoint = new Vector2Int(startMapPos.x, startMapPos.y);
        roomList.Add(Instantiate(roomPrefab, Vector2.zero, Quaternion.identity));
        roomMap[currentPoint.x][currentPoint.y] = true;
        Direction nowDir;
        Queue<Direction> dirQueue = new Queue<Direction>();
        Queue<int> dirRoomCount = new Queue<int>();
        int lastRoom = roomCount - 2;
        for (int i = 0; i < 4; i++)
        {
            do
            {
                nowDir = (Direction)Random.Range(0, 4);
            } while (dirQueue.Contains(nowDir));
            dirQueue.Enqueue(nowDir);
            int thisCount = Random.Range(0, roomCount - roomList.Count);
            GenerationOnewayRooms(thisCount, dirModify[(int)nowDir]);
            lastRoom -= thisCount;
            dirRoomCount.Enqueue(thisCount);

        }

    }

    public void UpdateText()
    {
        for(int i =  0;i<roomList.Count;i++)
        {
            roomList[i].GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().text = i.ToString();
        }
    }
    public bool CheckArround(bool[][] map, Vector2Int currentPoint)
    {
        foreach (var i in dirModify)
        {
            var temp = currentPoint + i;
            temp.Clamp(Vector2Int.zero, roomMapMax);
            if (!map[temp.x][temp.y])
            {
                return true;
            }
        }
        return false;
    }

}

public enum Direction
{
    up,
    down,
    left,
    right
}
