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
        public event UserResizeHandler eventResized;
        #endregion

        public int NumEvents { get { return timelineEvents.Count; } }

        private TranslateTransform canvasPan = new TranslateTransform();

		private List<TimelineEntry> timelineEvents = new List<TimelineEntry>();
		private Dictionary<TimelineEntry, TimelineEntryControl> eventControls = new Dictionary<TimelineEntry, TimelineEntryControl>();

		public TimelineLayerView()
		{
			InitializeComponent();

			//Hook up the canvas pan transform
			TransformGroup g = new TransformGroup();
			g.Children.Add(canvasPan);
			eventsCanvas.RenderTransform = g;
		}

		/// <summary>
		/// Adds an event to the timeline
		/// </summary>
		/// <param name="timelineEvent"></param>
		public void AddEvent(TimelineEntry timelineEvent)
		{
			timelineEvents.Add(timelineEvent);

			//Create a control for this event
			TimelineEntryControl eventControl = new TimelineEntryControl(timelineEvent, this);
			eventControls.Add(timelineEvent, eventControl);

			eventsCanvas.Children.Add(eventControl);

            //Position the control
            eventControl.UpdateInterface();

            //Subscribe to the control's events
            eventControl.UserResized += EventControl_UserResized;
		}

        /// <summary>
        /// Removes an event from the timeline
        /// </summary>
        /// <param name="timelineEvent"></param>
        public void RemoveEvent(TimelineEntry timelineEvent)
		{
			//Don't go on if that event doesn't exist
			if (!timelineEvents.Contains(timelineEvent))
			{
				return;
			}

			//Remove timeline event
			timelineEvents.Remove(timelineEvent);

			//Remove its control
			TimelineEntryControl eventControl = eventControls[timelineEvent];

			eventControls.Remove(timelineEvent);
			eventsCanvas.Children.Remove(eventControl);

            //Unsubscribe from events
            eventControl.UserResized -= EventControl_UserResized;
		}

        public TimelineEntry GetEvent(int index)
        {
            return timelineEvents[index];
        }

        /// <summary>
        /// Returns the event that's occuring at the given time
        /// Returns null if there is no event there
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimelineEntry GetEventAt(double time)
		{
			foreach (TimelineEntry e in timelineEvents)
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
			foreach (TimelineEntryControl eventControl in eventControls.Values)
			{
                eventControl.UpdateInterface();
			}
		}

        private void EventControl_UserResized(TimelineEntryControl sender, double startTime, double endTime)
        {
            //Bubble up the event to the parent
            if (eventResized != null)
                eventResized(sender, startTime, endTime);
        }
    }

}