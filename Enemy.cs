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
    public class Enemy : GameObject2D, IDamage
    {
        Player target;
        public RotatorPhysics.Comp<Enemy> Physics;
        [Replicate]
        public bool canShoot = false;
        [Replicate]
        public float shootPeriod = 1.5f;
        [Replicate]
        public float shootTimer = 0;

        public int Health { get; set; }

        public Enemy()
        {
            Physics = new RotatorPhysics.Comp<Enemy>();
            AddComponent(Physics);
        }
        public override void Initialize()
        {
            base.Initialize();
            target = Manager.Select(e => e as Player).Where(e => e != null).First();
            Physics.OnEvent += Physics_OnEvent;
        }

        private void Physics_OnEvent(RotatorPhysics.Comp<Enemy>.EventData eventData)
        {
            if (eventData.Type == "Shoot")
            {
                shootTimer = shootPeriod;
                eventData.Cause.Destroy();
            }
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
            shootTimer += Physics.DT(dt);
            if (shootTimer < 0) shootTimer = 0;
            if (shootTimer >= shootPeriod) { canShoot = true; shootTimer = shootPeriod; }
            if (Physics.CanUpdate)
            {
                if (canShoot)
                {
                    var bullet = Bullet.Shoot(Position, (target.Position - Position).Normal(), this);
                    Physics.RecordEvent("Shoot", bullet);
                    canShoot = false;
                    shootTimer = 0;
                }
            }
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0) { Health = 0; Physics.RecordEvent("Destroy"); }
        }
    }
}
