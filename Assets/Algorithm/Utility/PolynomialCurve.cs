using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class PolynomialCurve : ResponseCurve
    {
        float a;
        float h;
        float k;
        float b;
        public PolynomialCurve(float a, float h, float k,float b)
        {
            this.a = a;
            this.h = h;
            this.k = k;
            this.b = b;
        }
        public override float Plot(float x)
        {
            float result = (a * Mathf.Pow(x - h, k)) + b;

            return Mathf.Clamp01(result);
        }
    }
}
