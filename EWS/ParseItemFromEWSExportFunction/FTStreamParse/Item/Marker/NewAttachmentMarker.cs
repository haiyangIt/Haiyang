using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.Marker
{
    public class NewAttachmentMarker : Marker.MarkerBase
    {
        protected override string Name
        {
            get { return "StartAttachment"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.NewAttach; }
        }
    }
}
