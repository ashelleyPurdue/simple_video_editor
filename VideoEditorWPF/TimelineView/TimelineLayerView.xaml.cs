using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoEditorWPF
{
	/// <summary>
	/// Interaction logic for TimelineLayerView.xaml
	/// </summary>
	public partial class TimelineLayerView : UserControl, IPannableZoomable
	{
        #region IPannableZoomable
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set
            {
                m_scaleFactor = value;
                UpdateInterface();
            }
        }
        private double m_scaleFactor = 1;

        public double Pan
        {
            get { return canvasPan.X; }
            set { canvasPan.X = value; }
        }
        #endregion

        #region subscribable events
        public event UserResizeHandler entryResized;
        #endregion

        public int NumEntries { get { return timelineEntries.Count; } }

        private TranslateTransform canvasPan = new TranslateTransform();

		private List<TimelineEntry> timelineEntries = new List<TimelineEntry>();
		private Dictionary<TimelineEntry, TimelineEntryControl> entryControls = new Dictionary<TimelineEntry, TimelineEntryControl>();

		public TimelineLayerView()
		{
			InitializeComponent();

			//Hook up the canvas pan transform
			TransformGroup g = new TransformGroup();
			g.Children.Add(canvasPan);
			entriesCanvas.RenderTransform = g;
		}

		/// <summary>
		/// Adds an entry to the timeline
		/// </summary>
		/// <param name="timelineEntry"></param>
		public void AddEntry(TimelineEntry timelineEntry)
		{
			timelineEntries.Add(timelineEntry);

			//Create a control for this entry
			TimelineEntryControl entryControl = new TimelineEntryControl(timelineEntry, this);
			entryControls.Add(timelineEntry, entryControl);

			entriesCanvas.Children.Add(entryControl);

            //Position the control
            entryControl.UpdateInterface();

            //Subscribe to the control's events
            entryControl.UserResized += EntryControl_UserResized;
		}

        /// <summary>
        /// Removes an entry from the timeline
        /// </summary>
        /// <param name="timelineEntry"></param>
        public void RemoveEntry(TimelineEntry timelineEntry)
		{
			//Don't go on if that entry doesn't exist
			if (!timelineEntries.Contains(timelineEntry))
			{
				return;
			}

			//Remove timeline entry
			timelineEntries.Remove(timelineEntry);

			//Remove its control
			TimelineEntryControl entryControl = entryControls[timelineEntry];

			entryControls.Remove(timelineEntry);
			entriesCanvas.Children.Remove(entryControl);

            //Unsubscribe from events
            entryControl.UserResized -= EntryControl_UserResized;
		}

        public TimelineEntry GetEntry(int index)
        {
            return timelineEntries[index];
        }

        /// <summary>
        /// Returns the entry that's occuring at the given time
        /// Returns null if there is no entry there
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimelineEntry GetEntryAt(double time)
		{
			foreach (TimelineEntry e in timelineEntries)
			{
				if (time >= e.startTime && time < e.endTime)
				{
					//We've found it, so return this one.
					return e;
				}
			}

			//We didn't find any, so return null
			return null;
		}

		/// <summary>
		/// Positions all controls
		/// </summary>
		public void UpdateInterface()
		{
			//Position every control
			foreach (TimelineEntryControl entryControl in entryControls.Values)
			{
                entryControl.UpdateInterface();
			}
		}

        private void EntryControl_UserResized(TimelineEntryControl sender, double startTime, double endTime)
        {
            //Bubble up the event to the parent
            if (entryResized != null)
                entryResized(sender, startTime, endTime);
        }
    }

}