using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class LinearCurve : ResponseCurve
    {
        float a;
        float b;
        public LinearCurve(float a,float b)
        {
            this.a=a; 
            this.b=b;
        }
        public override float Plot(float x)
        {
            float result = (a * x) + b;
            return Mathf.Clamp01(result);
        }
    }
}
