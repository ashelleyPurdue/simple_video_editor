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

        public int NumEvents { get { return timelineEvents.Count; } }

        private TranslateTransform canvasPan = new TranslateTransform();

		private List<TimelineEvent> timelineEvents = new List<TimelineEvent>();
		private Dictionary<TimelineEvent, TimelineEventControl> eventControls = new Dictionary<TimelineEvent, TimelineEventControl>();

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
		public void AddEvent(TimelineEvent timelineEvent)
		{
			timelineEvents.Add(timelineEvent);

			//Create a control for this event
			TimelineEventControl eventControl = new TimelineEventControl(timelineEvent);
			eventControls.Add(timelineEvent, eventControl);

			eventsCanvas.Children.Add(eventControl);

            //Position the control
            UpdateEventControl(eventControl);
		}

		/// <summary>
		/// Removes an event from the timeline
		/// </summary>
		/// <param name="timelineEvent"></param>
		public void RemoveEvent(TimelineEvent timelineEvent)
		{
			//Don't go on if that event doesn't exist
			if (!timelineEvents.Contains(timelineEvent))
			{
				return;
			}

			//Remove timeline event
			timelineEvents.Remove(timelineEvent);

			//Remove its control
			TimelineEventControl eventControl = eventControls[timelineEvent];

			eventControls.Remove(timelineEvent);
			eventsCanvas.Children.Remove(eventControl);
		}

        public TimelineEvent GetEvent(int index)
        {
            return timelineEvents[index];
        }

		/// <summary>
		/// Returns the event that's occuring at the given time
		/// Returns null if there is no event there
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public TimelineEvent GetEventAt(decimal time)
		{
			foreach (TimelineEvent e in timelineEvents)
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
			foreach (TimelineEventControl eventControl in eventControls.Values)
			{
                UpdateEventControl(eventControl);
			}
		}


        //Misc methods

        /// <summary>
        /// Positions the given control
        /// </summary>
        /// <param name="eventControl"></param>
        private void UpdateEventControl(TimelineEventControl eventControl)
        {
            //Set the position
            Canvas.SetLeft(eventControl, (double)eventControl.timelineEvent.startTime * ScaleFactor);
            Canvas.SetTop(eventControl, 0);

            //Set the size
            eventControl.Width = (double)(eventControl.timelineEvent.endTime - eventControl.timelineEvent.startTime) * ScaleFactor;
            eventControl.Height = (double)this.Height;
            //eventControl.VerticalAlignment = VerticalAlignment.Stretch;
        }
	}

}