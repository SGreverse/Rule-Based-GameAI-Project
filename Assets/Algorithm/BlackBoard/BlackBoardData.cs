using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Algorithm.BlackBoard
{

    public class BlackboardData
    {
        public object Value { get; private set; }
        public float Timestamp { get; private set; }

        public BlackboardData(object value, float timestamp)
        {
            this.Value = value;
            this.Timestamp = timestamp;
        }
    }
}
