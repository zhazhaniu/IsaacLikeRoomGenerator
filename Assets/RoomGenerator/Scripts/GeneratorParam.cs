using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public struct RoomGenerateParam
    {
        public int normalRoomCount;
        public int bossRoomCount;
        public int shopRoomCount;
        public int hiddenRoomCount;
        public int rewardRoomCount;
        //random rnd = [1, deep], cut current branch when rnd < curParam
        public int randomCutAtDeep;
        public int cutParam;

        public List<Type> bossRoomPools;
        public List<Type> shopRoomPools;
        public List<Type> rewardRoomPools;
        public List<Type> hiddenRoomPools;
        public List<Type> normalPools;
    }
}

