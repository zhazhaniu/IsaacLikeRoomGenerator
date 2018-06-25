using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public enum DoorType
    {
        //close by room, or no room can associate
        None,
        Normal,
        Hidden,
        SuperHidden,
        Boss,
        Shop,
        Reward,
        Challenge,
    }

    public class DoorInspector : MonoBehaviour
    {
        [SerializeField]
        public DoorNode doorData = null;
    }

    [System.Serializable]
    public class DoorNode
    {
        //common, hidden, boss, etc...
        public DoorType doorType = DoorType.Normal;
        public Transform transform;
        public Vector3 position;
        public DoorNode associateDoor = null;
        public Grid grid = null;

        public void Associate(DoorNode otherDoor)
        {
            associateDoor = otherDoor;
            otherDoor.associateDoor = this;
            if (otherDoor.grid.owner.roomType == RoomType.Boss)
            {
                doorType = DoorType.Boss;
            }
        }
    }
}