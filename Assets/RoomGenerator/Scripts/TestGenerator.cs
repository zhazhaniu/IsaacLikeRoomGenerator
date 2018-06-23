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
    RogueLike.RoomGenerateLimit generateLimit = new RoomGenerateLimit();
    
    void Start () {
        generateLimit.normalRoomCount = 10;
        generateLimit.bossRoomCount = 1;
        generateLimit.randomCutAtDeep = 3;
        generateLimit.cutParam = 3;

        rg.generateConfig = generateLimit;
        rg.MapSize = new Vector2Int(32, 32);
        rg.StartGenerate(15, 15, Vector3.zero);
	}

    void LoadDoor(DoorNode door, Transform parent)
    {
        if (door.associateDoor == null)
        {
            return;
        }
        switch (door.doorType)
        {
            case DoorType.None:
                break;
            default:
                GameObject go = ResLoader.LoadGameObject(Data.doorRes[door.doorType]);
                door.transform = go.transform;
                go.transform.SetParent(parent);
                go.transform.position = door.position;
                go.name = "door";
                go.AddComponent<DoorInspector>().doorData = door;
                break;
        }
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
                RandomRoomColor(go, room.roomType);

                for (int i = 0; i < room.gridList.Count; ++i)
                {
                    RogueLike.Grid grid = room.gridList[i];
                    LoadDoor(grid.upDoor, room.transform);
                    LoadDoor(grid.downDoor, room.transform);
                    LoadDoor(grid.leftDoor, room.transform);
                    LoadDoor(grid.rightDoor, room.transform);
                }
            }

        }
	}

    void RandomRoomColor(GameObject go, RoomType roomType)
    {
        Color c = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
        if (roomType == RoomType.Init)
        {
            c = Color.white;
        }
        else if (roomType == RoomType.Boss)
        {
            c = Color.black;
        }

        SpriteRenderer[] sr = go.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sr.Length; ++i)
        {
            sr[i].color = c;
        }

    }
}
