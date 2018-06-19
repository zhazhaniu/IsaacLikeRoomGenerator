using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RogueLike;

public class ResLoader
{
    public static GameObject LoadGameObject(string path)
    {
        GameObject go = Resources.Load<GameObject>(path);
        return GameObject.Instantiate(go);
    }
}
public class TestGenerator : MonoBehaviour {
    RoomGenerator rg = new RoomGenerator();
    int genIndex = 0;
    
    void Start () {
        rg.StartGenerate(8, 8, Vector3.zero);
	}

    float lastShow = 0f;
	void Update ()
    {
        if (Time.time - lastShow > 0.5f)
        {
            lastShow = Time.time;

            if (genIndex < rg.roomList.Count)
            {
                var room = rg.roomList[genIndex++];
                var go = ResLoader.LoadGameObject(room.GetPath());
                go.AddComponent<RoomInspector>().roomData = room;
                room.transform = go.transform;
                room.transform.position = room.position;
            }
        }
        
		
	}
}
