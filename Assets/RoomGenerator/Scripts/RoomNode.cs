using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public class RoomInspector : MonoBehaviour
    {
        [SerializeField]
        public RoomNode roomData = null;
    }

    [System.Serializable]
    public class RoomNode
    {
        public int index = -1;
        public RoomType roomType = RoomType.Normal;
        public bool[,] gridDesc;
        public List<Grid> gridList = new List<Grid>();
        public Transform transform;
        public Vector3 position;

        public virtual string GetPath()
        {
            return "";
        }

        public bool BeyondBossRoom(int x, int y, RoomGenerator generator)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    if (gridDesc[i, j])
                    {
                        var up = generator.GetGrid(x + i, y + j + 1);
                        if (up != null && up.owner != null && up.owner.roomType == RoomType.Boss)
                        {
                            return true;
                        }

                        var down = generator.GetGrid(x + i, y + j - 1);
                        if (down != null && down.owner != null && down.owner.roomType == RoomType.Boss)
                        {
                            return true;
                        }

                        var left = generator.GetGrid(x + i - 1, y + j);
                        if (left != null && left.owner != null && left.owner.roomType == RoomType.Boss)
                        {
                            return true;
                        }
                        var right = generator.GetGrid(x + i + 1, y + j);
                        if (right != null && right.owner != null && right.owner.roomType == RoomType.Boss)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsSingleConnected(int x, int y, RoomGenerator generator)
        {
            int connectCount = 0;
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    if (gridDesc[i, j])
                    {
                        var up = generator.GetGrid(x + i, y + j + 1);

                        if (up != null && up.owner != null && up.owner != this)
                        {
                            ++connectCount;
                        }

                        var down = generator.GetGrid(x + i, y + j - 1);
                        if (down != null && down.owner != null && down.owner != this)
                        {
                            ++connectCount;
                        }

                        var left = generator.GetGrid(x + i - 1, y + j);
                        if (left != null && left.owner != null && left.owner != this)
                        {
                            ++connectCount;
                        }

                        var right = generator.GetGrid(x + i + 1, y + j);
                        if (right != null && right.owner != null && right.owner != this)
                        {
                            ++connectCount;
                        }
                    }
                }
            }

            return connectCount == 1;
        }

        bool IsConnectToRooms(int x, int y, RoomGenerator generator, HashSet<RoomType> roomTypes)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < colCount; ++j)
                {
                    if (gridDesc[i, j])
                    {
                        var up = generator.GetGrid(x + i, y + j + 1);

                        if (up != null && up.owner != null && roomTypes.Contains(up.owner.roomType))
                        {
                            return true;
                        }

                        var down = generator.GetGrid(x + i, y + j - 1);
                        if (down != null && down.owner != null && roomTypes.Contains(down.owner.roomType))
                        {
                            return true;
                        }

                        var left = generator.GetGrid(x + i - 1, y + j);
                        if (left != null && left.owner != null && roomTypes.Contains(left.owner.roomType))
                        {
                            return true;
                        }

                        var right = generator.GetGrid(x + i + 1, y + j);
                        if (right != null && right.owner != null && roomTypes.Contains(right.owner.roomType))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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
                        var grid = generator.GetGrid(x + i, y + j);
                        if (grid == null || grid.owner != null)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Revert()
        {
            for (int i = 0; i < gridList.Count; ++i)
            {
                var grid = gridList[i];
                grid.owner = null;
                grid.upDoor.doorType = DoorType.Normal;
            }

            gridList = new List<Grid>();
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

        //return is there exist an offset of gridDesc, can place the room
        public bool Place(int x, int y, RoomGenerator generator, bool singleConnected = false, HashSet<RoomType> avoidConnectRoom = null)
        {
            int rowCount = gridDesc.GetLength(0);
            int colCount = gridDesc.GetLength(1);
            for (int offsetX = 0; offsetX < rowCount; ++offsetX)
            {
                for (int offsetY = 0; offsetY < colCount; ++offsetY)
                {
                    //x,y must be filled with a non-empty grid
                    if (gridDesc[offsetX, offsetY] && IsFitSize(x - offsetX, y - offsetY, generator))
                    {
                        if (singleConnected && !IsSingleConnected(x - offsetX, y - offsetY, generator))
                        {
                            continue;
                        }

                        if (avoidConnectRoom != null && IsConnectToRooms(x, y, generator, avoidConnectRoom))
                        {
                            continue;
                        }

                        ApplyRoom(x - offsetX, y - offsetY, generator);
                        MarkUnborderDoors();

                        if (roomType != RoomType.Init && !CheckConnectable(generator))
                        {
                            Revert();
                            return false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckConnectable(RoomGenerator generator)
        {
            var hset = generator.GetConnectedRoom(this);
            return hset.Count > 0;
        }

        public virtual void SetPosition(Vector3 pos)
        {
            position = pos;
        }

        //some rooms may be in special shape
        public virtual void MarkUnborderDoors()
        {

        }

        public virtual void RepositionDoors()
        {

        }

        public virtual void CalculateTransportPos()
        {
            for (int i = 0; i < gridList.Count; ++i)
            {
                var grid = gridList[i];
                //door transport pos
                grid.upDoor.transportPos = grid.upDoor.position - new Vector3(0, 0.6f, 0);
                grid.downDoor.transportPos = grid.downDoor.position + new Vector3(0, 0.6f, 0);
                grid.leftDoor.transportPos = grid.leftDoor.position + new Vector3(0.6f, 0, 0);
                grid.rightDoor.transportPos = grid.rightDoor.position - new Vector3(0.6f, 0, 0);
            }

        }
    }
}
