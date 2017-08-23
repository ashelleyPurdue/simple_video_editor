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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private MouseDragMonitor timelinePanWatcher;


		public MainWindow()
		{
			InitializeComponent();

            //Subscribe to the timeline pan watcher so we can be informed when the user pans
            timelinePanWatcher = new MouseDragMonitor(timelineView, MouseButton.Middle);
            timelinePanWatcher.DragMoved += TimelinePanWatcher_DragMoved;

            //Set up the timeline view
            TimelineLayerView layerA = new TimelineLayerView();
            TimelineLayerView layerB = new TimelineLayerView();

            layerA.AddEvent(new TimelineEvent("0-10", 0, 10, null));
            layerB.AddEvent(new TimelineEvent("0-10", 0, 10, null));
            
            layerA.AddEvent(new TimelineEvent("20-25", 20, 25, null));
            layerB.AddEvent(new TimelineEvent("20-25", 20, 25, null));

            timelineView.AddLayer(layerA);
            timelineView.AddLayer(layerB);
        }

        private void TimelinePanWatcher_DragMoved(DragEventArgs args)
        {
            //Pan the timeline by the amount the mouse moved
            timelineView.Pan += args.deltaX;
        }

		private void timelineView_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            //Cap the scale factor at zero
            double newScaleFactor = timelineView.ScaleFactor + e.Delta * 0.01;
            if (newScaleFactor < 0)
            {
                newScaleFactor = 0;
            }

            timelineView.ScaleFactor = newScaleFactor;
		}

        private void timelineView_eventResized(TimelineEntryControl sender, double startTime, double endTime)
        {
            //Reject the resize if the start time is after the end time
            if (startTime > endTime)
                return;

            //Resize the event
            sender.timelineEvent.startTime = startTime;
            sender.timelineEvent.endTime = endTime;
            sender.UpdateInterface();
        }
    }
}
