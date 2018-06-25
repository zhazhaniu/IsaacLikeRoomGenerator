using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RogueLike;

namespace RogueLike
{
    public class ResLoader
    {
        public static GameObject LoadGameObject(string path)
        {
            GameObject go = Resources.Load<GameObject>(path);
            return GameObject.Instantiate(go);
        }
    }

    public class TestGenerator : MonoBehaviour
    {
        RoomGenerator rg = new RoomGenerator();
        int genIndex = 0;
        RogueLike.RoomGenerateParam generateConfig = new RoomGenerateParam();

        void Start()
        {
            generateConfig.normalRoomCount = 10;
            generateConfig.bossRoomCount = 1;
            generateConfig.randomCutAtDeep = 3;
            generateConfig.cutParam = 3;
            //pools
            generateConfig.normalPools = Data.normalPools;
            generateConfig.hiddenRoomPools = Data.hiddenRoomPools;
            generateConfig.rewardRoomPools = Data.rewardRoomPools;
            generateConfig.shopRoomPools = Data.shopRoomPools;
            generateConfig.bossRoomPools = Data.bossRoomPools;


            rg.generateConfig = generateConfig;
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
                    GameObject go = RogueLike.ResLoader.LoadGameObject(RogueLike.Data.doorRes[door.doorType]);
                    door.transform = go.transform;
                    go.transform.SetParent(parent);
                    go.transform.position = door.position;
                    go.name = "door";
                    go.AddComponent<DoorInspector>().doorData = door;
                    break;
            }
        }
        float lastShow = 0f;
        void Update()
        {
            if (Time.time - lastShow > 0.5f)
            {
                lastShow = Time.time;

                if (genIndex < rg.roomList.Count)
                {
                    var room = rg.roomList[genIndex++];
                    var go = RogueLike.ResLoader.LoadGameObject(room.GetPath());
                    go.AddComponent<RoomInspector>().roomData = room;
                    room.transform = go.transform;
                    room.transform.position = room.position;
                    SetRoomColor(go, room.roomType);

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

        void SetRoomColor(GameObject go, RoomType roomType)
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
            else if (roomType == RoomType.Normal)
            {
                c = Color.yellow + new Color(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            }
            else if (roomType == RoomType.Shop)
            {
                c = Color.green;
            }
            else if (roomType == RoomType.Reward)
            {
                c = new Color(0.8f, 0.45f, 0.65f);
            }
            else if (roomType == RoomType.Hidden)
            {
                c = Color.gray;
            }
            else
            {
                c = Color.red;
            }

            SpriteRenderer[] sr = go.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < sr.Length; ++i)
            {
                sr[i].color = c;
            }

        }
    }

}

