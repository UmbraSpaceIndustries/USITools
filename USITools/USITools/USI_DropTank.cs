using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USITools
{
    public class USI_DropTank : PartModule
    {
        //tank decouples when out of resources.  May have hilarious results.

        public override void OnUpdate()
        {
            if (vessel != null)
            {
                bool drop = true;
                foreach (var res in part.Resources.list)
                {
                    if (res.amount > 0.001)
                    {
                        drop = false;
                    }
                }
                if (drop)
                {
                    part.decouple();
                }
            }
        }
    }
}
