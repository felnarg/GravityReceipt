using System.Collections.Generic;
using UnityEngine;

namespace GravityReceipt.World
{
    public static class RoomRegistry
    {
        private static readonly List<RoomVolume> Rooms = new();

        public static void Register(RoomVolume room)
        {
            if (room is not { } || Rooms.Contains(room))
            {
                return;
            }

            Rooms.Add(room);
        }

        public static void Unregister(RoomVolume room)
        {
            Rooms.Remove(room);
        }

        public static RoomVolume FindRoom(Vector3 worldPos)
        {
            RoomVolume best = null;
            var bestSqr = float.MaxValue;
            for (var i = 0; i < Rooms.Count; i++)
            {
                var room = Rooms[i];
                if (room is not { } || !room.Contains(worldPos))
                {
                    continue;
                }

                var d = (worldPos - room.Center).sqrMagnitude;
                if (d >= bestSqr)
                {
                    continue;
                }

                bestSqr = d;
                best = room;
            }

            return best;
        }

        public static void Clear()
        {
            Rooms.Clear();
        }
    }
}
