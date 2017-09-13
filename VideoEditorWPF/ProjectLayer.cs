using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GleamTech.VideoUltimate;

namespace VideoEditorWPF
{
    /// <summary>
    /// Represents a layer in the project
    /// This is the model, not the UI.
    /// </summary>
    public class ProjectLayer
    {
        private const int VIDEO_WIDTH = 1920;   //TODO: Dynamically determine height and width based on the clips?
        private const int VIDEO_HEIGHT = 1080;

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
            //TODO: Make this search more efficient, handle overlapping events
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
        /// Returns a completely transparent bitmap if there is no frame at that point.
        /// Make sure to dispose of this Bitmap object when you're done with it!
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Bitmap GetFrameAt(double time)
        {
            //Search for the TimelineEntry at this time.
            TimelineEntry entry = GetTimelineEntryAt(time);

            //If it wasn't found, return a comletely transparent image
            if (entry == null)
            {
                Bitmap transparent = new Bitmap(VIDEO_WIDTH, VIDEO_HEIGHT);
                for (int x = 0; x < transparent.Width; x++)
                {
                    for (int y = 0; y < transparent.Height; y++)
                    {
                        transparent.SetPixel(x, y, Color.Transparent);
                    }
                }

                return transparent;
            }

            //Get the video clip from the entry
            VideoFrameReader reader = (VideoFrameReader)entry.data;

            //Find the offset into this timeline entry
            double offset = time - entry.startTime;

            //Read it
            reader.Seek(offset);
            return reader.GetFrame();
        }
    }
}
