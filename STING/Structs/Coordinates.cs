using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STING.Structs
{
    internal readonly struct Coordinates
    {
        // 6 decimals gets you within a foot or so on the Earth's surface
        public readonly float longitude;
        public readonly float latitude;

        internal Coordinates(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
