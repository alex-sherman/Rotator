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
        public int Health { get; private set; } = 100;
        Vector2 cursor;

        public int TakeDamage(int damage)
        {
            Health = Math.Max(Health - damage, 0);
            var width = (int)(Health / 100.0 * 50 + 150);
            Bounds = new Rectangle(-width / 2, -width / 2, width, width);
            return Health;
        }

        public override void Update(float dt)
        {
            RotatorPhysics.UpdateTime(dt);
            var movement = GetMovement() * dt * 600;
            if (InputState.Current.IsKeyDown("Run")) movement *= 1.8f;
            Position += movement;
            Camera.Position = Position;
            cursor = Camera.Unproject(Context<SceneScreen>.Current.Bounds, InputState.Current.MousePosition);
            if (InputState.Current.IsKeyDown("Shoot"))
                Bullet.Shoot(Position, (cursor - Position).Normal() * 1000, this);
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
            
            Rectangle cursorBox = new Rectangle(-10, -10, 20, 20);
            Batch3D.Current.DrawPart(GameObject2DPart, CreateTexTransform(cursorBox) * Matrix.CreateTranslation(cursor.X, cursor.Y, 0),
                new MaterialData()
                {
                    DiffuseTexture = ImageAsset.Blank.GetTexture(cursorBox),
                    DiffuseColor = Color.Green,
                    DisableLighting = true,
                });
        }
        Vector2 GetMovement()
        {
            return InputState.Current.GetAxis2D("MoveHorizontal", "MoveVertical");
        }
    }
}
