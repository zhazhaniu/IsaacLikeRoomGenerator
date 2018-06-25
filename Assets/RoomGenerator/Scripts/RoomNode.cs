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
            for (int offsetX = 0; offsetX < rowCount; ++offsetX)
            {
                for (int offsetY = 0; offsetY < colCount; ++offsetY)
                {
                    //x,y must be filled with a non-empty grid
                    if (gridDesc[offsetX, offsetY] && IsFitSize(x - offsetX, y - offsetY, generator) && !BeyondBossRoom(x - offsetX, y - offsetY, generator))
                    {
                        ApplyRoom(x - offsetX, y - offsetY, generator);
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

        //some rooms may be in special shape
        public virtual void MarkUnborderDoors()
        {

        }
    }
}
