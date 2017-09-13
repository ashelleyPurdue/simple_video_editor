using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GleamTech.VideoUltimate;

namespace VideoEditorWPF
{
    public class ProjectLayer
    {
        public string name;

        public List<TimelineEntry> clips = new List<TimelineEntry>();

        public ProjectLayer(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns the TimelineEntry at the given point in time
        /// Returns null if no such entry is found
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimelineEntry GetTimelineEntryAt(double time)
        {
            //Search for it
            foreach (TimelineEntry entry in clips)
            {
                if (time >= entry.startTime && time < entry.endTime)
                {
                    return entry;
                }
            }

            //It wasn't found, so return null
            return null;
        }

        /// <summary>
        /// Returns the frame at the given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Bitmap GetFrameAt(double time)
        {
            //Search for the TimelineEntry at this time.
            //TODO: Make this search more efficient
            return null;
        }
    }
}
