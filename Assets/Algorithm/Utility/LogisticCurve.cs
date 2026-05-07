using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{

    public class LogisticCurve : ResponseCurve
    {
        float k;
        float m;
        public LogisticCurve(float k,float m)
        {
            this.k= k;
            this.m= m;
        }
        public override float Plot(float x)
        {
            float denominator = 1f + Mathf.Exp(-k * (x - m));
            float value= Mathf.Clamp01(1f / denominator);

            if (value < 0.01f)
            {
                return 0f;
            }

            if (value > 0.99f)
            {
                return 1f;
            }
            return value;
        }
    }
}
