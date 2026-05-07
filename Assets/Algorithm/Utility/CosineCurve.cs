using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class CosineCurve : ResponseCurve
    {
        float a;
        float k;
        public CosineCurve(float a,float k)
        {
            this.a = a;
            this.k = k;
        }
        public override float Plot(float x)

        {
            float cosValue = Mathf.Pow(Mathf.Cos(a * x), k);

            return Mathf.Clamp01(cosValue);
        }
    }
}
