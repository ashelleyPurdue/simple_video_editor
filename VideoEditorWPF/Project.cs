using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GleamTech.VideoUltimate;

namespace VideoEditorWPF
{
    public class Project
    {
        public static Project activeProject;

        public Dictionary<string, VideoFrameReader> importedVideos = new Dictionary<string, VideoFrameReader>();
    }
}
