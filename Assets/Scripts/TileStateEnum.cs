using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtRng.MobileTTA {
    public enum TileStateEnum {
      CanNotAccess
    , CanAttack
    , CanMove
    , CanPassThrough
    , CanStay
    , Pending
        //
        //
    }
    public enum TileTraversalEnum {
        None     =  0x0,
        //
        CanWalk  = 0x01,  // 0001
        CanClimb = 0x02,  // 0010
        CanFly   = 0x04,  // 0100
        //
        WalkAndClimb = 0x03, // 0011
        FlyAndWalk   = 0x05, // 0101
        FlyAndClimb  = 0x06, // 0110
        //
        All         = 0x07,  // 0111 = 7
    }
}