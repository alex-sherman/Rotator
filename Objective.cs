using Spectrum.Framework;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    [LoadableType]
    public class Objective : GameObject2D
    {
        public RotatorPhysics.Comp<Objective> Physics;
        public event Action OnWin;
        public Objective()
        {
            Physics = new RotatorPhysics.Comp<Objective>();
            Physics.DisableSnapshot = true;
            AddComponent(Physics);
            Physics.OnCollision += Physics_OnCollision;
        }

        private void Physics_OnCollision(GameObject2D arg1, Vector2 arg2, float arg3)
        {
            if (arg1 is Player) OnWin?.Invoke();
        }
    }
}
