using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike
{
    public partial class RoomGenerator
    {
        public static int InfDistance = 1 << 29;

        RoomNode GetBossRoom()
        {
            for (int i = 0; i < roomList.Count; ++i)
            {
                var room = roomList[i];
                if (room.roomType == RoomType.Boss)
                {
                    return room;
                }
            }

            return null;
        }

        //Place the special rooms
        void PlaceInitRoom(int x, int y)
        {
            Room_Init initRoom = new Room_Init();
            initRoom.Place(x, y, this);
            usedGrid += initRoom.gridList.Count;
            roomList.Add(initRoom);
            initRoom.SetPosition(initRoom.gridList[0].position);
        }

        void PlaceShopRoom()
        {
            List<Type> pools = generateConfig.shopRoomPools;
            RoomNode room = null;

            HashSet<RoomType> avoidConnectTo = new HashSet<RoomType>()
            {
                RoomType.Init,
                RoomType.Reward,
                RoomType.Shop,
                RoomType.Boss,
            };

            for (int cnt = 0; cnt < generateConfig.shopRoomCount; ++cnt)
            {
                int rnd = UnityEngine.Random.Range(0, pools.Count);
                room = (RoomNode)Activator.CreateInstance(pools[rnd]);
                room.roomType = RoomType.Shop;

                List<Grid> connectableGrids = GetConnectableGrids();
                for (int i = 0; i < connectableGrids.Count; ++i)
                {
                    var gridIndex = connectableGrids[i].index;
                    if (room.Place(gridIndex.x, gridIndex.y, this, true, avoidConnectTo))
                    {
                        ++curGenerateInfo.shopRoomCount;
                        usedGrid += room.gridList.Count;
                        roomList.Add(room);
                        room.SetPosition(room.gridList[0].position);
                        room.MarkUnborderDoors();
                    }

                    if (generateConfig.shopRoomCount <= curGenerateInfo.shopRoomCount)
                    {
                        break;
                    }
                }
            }

            if (generateConfig.shopRoomCount > curGenerateInfo.shopRoomCount)
            {
                Debug.LogError("Generate Shop Room less than " + generateConfig.shopRoomCount);
            }
        }

        void PlaceBossRoom()
        {
            List<Type> pools = generateConfig.bossRoomPools;
            RoomNode room = null;

            int rnd = UnityEngine.Random.Range(0, pools.Count);
            room = (RoomNode)Activator.CreateInstance(pools[rnd]);
            room.roomType = RoomType.Boss;

            HashSet<RoomType> avoidConnectTo = new HashSet<RoomType>()
            {
                RoomType.Init,
                RoomType.Reward,
                RoomType.Shop,
                RoomType.Hidden,
            };

            List<Grid> connectableGrids = GetConnectableGrids();
            for (int i = 0; i < connectableGrids.Count; ++i)
            {
                var gridIndex = connectableGrids[i].index;
                if (room.Place(gridIndex.x, gridIndex.y, this, true))
                {
                    ++curGenerateInfo.bossRoomCount;
                    usedGrid += room.gridList.Count;
                    roomList.Add(room);
                    room.SetPosition(room.gridList[0].position);
                    room.MarkUnborderDoors();
                }

                if (generateConfig.bossRoomCount <= curGenerateInfo.bossRoomCount)
                {
                    break;
                }
            }

            if (generateConfig.bossRoomCount > curGenerateInfo.bossRoomCount)
            {
                Debug.LogError("Generate Boss Room less than " + generateConfig.bossRoomCount);
            }
        }

        void PlaceSuperHiddenRoom()
        {
            RoomNode bossRoom = GetBossRoom();
            List<Type> pools = generateConfig.spHiddenRoomPools;
            RoomNode room = null;

            int rnd = UnityEngine.Random.Range(0, pools.Count);
            room = (RoomNode)Activator.CreateInstance(pools[rnd]);

            HashSet<RoomType> avoidConnectTo = new HashSet<RoomType>()
            {
                RoomType.Reward,
                RoomType.Shop,
                RoomType.Hidden,
                RoomType.Boss,
            };

            float maxDis = 0f;
            List<Grid> connectableGrids = GetConnectableGrids();
            //make it 0~n : Far from boss room to Near boss room
            connectableGrids.Sort((a, b) =>
            {
                float disA = Vector3.Distance(bossRoom.gridList[0].position, a.position);
                float disB = Vector3.Distance(bossRoom.gridList[0].position, b.position);

                maxDis = Mathf.Max(maxDis, disA);
                maxDis = Mathf.Max(maxDis, disB);

                if (disA < disB) return 1;
                if (disA == disB) return 0;
                return -1;
            });

            for (int i = 0; i < connectableGrids.Count; ++i)
            {
                var gridIndex = connectableGrids[i].index;
                var tmpRoom = (RoomNode)Activator.CreateInstance(pools[rnd]);
                if (tmpRoom.Place(gridIndex.x, gridIndex.y, this, true))
                {
                    if (room.gridList.Count <= 0)
                    {
                        room = tmpRoom;
                    }
                    else
                    {
                        int rndDis = Random(0, (int)maxDis);
                        float disA = Vector3.Distance(bossRoom.gridList[0].position, tmpRoom.gridList[0].position);
                        if (rndDis > disA)
                        {
                            room.Revert();
                            room = tmpRoom;
                        }
                    }
                }
            }

            if (room.gridList.Count > 0)
            {
                room.roomType = RoomType.SuperHidden;
                ++curGenerateInfo.spHiddenRoomCount;
                usedGrid += room.gridList.Count;
                roomList.Add(room);
                room.SetPosition(room.gridList[0].position);
                room.MarkUnborderDoors();
            }
            else 
            {
                Debug.LogError("Generate SPHidden Room failed");
            }
        }

        void PlaceRewardRoom()
        {
            List<Type> pools = generateConfig.rewardRoomPools;
            RoomNode room = null;

            HashSet<RoomType> avoidConnectTo = new HashSet<RoomType>()
            {
                RoomType.Boss,
                RoomType.Shop,
                RoomType.Reward,
            };

            for (int cnt = 0; cnt < generateConfig.rewardRoomCount; ++cnt)
            {
                int rnd = UnityEngine.Random.Range(0, pools.Count);
                room = (RoomNode)Activator.CreateInstance(pools[rnd]);
                room.roomType = RoomType.Reward;

                List<Grid> connectableGrids = GetConnectableGrids();
                for (int i = 0; i < connectableGrids.Count; ++i)
                {
                    var gridIndex = connectableGrids[i].index;
                    if (room.Place(gridIndex.x, gridIndex.y, this, true, avoidConnectTo))
                    {
                        ++curGenerateInfo.rewardRoomCount;
                        usedGrid += room.gridList.Count;
                        roomList.Add(room);
                        room.SetPosition(room.gridList[0].position);
                        room.MarkUnborderDoors();
                    }

                    if (generateConfig.rewardRoomCount <= curGenerateInfo.rewardRoomCount)
                    {
                        break;
                    }
                }
            }

            if (generateConfig.rewardRoomCount > curGenerateInfo.rewardRoomCount)
            {
                Debug.LogError("Generate Reward Room less than " + generateConfig.rewardRoomCount);
            }
        }


        int[,] roomDistanceMap = null;
        void CalculateRoomDistance()
        {
            //needs room index
            SetRoomIndex();

            roomDistanceMap = new int[roomList.Count, roomList.Count];
            
            int row = roomDistanceMap.GetLength(0);
            int col = roomDistanceMap.GetLength(1);
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < col; ++j)
                {
                    roomDistanceMap[i, j] = InfDistance;
                }
            }

            //init distance with room & rooms around it
            for (int i = 0; i < roomList.Count; ++i)
            {
                roomDistanceMap[i, i] = 0;
                var hset = GetConnectedRoom(roomList[i]);
                foreach (var item in hset)
                {
                    roomDistanceMap[i, item.index] = 1;
                    roomDistanceMap[item.index, i] = 1;
                }
            }

            //floyd algorithm
            int len = roomList.Count;
            for (int i = 0; i < len; ++i)
            {
                for (int j = i + 1; j < len; ++j)
                {
                    for (int k = 0; k < len; ++k)
                    {
                        if (k == i || k == j)
                        {
                            continue;
                        }

                        roomDistanceMap[i, j] = Mathf.Min(roomDistanceMap[i, k] + roomDistanceMap[k, j], roomDistanceMap[i, j]);
                    }
                }
            }
        }

        //Place a hidden room that can reduce the other 2 rooms' "Graph Distance"
        void PlaceHiddenRoom()
        {
            List<Type> pools = generateConfig.hiddenRoomPools;
            RoomNode room = null;
            int rnd = UnityEngine.Random.Range(0, pools.Count);
            room = (RoomNode)Activator.CreateInstance(pools[rnd]);
            room.roomType = RoomType.Hidden;

            HashSet<RoomType> avoidConnectTo = new HashSet<RoomType>()
            {
                RoomType.Boss,
            };

            CalculateRoomDistance();
            int canReduceLength = 0;
            List<Grid> connectableGrids = GetConnectableGrids();
            for (int i = 0; i < connectableGrids.Count; ++i)
            {
                var gridIndex = connectableGrids[i].index;
                var tmpRoom = (RoomNode)Activator.CreateInstance(pools[rnd]);
                if (tmpRoom.Place(gridIndex.x, gridIndex.y, this, false, avoidConnectTo))
                {
                    var hset = GetConnectedRoom(tmpRoom);
                    RoomNode[] rooms = new RoomNode[hset.Count];
                    hset.CopyTo(rooms);

                    if (room.gridList.Count <= 0)
                    {
                        room = tmpRoom;

                        for (int fromRoom = 0; fromRoom < rooms.Length; ++fromRoom)
                        {
                            for (int toRoom = fromRoom + 1; toRoom < rooms.Length; ++toRoom)
                            {
                                canReduceLength = Mathf.Max(canReduceLength, roomDistanceMap[fromRoom, toRoom]);
                            }
                        }
                    }
                    else
                    {
                        int tmpCanreduce = 0;
                        for (int fromRoom = 0; fromRoom < rooms.Length; ++fromRoom)
                        {
                            for (int toRoom = fromRoom + 1; toRoom < rooms.Length; ++toRoom)
                            {
                                tmpCanreduce = Mathf.Max(tmpCanreduce, roomDistanceMap[fromRoom, toRoom]);
                            }
                        }

                        if (tmpCanreduce > canReduceLength)
                        {
                            room.Revert();
                            room = tmpRoom;
                            canReduceLength = tmpCanreduce;
                        }
                        else
                        {
                            tmpRoom.Revert();
                        }
                        
                    }
                }
            }

            if (room.gridList.Count > 0)
            {
                room.roomType = RoomType.Hidden;
                ++curGenerateInfo.hiddenRoomCount;
                usedGrid += room.gridList.Count;
                roomList.Add(room);
                room.SetPosition(room.gridList[0].position);
                room.MarkUnborderDoors();
            }
            else
            {
                Debug.LogError("Generate Hidden Room failed");
            }
        }
    }
}

