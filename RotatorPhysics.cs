using Replicate;
using Replicate.MetaData;
using Replicate.Serialization;
using Spectrum.Framework;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    public class RotatorPhysics
    {
        public static ReplicationModel SnapshotModel = new ReplicationModel(false);
        static RotatorPhysics()
        {
            SnapshotModel.Add(typeof(Vector2)).AddMember("X").AddMember("Y");
            var enemy = SnapshotModel.Add(typeof(Enemy), addMembers: false);
            enemy.AddMember("canShoot");
            enemy.AddMember("shootTimer");
            enemy.AddMember("Position");
            var bullet = SnapshotModel.Add(typeof(Bullet), addMembers: false);
            bullet.AddMember("Position");
        }
        public static float GlobalTimeRotation = 0;
        public static float RotateRate = 1.0f / 10;
        private static float LinearTimePassed = 0;
        private static float MaxLinearTime => RotationTimePassed(0, 0.5f);
        public static float MaxTime = 2 * MaxLinearTime;
        public static float RotationTimePassed(float a, float b)
            => (float)(1 / (RotateRate * Math.PI) * (Math.Sin(Math.PI * b) - Math.Sin(Math.PI * a)));
        public static void UpdateTime(float dt)
        {
            if (LinearTimePassed < MaxLinearTime)
            {
                if (MaxLinearTime - LinearTimePassed > dt)
                {
                    LinearTimePassed += dt;
                    return;
                }
                else
                {
                    dt -= MaxLinearTime - LinearTimePassed;
                    LinearTimePassed = MaxLinearTime;
                }
            }
            GlobalTimeRotation += RotateRate * dt;
        }
        public static float Now => LinearTimePassed + RotationTimePassed(0, GlobalTimeRotation);

        public class Comp<T> : Component<T>, IUpdate where T : GameObject2D
        {
            public float TimeRotation = 0;
            public Vector2 Velocity;
            public float TimeRate => (float)Math.Cos(Math.PI * (GlobalTimeRotation - TimeRotation));
            public float DT(float dt) => LinearTimePassed < MaxLinearTime ? dt : dt * TimeRate;
            private List<(float time, Stream stream)> snapshots = new List<(float time, Stream stream)>();
            private float lastSnapShot = 0;
            private float creation;
            public override void Initialize(Entity e)
            {
                base.Initialize(e);
                lastSnapShot = (float)SpecTime.Time;
                creation = Now;
            }
            public void Update(float dt)
            {
                if (Now < creation) { P.Destroy(); return; }
                P.Position += (Velocity * DT(dt));
                if (CanUpdate)
                {
                    if (SpecTime.Time > lastSnapShot + 0.1f)
                    {
                        lastSnapShot = (float)SpecTime.Time;
                        snapshots.Add((Now, new BinaryGraphSerializer(SnapshotModel).Serialize(P)));
                    }
                }
                else
                {
                    Stream snapshot = null;
                    while (snapshots.Any() && Now <= snapshots.Last().time)
                    {
                        snapshot = snapshots.Last().stream;
                        snapshots.RemoveAt(snapshots.Count - 1);
                    }
                    if (snapshot != null)
                    {
                        new BinaryGraphSerializer(SnapshotModel).Deserialize(typeof(T), snapshot, P);
                    }
                }
            }
            public bool CanUpdate => TimeRate > 0;
        }
    }
}
