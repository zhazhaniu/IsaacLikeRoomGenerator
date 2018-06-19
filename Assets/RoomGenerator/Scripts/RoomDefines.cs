using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{

    public class Room_Init : RoomNode
    {
        public Room_Init()
        {
            gridDesc = new bool[1, 2] { { true, true } };
        }

        public override string GetPath()
        {
            return "Rooms/1X2";
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

    public class Room_1X2 : RoomNode
    {
        public Room_1X2()
        {
            gridDesc = new bool[1, 2] { { true, true } };
        }

        public override string GetPath()
        {
            return "Rooms/1X2";
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
            return "Rooms/1111";
        }
    }

    public class Room_2X2_1101 : RoomNode
    {
        public Room_2X2_1101()
        {
            gridDesc = new bool[2, 2] { { true, true }, { false, true } };
        }

        public override string GetPath()
        {
            return "Rooms/1101";
        }
    }

}