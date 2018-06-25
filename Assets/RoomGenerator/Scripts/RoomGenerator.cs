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

    public class RoomGenerator
    {
        public Vector2Int MapSize = new Vector2Int(16, 16);
        public Grid[,] grids = null;
        public List<RoomNode> roomList = new List<RoomNode>();
        int usedGrid = 0;
        int maxUseGrid = 30;

        public RoomGenerateParam generateConfig;
        RoomGenerateParam curGenerateInfo;

        //Mix DFS & BFS
        List<SuspendTask> suspendTask = new List<SuspendTask>();

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

        public RoomGenerateInfo RandomOneRoom()
        {
            RoomGenerateInfo info = new RoomGenerateInfo();
            info.ty = null;
            info.roomType = RoomType.Normal;

            List<Type> pools = null;

            if (generateConfig.normalRoomCount > curGenerateInfo.normalRoomCount)
            {
                ++curGenerateInfo.normalRoomCount;
                pools = generateConfig.normalPools;
            }
            else if (generateConfig.bossRoomCount > curGenerateInfo.bossRoomCount)
            {
                ++curGenerateInfo.bossRoomCount;
                pools = generateConfig.bossRoomPools;
                info.roomType = RoomType.Boss;
            }
            else if (generateConfig.hiddenRoomCount > curGenerateInfo.hiddenRoomCount)
            {
                ++curGenerateInfo.hiddenRoomCount;
                pools = generateConfig.hiddenRoomPools;
                info.roomType = RoomType.Hidden;
            }
            else if (generateConfig.rewardRoomCount > curGenerateInfo.rewardRoomCount)
            {
                ++curGenerateInfo.rewardRoomCount;
                pools = generateConfig.rewardRoomPools;
                info.roomType = RoomType.Reward;
            }
            else if (generateConfig.shopRoomCount > curGenerateInfo.shopRoomCount)
            {
                ++curGenerateInfo.shopRoomCount;
                pools = generateConfig.shopRoomPools;
                info.roomType = RoomType.Shop;
            }

            if (pools != null)
            {
                int rnd = UnityEngine.Random.Range(0, pools.Count);
                info.ty = pools[rnd];
            }


            return info;
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

                    //door transport pos
                    grid.upDoor.transportPos = grid.upDoor.position - new Vector3(0, 0.6f, 0);
                    grid.downDoor.transportPos = grid.downDoor.position + new Vector3(0, 0.6f, 0);
                    grid.leftDoor.transportPos = grid.leftDoor.position + new Vector3(0.6f, 0, 0);
                    grid.rightDoor.transportPos = grid.rightDoor.position - new Vector3(0.6f, 0, 0);
                }
            }

            //init room
            Room_Init initRoom = new Room_Init();
            initRoom.Place(x, y, this);
            usedGrid += initRoom.gridList.Count;
            roomList.Add(initRoom);
            initRoom.SetPosition(initRoom.gridList[0].position);

            //4 direction
            var dirOffset = GenDirList();
            GenerateAt(x + dirOffset[0].x, y + dirOffset[0].y, 1);
            GenerateAt(x + dirOffset[1].x, y + dirOffset[1].y, 1);
            GenerateAt(x + dirOffset[2].x, y + dirOffset[2].y, 1);
            GenerateAt(x + dirOffset[3].x, y + dirOffset[3].y, 1);

            while (suspendTask.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, suspendTask.Count);
                var genTask = suspendTask[idx];
                suspendTask.RemoveAt(idx);
                GenerateAt(genTask.x, genTask.y, genTask.depth);
            }
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
            if (depth > generateConfig.randomCutAtDeep && UnityEngine.Random.Range(1, depth) < generateConfig.cutParam)
            {
                //depth - 3 : make it more easy to generate. Not sure is it good
                suspendTask.Add(new SuspendTask() { x = x, y = y, depth = depth - 3 });
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
                RoomGenerateInfo info = RandomOneRoom();
                if (info.ty != null)
                {
                    RoomNode room = (RoomNode)Activator.CreateInstance(info.ty);
                    room.roomType = info.roomType;
                    if (room.Place(x, y, this))
                    {
                        usedGrid += room.gridList.Count;
                        roomList.Add(room);
                        room.SetPosition(room.gridList[0].position);
                        room.MarkUnborderDoors();
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

            for (int i = 0; i < 5; ++i)
            {
                int idxA = UnityEngine.Random.Range(0, result.Count);
                int idxB = UnityEngine.Random.Range(0, result.Count);
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

