using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class DrawTileMap : MonoBehaviour
{
    public Tilemap tilemap;

    public Tile roomTile;


    private void Start()
    {
        // BoxFill(tilemap, roomTile, new Vector2(13, 11), new Vector2(8, 14));
        // DrawLine(tilemap,roomTile,Vector2.zero,Vector2.left,1);
        // DrawPolyLine(tilemap,roomTile,Vector2.zero,Vector2.up*-5+Vector2.right*10);
    }
    public static void BoxFill(Tilemap tilemap, Tile roomTile, Vector2 centerPos, Vector2 size)
    {
        Vector3Int startPos = (centerPos - size / 2).ToVector3Int();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                tilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, roomTile);
            }
        }
    }

    public static void BoxWire(Tilemap tilemap, Tile wallTile, Vector2 centerPos, Vector2 size)
    {
        Vector3Int startPos = (centerPos - size / 2).ToVector3Int();
        // todo 这个做法效率太低 ，需要更优秀的算法进行处理。

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (j == 0 || i == 0 || i == size.x - 1 || j == size.y - 1)
                {
                    tilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, wallTile);
                }
            }
        }
    }
    public static void DrawRoom(Tilemap floorTilemap, Tilemap wallTilemap, Tile roomTile, Tile wallTile, Vector2 centerPos, Vector2 size)
    {
        Vector3Int startPos = (centerPos - size / 2).ToVector3Int();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                floorTilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, roomTile);
                if (j == 0 || i == 0 || i == size.x - 1 || j == size.y - 1)
                {
                    wallTilemap.SetTile(startPos + Vector3Int.right * i + Vector3Int.up * j, wallTile);
                }
            }
        }
    }

    public static void DrawLine(Tilemap tilemap, Tile tile, Vector2 startPos, Vector2 dir, int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            tilemap.SetTile((startPos + dir * i).ToVector3Int(), tile);
        }
    }

    public  static void DrawLine(Tilemap tilemap,Tile tile ,Vector2 startPos,Vector2 endPos)
    {
        if((startPos-endPos).x==0||(startPos-endPos).y==0)
        {
            DrawLine(tilemap,tile,startPos,(endPos-startPos).normalized,(int)(endPos-startPos).Sum());
        }
        else{
            DrawPolyLine(tilemap,tile,startPos,endPos);
        }
    }
    public static void DrawPolyLine(Tilemap tilemap, Tile tile, Vector2 startPos, Vector2 endPos)
    {
        int offsetX = (int)endPos.x - (int)startPos.x;
        int offsetY = (int)endPos.y - (int)startPos.y;
        if (offsetX > offsetY)
        {
            Vector2 dir = Vector2.right * ((offsetX > 0) ? 1 : -1);
            DrawLine(tilemap, tile, startPos, dir, Mathf.Abs(offsetX));
            dir = Vector2.up * ((offsetY > 0) ? 1 : -1);
            DrawLine(tilemap, tile, startPos + Vector2.right * offsetX , dir, Mathf.Abs(offsetY)+1);
        }
        else
        {
            Vector2 dir = Vector2.up * ((offsetY > 0) ? 1 : -1);
            DrawLine(tilemap, tile, startPos, dir, Mathf.Abs(offsetY));
            dir = Vector2.right * ((offsetX > 0) ? 1 : -1);
            DrawLine(tilemap, tile, startPos + Vector2.up * offsetY, dir, Mathf.Abs(offsetX)+1);
        }
    }

    public static void DrawHallway(Tilemap floorTilemap,Tilemap wallTilemap,Tile floorTile,Tile wallTile ,Vector2 startPos,Vector2 endPos)
    {
        DrawPolyLine(floorTilemap,floorTile,startPos,endPos);
    }
}
public static class DTMHelper
{
    public static Vector3Int ToVector3Int(this Vector2 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, 0);
    }
}