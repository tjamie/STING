using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
