using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class InverseRootCurve : ResponseCurve
    {
        float a;
        float b;
        public InverseRootCurve(float a,float b)
        {
            this.a = a; 
            this.b = b;
        }
        public override float Plot(float x)
        {
            return b+ a * (Mathf.Sqrt(1-x)) ;
        }
    }
}
