using System.Collections.Generic;
using UnityEngine;

namespace GravityReceipt.World
{
    public static class RoomRegistry
    {
        private static readonly List<RoomVolume> Rooms = new();

        public static void Register(RoomVolume room)
        {
            if (room == null || Rooms.Contains(room))
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
            RoomVolume bestOwn = null;
            var bestOwnSqr = float.MaxValue;
            RoomVolume bestAny = null;
            var bestAnySqr = float.MaxValue;
            for (var i = Rooms.Count - 1; i >= 0; i--)
            {
                var room = Rooms[i];
                if (room == null)
                {
                    Rooms.RemoveAt(i);
                    continue;
                }

                if (!room.Contains(worldPos))
                {
                    continue;
                }

                var d = (worldPos - room.Center).sqrMagnitude;
                if (d < bestAnySqr)
                {
                    bestAnySqr = d;
                    bestAny = room;
                }

                if (room.HasOwnGravity && d < bestOwnSqr)
                {
                    bestOwnSqr = d;
                    bestOwn = room;
                }
            }

            return bestOwn != null ? bestOwn : bestAny;
        }

        public static void Clear()
        {
            Rooms.Clear();
        }
    }
}
