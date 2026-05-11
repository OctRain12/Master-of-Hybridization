using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TestInputManager : MonoBehaviour
{
    //面板选择对应物种
    [Header("选择物种")]
    public SpeciesData speciesA;
    public SpeciesData speciesB;
    public GridManager gridManager;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputManager();
    }
    //输入操作方法
    private void InputManager()
    {
        //获取鼠标下的地块
        LandTile hoverdTile = GetTileUnderMouse();
        if(hoverdTile == null) return;
        //按键1：种植A,需要判断是否为空土地
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(hoverdTile.currentState == TileState.Empty)
            {
                hoverdTile.Plant(new PlantInstanceData(speciesA, speciesA.defaultGenoType));
            }
        }
        //按键2：种植B,需要判断是否为空土地
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(hoverdTile.currentState == TileState.Empty)
            {
                hoverdTile.Plant(new PlantInstanceData(speciesB, speciesB.defaultGenoType));
            }
        }
        //按键3：收获果实，需要判断是否成熟
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(hoverdTile.currentState == TileState.Mature)
            {
                hoverdTile.Harvest(false);
            }
        }
        //按键4：收获种子
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            if(hoverdTile.currentState == TileState.Mature)
            {
                hoverdTile.Harvest(true);
            }
        }
    }


    //采用射线检测当前鼠标下的地块
    private LandTile GetTileUnderMouse()
    {
        //获取对应世界坐标
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos,Vector2.zero);
        //成功获取到collider的时候，返回对应地块
        if(hit.collider != null)
        {
            return hit.collider.GetComponent<LandTile>();
        }
        return null;
    }
}
