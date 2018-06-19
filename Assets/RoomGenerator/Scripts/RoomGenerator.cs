using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public class Data
    {
        // 1, 1 is an offset to show in demo scene better
        public static Vector2 GridSize = new Vector2(12.80f / 2, 7.20f) + new Vector2(1, 1); 
    }

    public class DoorNode
    {
        //common, hidden, boss, etc...
        public int doorType = 0;
        public Vector3 position;
        public DoorNode associateDoor = null;
    }

    public class RoomInspector : MonoBehaviour
    {
        [SerializeField]
        public RoomNode roomData = null;
    }

    [Serializable]
    public class RoomNode
    {
        public bool[,] gridDesc;
        public List<Grid> gridList = new List<Grid>();
        public Transform transform;
        public Vector3 position;

        public virtual string GetPath()
        {
            return "";
        }

        public bool IsFitSize(int x, int y, RoomGenerator generator)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    if (gridDesc[i, j])
                    {
                        var grid = generator.GetGrid(x + i , y + j);
                        if (grid == null || grid.owner != null)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        void ApplyRoom(int x, int y, RoomGenerator generator)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    if (gridDesc[i, j])
                    {
                        var grid = generator.GetGrid(x + i, y + j);
                        grid.owner = this;
                        gridList.Add(grid);
                    }
                }
            }
        }

        public bool TryPutRoom(int x, int y, RoomGenerator generator)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    //if need a non-empty grid
                    if (IsFitSize(x - i, y - j, generator))
                    {
                        ApplyRoom(x - i, y - j, generator);
                        return true;
                    }
                }
            }

            return false;
        }

        //return is there exist an offset of gridDesc, can place the room
        public bool Place(int x, int y, RoomGenerator generator)
        {
            return TryPutRoom(x, y, generator);
        }

        
        public virtual void SetPosition(Vector3 pos)
        {
            position = pos;
        }
    }

    public class Grid
    {
        public bool inSteping = false;
        public Vector3 position;
        public RoomNode owner = null;
        public Vector2Int index;

        public DoorNode upDoor      = null;
        public DoorNode downDoor    = null;
        public DoorNode leftDoor    = null;
        public DoorNode rightDoor   = null;
    }

    public class RoomGenerator
    {
        public Vector2Int MapSize = new Vector2Int(16, 16);
        public Grid[,] grids = null;
        public List<RoomNode> roomList = new List<RoomNode>();
        int usedGrid = 0;
        int maxUseGrid = 30;

        public List<Type> roomPools = new List<Type>()
        {
            //typeof(Room_1X1),
            typeof(Room_1X2),
            typeof(Room_2X2),
            typeof(Room_2X2_1101),
        };

        public Type RandomOneRoom()
        {
            int rnd = UnityEngine.Random.Range(0, roomPools.Count);
            return roomPools[rnd];
        }

        public bool IsValidIndex(int x, int y)
        {
            return (x > 0 && x < MapSize.x && y > 0 && y < MapSize.y);
        }

        public Grid GetGrid(int x, int y)
        {
            if (!IsValidIndex(x, y))
            {
                return null;
            }

            return grids[x, y];
        }

        public void StartGenerate(int x, int y, Vector3 basePosition)
        {
            if (!IsValidIndex(x, y))
            {
                Debug.LogError("x or y is invalid");
                return;
            }

            if (MapSize.x > 100 || MapSize.x < 0 || MapSize.y > 100 || MapSize.y < 0)
            {
                Debug.LogError("MapSize is invalid");
                return;
            }

            roomList.Clear();
            usedGrid = 0;

            var size = new int[] { MapSize.x, MapSize.y };
            var startIndex = new int[] { 0, 0 };
            grids = (Grid[,])Array.CreateInstance(typeof(Grid), size, startIndex);

            for (int row = 0; row < MapSize.x; ++row)
            {
                for (int col = 0; col < MapSize.y; ++col)
                {
                    var grid = new Grid();
                    grid.index = new Vector2Int(row, col);
                    grids[row, col] = grid;
                    grid.position = basePosition + new Vector3((col - 1) * Data.GridSize.y, -(row - 1) * Data.GridSize.x, 0);
                }
            }

            //init room
            Room_Init initRoom = new Room_Init();
            initRoom.Place(x, y, this);
            usedGrid += initRoom.gridList.Count;
            roomList.Add(initRoom);
            initRoom.SetPosition(initRoom.gridList[0].position);

            GenerateAt(x, y);
        }

        void GenerateAt(int x, int y)
        {
            if (!IsValidIndex(x, y))
            {
                return;
            }

            if (usedGrid >= maxUseGrid)
            {
                return;
            }

            Grid grid = grids[x, y];
            if (grid.inSteping)
            {
                return;
            }
            grid.inSteping = true;

            if (grid.owner == null)
            {
                Type roomType = RandomOneRoom();
                RoomNode room = (RoomNode)Activator.CreateInstance(roomType);
                if (room.Place(x, y, this))
                {
                    usedGrid += room.gridList.Count;
                    roomList.Add(room);
                    room.SetPosition(room.gridList[0].position);
                }
            }

            //4 direction
            if (UnityEngine.Random.Range(1, 100) > 30)
            {
                GenerateAt(x, y + 1);
            }

            if (UnityEngine.Random.Range(1, 100) > 90)
            {
                GenerateAt(x + 1, y);
            }

            if (UnityEngine.Random.Range(1, 100) > 20)
            {
                GenerateAt(x, y - 1);
            }

            if (UnityEngine.Random.Range(1, 100) > 80)
            {
                GenerateAt(x - 1, y);
            }

             grid.inSteping = false;
        }

    } //RoomGenerator


} //namespace

