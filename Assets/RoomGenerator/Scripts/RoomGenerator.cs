using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{

    public class Grid
    {
        //mark of this grid has been searched
        public bool steped = false;
        public Vector3 position;
        public RoomNode owner = null;
        public Vector2Int index;

        public DoorNode upDoor = new DoorNode();
        public DoorNode downDoor = new DoorNode();
        public DoorNode leftDoor = new DoorNode();
        public DoorNode rightDoor = new DoorNode();

        public Grid()
        {
            upDoor.grid = this;
            downDoor.grid = this;
            leftDoor.grid = this;
            rightDoor.grid = this;
        }
    }

    public struct RoomGenerateInfo
    {
        public Type ty;
        public RoomType roomType;
    }

    public struct SuspendTask
    {
        public int x;
        public int y;
        public int depth;
    }

    public partial class RoomGenerator
    {
        public Vector2Int MapSize = new Vector2Int(16, 16);
        public Grid[,] grids = null;
        public List<RoomNode> roomList = new List<RoomNode>();
        int usedGrid = 0;
        int maxUseGrid = 30;

        public RoomGenerateParam generateConfig;
        RoomGenerateParam curGenerateInfo;

        System.Random random = null;

        public RoomGenerator()
        {
            SetSeed(System.Environment.TickCount % 10000);
        }

        public void SetSeed(int seed)
        {
            Debug.Log("Room Generator set seed:" + seed);
            random = new System.Random(seed);
        }

        int Random(int min, int max)
        {
            return random.Next(min, max);
        }

        public bool IsValidIndex(int x, int y)
        {
            return (x >= 0 && x < MapSize.x && y >= 0 && y < MapSize.y);
        }

        public Grid GetGrid(int x, int y)
        {
            if (!IsValidIndex(x, y))
            {
                return null;
            }

            return grids[x, y];
        }

        Grid GetEmptyGrid(int x, int y)
        {
            Grid g = GetGrid(x, y);
            if (g != null && g.owner == null)
            {
                return g;
            }

            return null;
        }

        //Add all empty grid beyond the grid
        void AddEmptyGridToList(List<Grid> list, Grid grid)
        {
            if (grid == null)
            {
                return;
            }

            Grid up = GetEmptyGrid(grid.index.x, grid.index.y + 1);
            Grid down = GetEmptyGrid(grid.index.x, grid.index.y - 1);
            Grid left = GetEmptyGrid(grid.index.x - 1, grid.index.y);
            Grid right = GetEmptyGrid(grid.index.x + 1, grid.index.y);

            if (up != null) list.Add(up);
            if (down != null) list.Add(down);
            if (left != null) list.Add(left);
            if (right != null) list.Add(right);
        }

        void AddNearRoomToHSet(HashSet<RoomNode> hset, Grid grid)
        {
            if (grid == null)
            {
                return;
            }

            Grid up = grid.upDoor.doorType != DoorType.None ? GetGrid(grid.index.x + 1, grid.index.y) : null;
            Grid down = grid.downDoor.doorType != DoorType.None ? GetGrid(grid.index.x - 1, grid.index.y) : null;
            Grid left = grid.leftDoor.doorType != DoorType.None ? GetGrid(grid.index.x, grid.index.y - 1) : null;
            Grid right = grid.rightDoor.doorType != DoorType.None ? GetGrid(grid.index.x, grid.index.y + 1) : null;

            if (up != null && up.downDoor.doorType != DoorType.None && up.owner != null && up.owner != grid.owner && !hset.Contains(up.owner)) hset.Add(up.owner);
            if (down != null && down.upDoor.doorType != DoorType.None && down.owner != null && down.owner != grid.owner && !hset.Contains(down.owner)) hset.Add(down.owner);
            if (left != null && left.rightDoor.doorType != DoorType.None && left.owner != null && left.owner != grid.owner && !hset.Contains(left.owner)) hset.Add(left.owner);
            if (right != null && right.leftDoor.doorType != DoorType.None && right.owner != null && right.owner != grid.owner && !hset.Contains(right.owner)) hset.Add(right.owner);
        }

        //Get all empty grid beyond the room
        List<Grid> GetOutsideEmptyGrid(RoomNode room)
        {
            List<Grid> list = new List<Grid>();
            for (int i = 0; i < room.gridList.Count; ++i)
            {
                var grid = room.gridList[i];
                AddEmptyGridToList(list, grid);
            }
            return list;
        }

        //Get all empty grid beyond rooms (exclude the special room)
        List<Grid> GetConnectableGrids()
        {
            List<Grid> list = new List<Grid>();
            for (int i = 0; i < roomList.Count; ++i)
            {
                var room = roomList[i];
                if (room.roomType == RoomType.Normal)
                {
                    var tmpList = GetOutsideEmptyGrid(room);
                    list.AddRange(tmpList);
                }
            }

            return list;
        }

        public HashSet<RoomNode> GetConnectedRoom(RoomNode room)
        {
            HashSet<RoomNode> hset = new HashSet<RoomNode>();
            for (int i = 0; i < room.gridList.Count; ++i)
            {
                var grid = room.gridList[i];
                AddNearRoomToHSet(hset, grid);
            }
            return hset;
        }

        public RoomGenerateInfo RandomFightRoom()
        {
            List<Type> pools = null;
            RoomGenerateInfo info = new RoomGenerateInfo();
            info.ty = null;
            info.roomType = RoomType.Normal;

            if (generateConfig.normalRoomCount > curGenerateInfo.normalRoomCount)
            {
                pools = generateConfig.normalPools;
            }

            if (pools != null)
            {
                int rnd = Random(0, pools.Count);
                info.ty = pools[rnd];
            }

            return info;
        }

        void InitNewMap(Vector3 basePosition)
        {
            curGenerateInfo = new RoomGenerateParam();
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
                    grid.position = basePosition + new Vector3((col - 1) * Data.GridSize.x, -(row - 1) * Data.GridSize.y, 0);

                    //door pos
                    grid.upDoor.position = grid.position + new Vector3(0, Data.GroundSize.y * 0.5f + Data.DoorHeight * 0.5f, 0);
                    grid.downDoor.position = grid.position + new Vector3(0, -Data.GroundSize.y * 0.5f - Data.DoorHeight * 0.5f, 0);
                    grid.leftDoor.position = grid.position + new Vector3(-Data.GroundSize.x * 0.5f - Data.DoorHeight * 0.5f, 0, 0);
                    grid.rightDoor.position = grid.position + new Vector3(Data.GroundSize.x * 0.5f + Data.DoorHeight * 0.5f, 0, 0);
                }
            }
        }

        public void StartGenerate(int x, int y, Vector3 basePosition)
        {
            if (!IsValidIndex(x, y))
            {
                Debug.LogError("x or y is invalid");
                return;
            }

            if (MapSize.x > 256 || MapSize.x < 0 || MapSize.y > 256 || MapSize.y < 0)
            {
                Debug.LogError("MapSize is invalid");
                return;
            }

            InitNewMap(basePosition);
            PlaceInitRoom(x, y);


            //4 direction
            var dirOffset = GenDirList();
            GenerateAt(x + dirOffset[0].x, y + dirOffset[0].y, 1);
            GenerateAt(x + dirOffset[1].x, y + dirOffset[1].y, 1);
            GenerateAt(x + dirOffset[2].x, y + dirOffset[2].y, 1);
            GenerateAt(x + dirOffset[3].x, y + dirOffset[3].y, 1);

            //generate normalRoom
            int generateCount = 0;
            while (generateConfig.normalRoomCount > curGenerateInfo.normalRoomCount && ++generateCount < 1000)
            {
                var canConnect = GetConnectableGrids();
                for (int i = 0; i < canConnect.Count; ++i)
                {
                    var g = canConnect[i];
                    GenerateAt(g.index.x, g.index.y, 1);
                    if (generateConfig.normalRoomCount <= curGenerateInfo.normalRoomCount)
                    {
                        break;
                    }
                }
            }

            PlaceHiddenRoom();

            PlaceBossRoom();
            PlaceShopRoom();
            PlaceRewardRoom();
            PlaceSuperHiddenRoom();

            AssociateDoors();
            SetRoomIndex();
        }

        int GenerateAt(int x, int y, int depth)
        {
            if (!IsValidIndex(x, y))
            {
                return 0;
            }

            if (usedGrid >= maxUseGrid)
            {
                return 0;
            }

            //if too deep
            if (depth > generateConfig.randomCutAtDeep && Random(1, depth) < generateConfig.cutParam)
            {
                return 1;
            }

            Grid grid = grids[x, y];
            if (grid.steped)
            {
                return 0;
            }
            grid.steped = true;

            if (grid.owner == null)
            {
                RoomGenerateInfo info = RandomFightRoom();
                if (info.ty != null)
                {
                    RoomNode room = (RoomNode)Activator.CreateInstance(info.ty);
                    room.roomType = info.roomType;
                    if (room.Place(x, y, this))
                    {
                        ++curGenerateInfo.normalRoomCount;
                        usedGrid += room.gridList.Count;
                        roomList.Add(room);
                        room.SetPosition(room.gridList[0].position);
                        room.RepositionDoors();
                        room.CalculateTransportPos();
                    }
                }
            }

            //4 direction
            var dirOffset = GenDirList();
            if (1 == GenerateAt(x + dirOffset[0].x, y + dirOffset[0].y, depth + 1)) return 1;
            if (1 == GenerateAt(x + dirOffset[1].x, y + dirOffset[1].y, depth + 1)) return 1;
            if (1 == GenerateAt(x + dirOffset[2].x, y + dirOffset[2].y, depth + 1)) return 1;
            if (1 == GenerateAt(x + dirOffset[3].x, y + dirOffset[3].y, depth + 1)) return 1;

            return 0;
        }

        bool CanAssociate(Grid grid1, DoorNode door1, Grid grid2, DoorNode door2)
        {
            if (door1.doorType == DoorType.None || door2.doorType == DoorType.None)
            {
                return false;
            }

            if (grid1.owner == grid2.owner)
            {
                return false;
            }

            if (grid1.owner == null || grid2.owner == null)
            {
                return false;
            }

            if (door1.associateDoor != null || door2.associateDoor != null)
            {
                return false;
            }

            return true;
        }

        void AssociateDoors()
        {
            int row = grids.GetLength(0);
            int col = grids.GetLength(1);
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < col; ++j)
                {
                    Grid grid = GetGrid(i, j);
                    Grid up = GetGrid(i - 1, j);
                    Grid down = GetGrid(i + 1, j);
                    Grid left = GetGrid(i, j - 1);
                    Grid right = GetGrid(i, j + 1);

                    if (up != null && CanAssociate(grid, grid.upDoor, up, up.downDoor))
                    {
                        grid.upDoor.Associate(up.downDoor);
                    }

                    if (down != null && CanAssociate(grid, grid.downDoor, down, down.upDoor))
                    {
                        grid.downDoor.Associate(down.upDoor);
                    }

                    if (left != null && CanAssociate(grid, grid.leftDoor, left, left.rightDoor))
                    {
                        grid.leftDoor.Associate(left.rightDoor);
                    }

                    if (right != null && CanAssociate(grid, grid.rightDoor, right, right.leftDoor))
                    {
                        grid.rightDoor.Associate(right.leftDoor);
                    }
                }
            }
        }

        List<Vector2Int> GenDirList()
        {
            List<Vector2Int> result = new List<Vector2Int>()
            {
                new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0),
            };

            for (int i = 0; i < 2; ++i)
            {
                int idxA = Random(0, result.Count);
                int idxB = Random(0, result.Count);
                Vector2Int tmp = result[idxB];
                result[idxB] = result[idxA];
                result[idxA] = tmp;
            }
            return result;
        }

        void SetRoomIndex()
        {
            for (int i = 0; i < roomList.Count; ++i)
            {
                roomList[i].index = i;
            }
        }
    } //RoomGenerator


} //namespace

