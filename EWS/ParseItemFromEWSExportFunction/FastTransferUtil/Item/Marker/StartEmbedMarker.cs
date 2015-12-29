using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.Marker
{
    public class StartEmbedMarker : Marker.MarkerBase
    {
        protected override string Name
        {
            get { return "StartEmbedMarker"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.StartEmbed; }
        }
    }
}
