using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public interface IDamagable
    {
        //gets the raw damage and returns what hp the entity is at after being hit
        float TakeDamage(float damage,IDamagable other);
    }
}
