using Microsoft.Xna.Framework.Input;
using Spectrum;
using Spectrum.Framework;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Input;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Screens.InputElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    public static class RotatorEntry
    {
        public static void Main(string[] args) { Entry<RotatorGame>.Main(args); }
    }
    [Serializable]
    public class RotatorGame : SpectrumGame
    {
        protected override void Initialize()
        {
            base.Initialize();
            var camera = new Camera2D();
            var layout = Root.AddElement(new LinearLayout());
            layout.BackgroundColor = "0xffffff";
            layout.AddElement(new TextElement() { TextSource = () => RotatorPhysics.Now.ToString("0.00") });
            layout.Height = 1.0;
            layout.Width = 1.0;
            var scene = layout.AddElement(new SceneScreen(camera));
            scene.AddElement(new DebugPrinter());
            camera.Position = new Vector2(0, 0);
            var manager = scene.Manager;
            var player = manager.CreateEntity(new InitData<Player>(() => new Player()
            {
                Texture = ImageAsset.Blank,
                Color = Color.Blue,
                Bounds = new Rectangle(-100, -100, 200, 200),
            }));
            player.Camera = camera;
            manager.CreateEntity(new InitData<Enemy>(() => new Enemy()
            {
                Texture = ImageAsset.Blank,
                Color = Color.Red,
                Bounds = new Rectangle(-100, -100, 200, 200),
                Position = new Vector2(-500, -500),
            }));
            InputLayout.Default.Axes1["MoveHorizontal"] = new Axis1(new KeyboardAxis(Keys.D, Keys.A));
            InputLayout.Default.Axes1["MoveVertical"] = new Axis1(new KeyboardAxis(Keys.W, Keys.S));
            InputLayout.Default.Axes1["MoveHorizontal"].Axes.Add(new GamepadAxis(GamepadAxisType.LeftStickHorizontal));
            InputLayout.Default.Axes1["MoveVertical"].Axes.Add(new GamepadAxis(GamepadAxisType.LeftStickVertical));
            InputLayout.AddBind("Run", Keys.LeftShift);
            InputLayout.AddBind("Shoot", new KeyBind(0));
            layout.RegisterHandler(Keys.F1, (_) => { Game.Debug ^= true; });
            PostProcessEffect.Enabled = false;
        }
    }
}
