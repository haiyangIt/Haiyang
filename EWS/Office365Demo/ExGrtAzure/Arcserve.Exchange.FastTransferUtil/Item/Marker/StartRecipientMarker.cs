using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.Marker
{
    public class StartRecipientMarker : Marker.MarkerBase
    {
        protected override string Name
        {
            get { return "StartRecipientMarker"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.StartRecip; }
        }

        
    }
}
