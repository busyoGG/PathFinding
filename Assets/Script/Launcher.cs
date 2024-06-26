using System;
using System.Collections;
using System.Collections.Generic;
using AStar;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private bool _inited;

    private List<AStarData> _path;
    // Start is called before the first frame update
    void Start()
    {
        string json = FileUtils.ReadFile(Application.dataPath + "/Json/map.json");
        AStarManager.Ins().Init(json);
        _path = AStarManager.Ins().Search(new Vector2(0, 0), new Vector2(8, 8));

        _inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (_inited)
        {
            AStarManager.Ins().ShowMapGizmos(Color.green,0);
            AStarManager.Ins().ShowPath(_path,Color.red,0);
        }
    }
}
