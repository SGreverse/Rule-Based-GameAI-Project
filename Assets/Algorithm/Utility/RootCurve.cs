using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class SquareRootCurve : ResponseCurve
    {
        float a;
        float b;
        public SquareRootCurve(float a, float b)
        {
            this.a = a;
            this.b = b;
        }
        public override float Plot(float x)
        {
            return a * (Mathf.Sqrt(x))+b;
        }
    }
}
