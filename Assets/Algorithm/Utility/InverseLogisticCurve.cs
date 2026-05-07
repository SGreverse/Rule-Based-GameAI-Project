using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Algorithm.Utility
{
    public class InverseLogisticCurve:ResponseCurve
    {
        float k;
        float m;

        public InverseLogisticCurve(float k,float m)
        {
            this.k = k;
            this.m = m;
        }

        public override float Plot(float x)
        {
            float denominator = 1f + Mathf.Exp(k * (x - m));
            float value= Mathf.Clamp01(1f / denominator);
            if (value < 0.01f)
            {
                return 0f;
            }

            // Optional: Clamp the top end to absolute 1 if it's over 99%
            if (value > 0.99f)
            {
                return 1f;
            }
            return value;
        }
    }
}
