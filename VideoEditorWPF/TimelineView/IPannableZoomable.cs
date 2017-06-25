using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditorWPF
{
    public static class IPannableZoomableUtils
    {
        /// <summary>
        /// Transforms the given position in "local space" to its corresponding position in "global space"
        /// </summary>
        public static double LocalToGlobalPos(double localPos, IPannableZoomable space)
        {
            return space.Pan + localPos * space.ScaleFactor;
        }

        /// <summary>
        /// Transfroms the given position in "global space" to its corresponding position in "local space"
        /// </summary>
        public static double GlobalToLocalPos(double globalPos, IPannableZoomable space)
        {
            return (globalPos - space.Pan) / space.ScaleFactor;
        }
    }

    public interface IPannableZoomable
    {
        double ScaleFactor { get; set; }
        double Pan { get; set; }
    }
}
