﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTStreamUtil.Item.Marker
{
    public class EndEmbedMarker : MarkerBase
    {
        protected override string Name
        {
            get { return "EndEmbedMarker"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.EndEmbed; }
        }
    }
}
