using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public enum RoomType
    {
        Init,
        Normal,
        Hidden,
        SuperHidden,
        Boss,
        Shop,
        Reward,
        Challenge,
    }

    public class Data
    {
        public static Vector2 GridSize = new Vector2(12.80f, 7.20f);
        public static Vector2 GroundSize = new Vector2(10.2f, 5.4f);
        public static float DoorHeight = 1.2f;

        public static Dictionary<DoorType, string> doorRes = new Dictionary<DoorType, string>()
        {
            {DoorType.Normal, "Doors/NormalDoor" },
            {DoorType.Hidden, "Doors/NormalDoor" },
            {DoorType.Shop, "Doors/NormalDoor" },
            {DoorType.Reward, "Doors/NormalDoor" },
            {DoorType.Challenge, "Doors/NormalDoor" },
            {DoorType.Boss, "Doors/BossDoor" },
        };

        public static List<Type> normalPools = new List<Type>()
        {
            typeof(Room_1X1),
            typeof(Room_1X1),
            typeof(Room_1X1),
            typeof(Room_1X1),
            typeof(Room_1X1),
            typeof(Room_L_0111),
            typeof(Room_L_1011),
            typeof(Room_L_1101),
            typeof(Room_L_1110),
            typeof(Room_2X2),
        };
        
        public static List<Type> bossRoomPools = new List<Type>()
        {
            typeof(Room_1X1),
        };

        public static List<Type> shopRoomPools = new List<Type>()
        {
            typeof(Room_1X1),
        };

        public static List<Type> rewardRoomPools = new List<Type>()
        {
            typeof(Room_1X1),
        };

        public static List<Type> hiddenRoomPools = new List<Type>()
        {
            typeof(Room_1X1),
        };
    }

    public class Room_Init : RoomNode
    {
        public Room_Init()
        {
            gridDesc = new bool[1, 1] { { true } };
            roomType = RoomType.Init;
        }

        public override string GetPath()
        {
            return "Rooms/1X1";
        }
    }

    public class Room_1X1 : RoomNode
    {
        public Room_1X1()
        {
            gridDesc = new bool[1, 1] { { true } };
        }

        public override string GetPath()
        {
            return "Rooms/1X1";
        }
    }

    public class Room_L_0111 : RoomNode
    {
        public Room_L_0111()
        {
            gridDesc = new bool[2, 2] { { false, true }, { true, true } };
        }

        public override string GetPath()
        {
            return "Rooms/L_0111";
        }

        public override void SetPosition(Vector3 pos)
        {
            pos = new Vector3(gridList[0].position.x - Data.GridSize.x, gridList[0].position.y, 0);
            base.SetPosition(pos);
        }
    }

    public class Room_L_1011 : RoomNode
    {
        public Room_L_1011()
        {
            gridDesc = new bool[2, 2] { { true, false }, { true, true } };
        }

        public override string GetPath()
        {
            return "Rooms/L_1011";
        }

    }

    public class Room_L_1101 : RoomNode
    {
        public Room_L_1101()
        {
            gridDesc = new bool[2, 2] { { true, true }, { false, true } };
        }

        public override string GetPath()
        {
            return "Rooms/L_1101";
        }

    }

    public class Room_L_1110 : RoomNode
    {
        public Room_L_1110()
        {
            gridDesc = new bool[2, 2] { { true, true }, { true, false } };
        }

        public override string GetPath()
        {
            return "Rooms/L_1110";
        }

    }

    public class Room_2X2 : RoomNode
    {
        public Room_2X2()
        {
            gridDesc = new bool[2, 2] { { true, true }, { true, true } };
        }

        public override string GetPath()
        {
            return "Rooms/2X2";
        }
    }

}