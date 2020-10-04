using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Input;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    [LoadableType]
    public class Player : GameObject2D, IDamage
    {
        public Camera2D Camera;
        public int Health { get; private set; } = 5;
        Vector2 cursor;
        public RotatorPhysics.Comp<Player> Physics;
        public event Action OnDeath;
        public Player()
        {
            Physics = new RotatorPhysics.Comp<Player>() { DisableSnapshot = true };
            AddComponent(Physics);
        }
        public void TakeDamage(int damage)
        {
            Health = Math.Max(Health - damage, 0);
            var width = (int)(Health / 30.0 * 50 + 150);
            Bounds = new Rectangle(-width / 2, -width / 2, width, width);
            if (Health == 0) OnDeath?.Invoke();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            RotatorPhysics.UpdateTime(dt);
            var movement = GetMovement() * dt * 600;
            if (InputState.Current.IsKeyDown("Run")) movement *= 1.8f;
            Position += movement;
            Camera.Position = Position;
            //cursor = Camera.Unproject(Context<SceneScreen>.Current.Bounds, InputState.Current.MousePosition);
            //if (InputState.Current.IsNewKeyPress("Shoot"))
            //    Bullet.Shoot(Position, (cursor - Position).Normal() * 1000, this);
            if (InputState.Current.IsNewKeyPress("Shoot"))
            {
                var tests = Manager.Where(e => e is Enemy).Cast<Enemy>().ToList();
                foreach (var go in tests)
                {
                    if (Bounds.Translate((Point)Position).Intersects(go.Bounds.Translate((Point)go.Position)) && go.Physics.CanUpdate)
                        go.Physics.RecordEvent("Destroy");
                }

            }
        }
        public override void Draw(float gameTime)
        {
            base.Draw(gameTime);
            var timeBar = new Rectangle(-100, 115, (int)(200 * RotatorPhysics.Now / RotatorPhysics.MaxTime), 10);
            Batch3D.Current.DrawPart(GameObject2DPart, CreateTexTransform(timeBar) * World, new MaterialData()
            {
                DiffuseTexture = ImageAsset.Blank.GetTexture(timeBar),
                DiffuseColor = Color.Green,
                DisableLighting = true,
            });

            //Rectangle cursorBox = new Rectangle(-10, -10, 20, 20);
            //Batch3D.Current.DrawPart(GameObject2DPart, CreateTexTransform(cursorBox) * Matrix.CreateTranslation(cursor.X, cursor.Y, 0),
            //    new MaterialData()
            //    {
            //        DiffuseTexture = ImageAsset.Blank.GetTexture(cursorBox),
            //        DiffuseColor = Color.Green,
            //        DisableLighting = true,
            //    });
        }
        Vector2 GetMovement()
        {
            var movement = InputState.Current.GetAxis2D("MoveHorizontal", "MoveVertical");
            foreach (var collision in Physics.Collisions.Where(p => p.Key.Data<bool>("Collide")).Select(p => p.Value))
            {
                var normal = -collision.Normal;
                if (normal.Dot(movement) < 0)
                {
                    movement -= normal * normal.Dot(movement);
                }
            }
            return movement;
        }
    }
}
