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
        public int rewardRoomCount;

        //only support generate 1 hidden & super hidden room
        public int hiddenRoomCount;
        public int spHiddenRoomCount;
        
        //random rnd = [1, deep], cut current branch when rnd < curParam
        public int randomCutAtDeep;
        public int cutParam;

        public List<Type> bossRoomPools;
        public List<Type> shopRoomPools;
        public List<Type> rewardRoomPools;
        public List<Type> hiddenRoomPools;
        public List<Type> spHiddenRoomPools;
        public List<Type> normalPools;
    }
}

