using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AtRng.MobileTTA {
    public interface IAccount {

        List<IUnit> GetUnitCollection();

        // Currency Accessors.
    }
}
