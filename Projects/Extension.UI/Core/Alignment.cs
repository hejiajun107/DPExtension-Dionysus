using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.UI.Core
{
    public enum HorizontalAlignment
    {
        //
        // 摘要:
        //     An element aligned to the left of the layout slot for the parent element.
        Left = 0,
        //
        // 摘要:
        //     An element aligned to the center of the layout slot for the parent element.
        Center = 1,
        //
        // 摘要:
        //     An element aligned to the right of the layout slot for the parent element.
        Right = 2,
        //
        // 摘要:
        //     An element stretched to fill the entire layout slot of the parent element.
        Stretch = 3
    }

    public enum FlowDirection
    {
        //
        // 摘要:
        //     Indicates that content should flow from left to right.
        LeftToRight = 0,
        //
        // 摘要:
        //     Indicates that content should flow from right to left.
        RightToLeft = 1
    }
    public enum VerticalAlignment
    {
        //
        // 摘要:
        //     The child element is aligned to the top of the parent's layout slot.
        Top = 0,
        //
        // 摘要:
        //     The child element is aligned to the center of the parent's layout slot.
        Center = 1,
        //
        // 摘要:
        //     The child element is aligned to the bottom of the parent's layout slot.
        Bottom = 2,
        //
        // 摘要:
        //     The child element stretches to fill the parent's layout slot.
        Stretch = 3
    }

}
