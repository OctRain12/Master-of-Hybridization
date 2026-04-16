using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    
    [Header("地块设置")]
    public int high = 5;
    public int width = 5;
    public float cellsize = 1.1f;

    [Header("引用")]
    public GameObject gridPrefab;
    
    //关键字典，用于以后地块的索引等
    private Dictionary<Vector2Int,GameObject> gridDictionary = new Dictionary<Vector2Int, GameObject>();
    
    
    void Start()
    {
        Camera.main.transform.position = new Vector3(width*cellsize/2-cellsize/2,high*cellsize/2-cellsize/2,-10);
        GenerateGrid();    
    }

//生成地块
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < high; y++)
            {
                //世界坐标
                Vector2 pos = new Vector2(x*cellsize,y*cellsize);
                //实例化地块
                GameObject newTile = Instantiate(gridPrefab, pos, quaternion.identity, transform);
                //命名
                newTile.name = $"Tile_{x}_{y}";
                //添加到字典
                gridDictionary.Add(new Vector2Int(x,y),newTile);
            }
        }
    }

//后续查询地块的方法
    public GameObject GetTileAtPosition(Vector2Int pos)
    {
        if(gridDictionary.ContainsKey(pos))
        {
            return gridDictionary[pos];
        }
        return null;
    }


}
