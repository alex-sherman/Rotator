using Replicate;
using Replicate.MetaData;
using Replicate.Serialization;
using Spectrum.Framework;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Network;
using Spectrum.Framework.Physics.Collision;
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
            var destoryable = SnapshotModel.Add(typeof(Destroyable), addMembers: false);
        }
        public static float GlobalTimeRotation = 0;
        public static float RotateRate = 1.0f / 40;
        private static float LinearTimePassed = 0;
        private static float MaxLinearTime => RotationTimePassed(0, 0.5f);
        public static float MaxTime => 2 * MaxLinearTime;
        public static float RotationTimePassed(float a, float b)
            => (float)(1 / (RotateRate * Math.PI) * (Math.Sin(Math.PI * b) - Math.Sin(Math.PI * a)));
        public static void Reset()
        {
            LinearTimePassed = 0;
            GlobalTimeRotation = 0;
        }
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
        public static void DetectCollisions(EntityManager manager)
        {
            var tests = manager.Where(e => e is GameObject2D).Cast<GameObject2D>().ToList();
            for (int i = 0; i < tests.Count; i++)
            {
                for (int j = 0; j < tests.Count; j++)
                {
                    if (i == j) continue;
                    var testGo = tests[i];
                    var comp = testGo.GetComponent<ICollisionHandler>();
                    var go = tests[j];
                    if (comp == null || go.Data<bool>("Destroyed") || testGo.Data<bool>("Destroyed")) continue;
                    var A = comp.P.Bounds.Translate((Point)comp.P.Position);
                    var B = go.Bounds.Translate((Point)go.Position);
                    if (A.Intersects(B))
                    {
                        var penetration = float.MaxValue;
                        var normal = Vector2.Zero;
                        if (A.YMax > B.YMin && A.YMax - B.YMin < penetration)
                        {
                            penetration = A.YMax - B.YMin;
                            normal = new Vector2(0, 1);
                        }
                        if (B.YMax > A.YMin && B.YMax - A.YMin < penetration)
                        {
                            penetration = B.YMax - A.YMin;
                            normal = new Vector2(0, -1);
                        }
                        if (A.Right > B.Left && A.Right - B.Left < penetration)
                        {
                            penetration = A.Right - B.Left;
                            normal = new Vector2(1, 0);
                        }
                        if (B.Right > A.Left && B.Right - A.Left < penetration)
                        {
                            penetration = B.Right - A.Left;
                            normal = new Vector2(-1, 0);
                        }
                        comp.HandleCollision(go, normal, penetration);
                        go.GetComponent<ICollisionHandler>()?.HandleCollision(comp.P, -normal, -penetration);
                    }
                }
            }
        }
        public interface ICollisionHandler
        {
            bool HandleCollision(GameObject2D go, Vector2 normal, float penetration);
            GameObject2D P { get; }
        }

        public class Comp<T> : Component<T>, IUpdate, ICollisionHandler where T : GameObject2D
        {
            public struct EventData
            {
                public string Type;
                public GameObject2D Cause;
                public Stream stream;
            }
            public class CollisionData { public Vector2 Normal; public float Penetration; public bool Colliding; }
            public Dictionary<GameObject2D, CollisionData> Collisions = new Dictionary<GameObject2D, CollisionData>();
            GameObject2D ICollisionHandler.P => P;
            // TODO: This doesn't work when non-zero
            public bool DisableSnapshot = false;
            public float TimeRotation = 0;
            public Vector2 Velocity;
            public float TimeRate => (float)Math.Cos(Math.PI * (GlobalTimeRotation - TimeRotation));
            public float DT(float dt) => LinearTimePassed < MaxLinearTime ? dt : dt * TimeRate;
            private List<(float time, EventData @event)> events = new List<(float time, EventData @event)>();
            private float lastSnapShot = 0;
            bool ignoreCollision = false;
            public override void Initialize(Entity e)
            {
                base.Initialize(e);
                lastSnapShot = (float)SpecTime.Time;
            }
            public bool IsColliding(GameObject2D go) => Collisions.ContainsKey(go);
            private bool lastReversed = false;
            public void Update(float dt)
            {
                var endedCollisions = Collisions.Where(p => !p.Value.Colliding).Select(p => p.Key).ToList();
                foreach (var ended in endedCollisions)
                    Collisions.Remove(ended);
                foreach (var key in Collisions.Keys.ToList())
                    Collisions[key].Colliding = false;
                ignoreCollision = false;
                if (!Destroyed)
                {
                    P.Position += (Velocity * DT(dt));
                }
                if (!DisableSnapshot)
                {
                    if (TimeRate > 0)
                    {
                        if (SpecTime.Time > lastSnapShot + 0.1f)
                        {
                            lastSnapShot = (float)SpecTime.Time;
                            events.Add((Now, new EventData { stream = new BinaryGraphSerializer(SnapshotModel).Serialize(P) }));
                        }
                    }
                    else
                    {
                        while (events.Any() && ((lastReversed && (TimeRate > 0)) || Now <= events.Last().time))
                        {
                            var eventData = events.Last().@event;
                            events.RemoveAt(events.Count - 1);
                            if (eventData.stream != null)
                                new BinaryGraphSerializer(SnapshotModel).Deserialize(typeof(T), (Stream)eventData.stream, P);
                            if (eventData.Type != null && !(eventData.Cause?.Destroying ?? false))
                                HandleEvent(eventData);
                        }
                    }
                }
                lastReversed = TimeRate < 0;
            }
            public event Action<GameObject2D, Vector2, float> OnCollision;
            public bool HandleCollision(GameObject2D go, Vector2 normal, float penetration)
            {
                if (ignoreCollision) return false;
                if (!Collisions.ContainsKey(go))
                {
                    OnCollision?.Invoke(go, normal, penetration);
                    Collisions[go] = new CollisionData();
                }
                Collisions[go].Colliding = true;
                Collisions[go].Penetration = penetration;
                Collisions[go].Normal = normal;
                return true;
            }
            public event Action<EventData> OnEvent;
            public virtual void HandleEvent(EventData e)
            {
                switch (e.Type)
                {
                    case "Create":
                        P.Destroy();
                        break;
                    case "Destroy":
                        Destroyed = false;
                        break;
                    default:
                        OnEvent?.Invoke(e);
                        break;
                }
            }
            public void RecordEvent(string type, GameObject2D cause = null)
            {
                events.Add((Now, new EventData { Type = type, Cause = cause }));
                if (type == "Destroy")
                {
                    Destroyed = true;
                }
            }
            public bool Destroyed
            {
                get => P.Data<bool>("Destroyed");
                set
                {
                    if (!value) ignoreCollision = true;
                    P.DrawEnabled = !value;
                    P.SetData("Destroyed", value);
                }
            }
            public bool CanUpdate => !Destroyed && TimeRate > 0;

        }
    }
}
