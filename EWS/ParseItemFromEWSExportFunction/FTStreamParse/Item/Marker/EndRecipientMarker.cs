using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.Marker
{
    public class EndRecipientMarker : Marker.MarkerBase
    {
        protected override string Name
        {
            get { return "EndRecipientMarker"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.EndToRecip; }
        }
    }
}
