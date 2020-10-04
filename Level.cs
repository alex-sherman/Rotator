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
    public class Level
    {
        public List<InitData> InitDatas = new List<InitData>();
        public float RotateRate = 1 / 5f;
        public static List<Level> Levels = new List<Level>();
        static Level()
        {
            var enemy = new InitData<Enemy>(() => new Enemy()
            {
                Texture = ImageAsset.Blank,
                Color = Color.Red,
                Bounds = new Rectangle(-100, -100, 200, 200),
            }).ToImmutable();
            var objective = new InitData<Objective>(() => new Objective()
            {
                Texture = ImageAsset.Blank,
                Color = "gold",
                Bounds = new Rectangle(-75, -75, 150, 150),
            }).ToImmutable();
            var wall = new InitData<GameObject2D>(() => new GameObject2D
            {
                Texture = ImageAsset.Blank,
                Color = "brown",
            }).SetData("Collide", true).ToImmutable();
            var destroyable = new InitData<Destroyable>(() => new Destroyable
            {
                Texture = ImageAsset.Blank,
                Color = "gray",
            }).SetData("Collide", true).ToImmutable();
            var player = new InitData<Player>(() => new Player()
            {
                Texture = ImageAsset.Blank,
                Color = Color.Blue,
                Bounds = new Rectangle(-100, -100, 200, 200),
            }).ToImmutable();
            Levels.Add(new Level()
            {
                InitDatas = {
                    player,
                    //enemy.Set(e => e.Position, new Vector2(-500, -500)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 2000))
                        .Set(e => e.Position, new Vector2(200, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 2000))
                        .Set(e => e.Position, new Vector2(-200, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-200, -50, 400, 100))
                        .Set(e => e.Position, new Vector2(0, -150)),
                    objective.Set(e => e.Position, new Vector2(0, 1850))
                }
            });
            Levels.Add(new Level()
            {
                RotateRate = 1 / 5f,
                InitDatas = {
                    player,
                    enemy.Set(e => e.Position, new Vector2(0, 1000)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 2000))
                        .Set(e => e.Position, new Vector2(400, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 2000))
                        .Set(e => e.Position, new Vector2(-400, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-400, -50, 800, 100))
                        .Set(e => e.Position, new Vector2(0, -150)),
                    objective.Set(e => e.Position, new Vector2(0, 1850))
                }
            });
            Levels.Add(new Level()
            {
                RotateRate = 1 / 5f,
                InitDatas = {
                    player,
                    enemy.Set(e => e.Position, new Vector2(0, 1000)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 3000))
                        .Set(e => e.Position, new Vector2(400, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-50, -150, 100, 3000))
                        .Set(e => e.Position, new Vector2(-400, 0)),
                    wall.Set(e => e.Bounds, new Rectangle(-400, -50, 800, 100))
                        .Set(e => e.Position, new Vector2(0, -150)),
                    destroyable.Set(e => e.Bounds, new Rectangle(-400, -50, 800, 100))
                        .Set(e => e.Position, new Vector2(0, 2600)),
                    objective.Set(e => e.Position, new Vector2(0, 2850))
                }
            });
        }
    }
}
