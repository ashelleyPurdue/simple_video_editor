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

            //Start with an empty project
            Project.activeProject = new Project();

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

        #region update methods

        private void UpdateImportedVideoBox()
        {
            //Remove all items from the imported videos listbox
            foreach (Label videoLabel in importedVideosListbox.Items)
            {
                videoLabel.MouseDown -= importedVideoLabel_MouseDown;   //We need to unsubscribe from the event to
                                                                        //make sure the labels we remove get garbage collected.
            }
            importedVideosListbox.Items.Clear();

            //Add a new label for each imported video
            foreach (string importedFile in Project.activeProject.importedVideos.Keys)
            {
                Label fileLabel = new Label();
                fileLabel.Content = importedFile;

                importedVideosListbox.Items.Add(fileLabel);

                //Subscribe to the label's click event so the user can add it to the timeline
                fileLabel.MouseDown += importedVideoLabel_MouseDown;
            }
        }

        #endregion

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

            string fileName = importFileBrowser.FileName;

            //Don't go on if that file is already there
            if (Project.activeProject.importedVideos.ContainsKey(fileName))
                return;

            //Load the video file
            Project.activeProject.importedVideos.Add(fileName, new VideoFrameReader(fileName));

            //Update the listbox
            UpdateImportedVideoBox();
        }

        private void importedVideoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Add the video at 0 zero seconds on the timeline.
            //TODO: Make the user drag it to the spot in the timeline they want.

            Label clickedLabel = (Label)sender;

            //Open the video file so we can get its length
            VideoFrameReader reader = Project.activeProject.importedVideos[(string)clickedLabel.Content];
            double length = reader.Duration.TotalSeconds;

            //Create a timeline entry for it
            TimelineEntry newEntry = new TimelineEntry((string)clickedLabel.Content, 0, length, null);

            //Add it to the timeline
            timelineView.GetLayer(0).AddEntry(newEntry);
        }

        private void importedVideosListbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Forward the event to the label that was clicked
            System.Windows.Media.VisualTreeHelper.HitTest(this, Mouse.GetPosition(null));
        }
    }
}
