using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class ExponentialCurve : ResponseCurve
    {
        float k;
        public ExponentialCurve(float k)
        {
            this.k = k;
        }
        public override float Plot(float x)
        {
            return Mathf.Exp(-k*x);
        }
    }
}
