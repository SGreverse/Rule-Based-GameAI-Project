using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.Utility
{
    public class UtilityFactor
    {
        public string Name; // Added so the debugger can read it
        public ResponseCurve Curve;
        public float Weight;
        public Func<EnemyManager, float> ParameterFetcher;
    }
}
