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
        public Bullet()
        {
            Bounds = new Rectangle(-10, -10, 20, 20);
            Texture = ImageAsset.Blank;
            Color = Color.Black;
            AddComponent(new RotatorPhysics.Comp<Bullet>());
            physics = GetComponent<RotatorPhysics.Comp<Bullet>>();
        }
        public static Bullet Shoot(Vector2 position, Vector2 velocity, GameObject2D owner)
        {
            return owner.Manager.CreateEntity(new InitData<Bullet>(() => new Bullet()
            {
                Velocity = velocity,
                Position = position,
                Owner = owner,
            }));
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            var tests = Manager.Where(e => e is GameObject2D && e != Owner && e is IDamage).Cast<GameObject2D>();
            foreach (var go in tests)
            {
                if (Bounds.Translate((Point)Position).Intersects(go.Bounds.Translate((Point)go.Position)))
                {
                    ((IDamage)go).TakeDamage(10);
                    Destroy();
                    return;
                }
            }
        }
    }
}
