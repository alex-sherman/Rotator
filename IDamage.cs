using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rotator
{
    interface IDamage
    {
        int TakeDamage(int damage);
        int Health { get; }
    }
}
