using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    [LoadableType]
    public class Bullet : GameObject2D
    {
        public GameObject2D Owner;
        RotatorPhysics.Comp<Bullet> physics;
        public Vector2 Velocity { get => physics.Velocity; set => physics.Velocity = value; }
        public float TimeRotation { get => physics.TimeRotation; set => physics.TimeRotation = value; }
        public Bullet()
        {
            Bounds = new Rectangle(-10, -10, 20, 20);
            Texture = ImageAsset.Blank;
            Color = Color.Black;
            AddComponent(new RotatorPhysics.Comp<Bullet>());
            physics = GetComponent<RotatorPhysics.Comp<Bullet>>();
        }
        public override void Initialize()
        {
            base.Initialize();
            //physics.RecordEvent("Create", Owner);
            physics.OnCollision += Physics_OnCollision;
        }

        private void Physics_OnCollision(GameObject2D go, Vector2 normal, float penetration)
        {
            if (go == Owner || go is Bullet) return;
            if (go is IDamage damage)
            {
                damage.TakeDamage(10);
            }
            if (go is Player) { Destroy(); return; }
            physics.RecordEvent("Destroy", go);
        }

        public static Bullet Shoot(Vector2 position, Vector2 velocity, GameObject2D owner, float timeRotation = 0)
        {
            return owner.Manager.CreateEntity(new InitData<Bullet>(() => new Bullet()
            {
                Velocity = velocity.Normal() * 1500,
                Position = position,
                Owner = owner,
                TimeRotation = timeRotation
            }));
        }
    }
}
