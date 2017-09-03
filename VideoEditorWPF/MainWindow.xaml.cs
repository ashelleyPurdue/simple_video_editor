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
using GleamTech.VideoUltimate;

namespace VideoEditorWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private MouseDragMonitor timelinePanWatcher;
        private Microsoft.Win32.OpenFileDialog importFileBrowser = new Microsoft.Win32.OpenFileDialog();

		public MainWindow()
		{
			InitializeComponent();

            //Subscribe to the timeline pan watcher so we can be informed when the user pans
            timelinePanWatcher = new MouseDragMonitor(timelineView, MouseButton.Middle);
            timelinePanWatcher.DragMoved += TimelinePanWatcher_DragMoved;

            //Set up the timeline view
            TimelineLayerView layerA = new TimelineLayerView();
            TimelineLayerView layerB = new TimelineLayerView();

            timelineView.AddLayer(layerA);
            timelineView.AddLayer(layerB);

            layerA.AddEntry(new TimelineEntry("0-10", 0, 10, null));
            layerB.AddEntry(new TimelineEntry("0-10", 0, 10, null));
            
            layerA.AddEntry(new TimelineEntry("20-25", 20, 25, null));
            layerB.AddEntry(new TimelineEntry("20-25", 20, 25, null));
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

        private void timelineView_entryResized(TimelineEntryControl sender, double startTime, double endTime)
        {
            //Reject the resize if the start time is after the end time
            if (startTime > endTime)
                return;

            //Resize the entry
            sender.timelineEntry.startTime = startTime;
            sender.timelineEntry.endTime = endTime;
            sender.UpdateInterface();
        }

        private void importVideoButton_Click(object sender, RoutedEventArgs e)
        {
            //Browse for a video file
            bool? userConfirmed = importFileBrowser.ShowDialog();

            //Don't go on if canceled
            if (userConfirmed != true)
                return;

            //Don't go on if that file is already there
            if (importedVideosListbox.Items.Contains(importFileBrowser.FileName))
                return;

            //Add the video file to the imported list
            Label fileLabel = new Label();
            fileLabel.Content = importFileBrowser.FileName;

            importedVideosListbox.Items.Add(fileLabel);

            //Subscribe to the label's click event so the user can add it to the timeline
            fileLabel.MouseDown += importedVideoLabel_MouseDown;
        }

        private void importedVideoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Add the video at 0 zero seconds on the timeline.
            //TODO: Make the user drag it to the spot in the timeline they want.

            Label clickedLabel = (Label)sender;

            //Open the video file so we can get its length
            VideoFrameReader reader = new VideoFrameReader((string)clickedLabel.Content);
            double length = reader.Duration.TotalSeconds;

            //Create a timeline entry for it
            TimelineEntry newEntry = new TimelineEntry((string)clickedLabel.Content, 0, length, null);
            reader.Dispose();

            //Add it to the timeline
            timelineView.GetLayer(0).AddEntry(newEntry);
        }
    }
}
