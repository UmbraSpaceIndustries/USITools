using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USITools
{
    public class USI_DropTank : PartModule
    {
        //tank decouples when out of resources.  May have hilarious results.
        [KSPField] 
        public bool explode = true;

        [KSPField] 
        public float threshold = 0.0001f;


        public override void OnUpdate()
        {
            if (vessel != null)
            {
                bool drop = true;
                foreach (var res in part.Resources.list)
                {
                    if (res.amount >= threshold)
                    {
                        drop = false;
                    }
                }
                if (drop)
                {
                    if (part.parent != null)
                    {
                        part.decouple();
                    }
                    if (explode)
                    {
                        part.explode();
                    } 

                }
            }
        }
    }
}
