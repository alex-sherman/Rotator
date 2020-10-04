using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    public class Destroyable : GameObject2D, IDamage
    {
        RotatorPhysics.Comp<Destroyable> physics;
        public Destroyable()
        {
            physics = new RotatorPhysics.Comp<Destroyable>();
            AddComponent(physics);
        }

        public void TakeDamage(int damage)
        {
            physics.RecordEvent("Destroy");
        }
    }
}
