using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace STING.Structs
{
    internal readonly struct BoxCoordinates
    {
        internal readonly Coordinates southwest;
        internal readonly Coordinates northeast;

        internal BoxCoordinates(Coordinates southwest, Coordinates northeast)
        {
            this.southwest = southwest;
            this.northeast = northeast;
        }

        internal bool IsNonzero()
        {
            // In WGS84, this essentially interprets any area that's less than 1 square foot or so on the Earth's surface as zero
            float area = (this.northeast.latitude - this.southwest.latitude) * (this.northeast.longitude - this.southwest.longitude);
            bool result = area * area > float.Epsilon;
            return result;
        }
    }
}
