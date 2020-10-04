using Replicate;
using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    [LoadableType]
    public class Enemy : GameObject2D
    {
        Player target;
        RotatorPhysics.Comp<Enemy> physics;
        [Replicate]
        public bool canShoot = false;
        [Replicate]
        public float shootPeriod = 1.5f;
        [Replicate]
        public float shootTimer = 0;
        public Enemy()
        {
            physics = new RotatorPhysics.Comp<Enemy>();
            AddComponent(physics);
        }
        public override void Initialize()
        {
            base.Initialize();
            target = Manager.Select(e => e as Player).Where(e => e != null).First();
        }
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            var shootBar = new Rectangle(-100, 115, (int)(200 * shootTimer / shootPeriod), 10);
            Batch3D.Current.DrawPart(GameObject2DPart, CreateTexTransform(shootBar) * World, new MaterialData()
            {
                DiffuseTexture = ImageAsset.Blank.GetTexture(shootBar),
                DiffuseColor = Color.Green,
                DisableLighting = true,
            });
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            if (!canShoot)
            {
                shootTimer += physics.DT(dt);
            }
            if (shootTimer < 0) shootTimer = 0;
            if (shootTimer >= shootPeriod) canShoot = true;
            if (physics.CanUpdate)
            {
                if (canShoot)
                {
                    Bullet.Shoot(Position, (target.Position - Position).Normal() * 1000, this);
                    canShoot = false;
                    shootTimer = 0;
                }
            }
        }
    }
}
