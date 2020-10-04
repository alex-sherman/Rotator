using Spectrum.Framework;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    public class RotatorScene : SceneScreen
    {
        private Camera2D camera;
        public RotatorScene(Camera2D camera) : base(camera) { this.camera = camera; }
        int currentLevel = 0;
        void LoadLevel(int index)
        {
            Manager.ClearEntities();
            var level = Level.Levels[index];
            foreach (var init in level.InitDatas)
            {
                Manager.CreateEntity(init);
            }
            Manager.Where(e => e is Player).Cast<Player>().First().Camera = camera;
            Manager.Where(e => e is Player).Cast<Player>().First().OnDeath += RotatorScene_OnDeath;
            Manager.Where(e => e is Objective).Cast<Objective>().First().OnWin += OnWin;
            RotatorPhysics.RotateRate = level.RotateRate;
            RotatorPhysics.Reset();
        }

        private void RotatorScene_OnDeath()
        {
            LoadLevel(currentLevel);
        }

        private void OnWin()
        {
            currentLevel++;
            currentLevel %= Level.Levels.Count;
            LoadLevel(currentLevel);
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadLevel(currentLevel);
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            using (Inject())
            {
                RotatorPhysics.DetectCollisions(Manager);
            }
        }
    }
}
