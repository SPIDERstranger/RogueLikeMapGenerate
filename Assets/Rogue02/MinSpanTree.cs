using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinSpanTree
{
    public static List<Branch> KruskalSpanTree(List<Branch> branches)
    {
        List<Branch> branchList = new List<Branch>(branches);
        List<Branch> result = new List<Branch>();
        Dictionary<int, int> fatherDic = new Dictionary<int, int>();
        int length = 0;
        // 设定将所有节点放到字典中方便查找
        foreach (Branch b in branchList)
        {
            
            fatherDic.TryAdd(b.indexA, b.indexA);
            fatherDic.TryAdd(b.indexB, b.indexB);
        }
        // 对权重进行排序
        branchList.Sort((left, right) =>
        {
            if (left.weight > right.weight)
                return 1;
            else if (left.weight == right.weight)
                return 0;
            else
                return -1;
        });
        // 遍历分支
        for (int i = 0; i < branchList.Count; i++)
        {
            // 如果达到了最小生成树的数量就退出。
            if (length == fatherDic.Count)
                break;
            // 如果两个节点位于的团体不是同一个团体
            if (GetFather(fatherDic, branchList[i].indexA) != GetFather(fatherDic, branchList[i].indexB))
            {
                //将他们的父亲节点连接起来。即设定B的老大的老大设定为A的老大
                fatherDic[GetFather(fatherDic, branchList[i].indexB)] = GetFather(fatherDic, branchList[i].indexA);
                result.Add(branchList[i]);
                length++;
            }
        }
        return result;
    }
    private static int GetFather(Dictionary<int, int> dic, int num)
    {
        if (dic[num] == num)
            return num;
        else
        {
            return GetFather(dic, dic[num]);
        }
    }
}

public static class MSTHelper
{
    public static bool TryAdd<Tkey,TValue>(this Dictionary<Tkey,TValue> dic ,Tkey key ,TValue value)
    {
        if(!dic.ContainsKey(key))
        {
            dic.Add(key,value);
            return true;
        }
        else{
            return false;
        }

    }
}

[System.Serializable]
public struct Branch
{
    public int indexA;
    public int indexB;
    public float weight;

    public Branch(int indexA, int indexB, float weight)
    {
        this.indexA = indexA;
        this.indexB = indexB;
        this.weight = weight;
    }
}
