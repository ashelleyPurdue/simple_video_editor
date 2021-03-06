﻿using System;
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
    public delegate void UserResizeHandler(TimelineEntryControl sender, double startTime, double endTime);

    /// <summary>
    /// Interaction logic for TimelineEntryControl.xaml
    /// </summary>
    public partial class TimelineEntryControl : UserControl
	{
        #region subscribable events
        public event UserResizeHandler UserResized;     // Called when the user attempts to resize or move this entry
        #endregion

        public TimelineEntry timelineEntry { get; private set; }

        private TimelineLayerView parentLayerView;
        private MouseDragMonitor leftHandleDragMonitor;
        private MouseDragMonitor rightHandleDragMonitor;
        private MouseDragMonitor moveDragMonitor;

		public TimelineEntryControl(TimelineEntry timelineEntry, TimelineLayerView parentLayerView)
		{
			this.timelineEntry = timelineEntry;
            this.parentLayerView = parentLayerView;

			InitializeComponent();

            //Initialize the drag monitors
            leftHandleDragMonitor = new MouseDragMonitor(leftHandle, MouseButton.Left);
            leftHandleDragMonitor.DragMoved += LeftHandleDragMonitor_DragMoved;
            leftHandleDragMonitor.DragReleased += LeftHandleDragMonitor_DragReleased;

            rightHandleDragMonitor = new MouseDragMonitor(rightHandle, MouseButton.Left);
            rightHandleDragMonitor.DragMoved += RightHandleDragMonitor_DragMoved;
            rightHandleDragMonitor.DragReleased += RightHandleDragMonitor_DragReleased;

            moveDragMonitor = new MouseDragMonitor(visibleRectangle, MouseButton.Left);
            moveDragMonitor.DragMoved += MoveDragMonitor_DragMoved;
            moveDragMonitor.DragReleased += MoveDragMonitor_DragReleased;
        }



        /// <summary>
        /// Updates the control to match the start/end times
        /// </summary>
        public void UpdateInterface()
        {
            //Update the size based on the start/end points

            //Set the position
            Canvas.SetLeft(this, timelineEntry.startTime * parentLayerView.ScaleFactor);
            Canvas.SetTop(this, 0);

            //Set the size
            Width = (timelineEntry.endTime - timelineEntry.startTime) * parentLayerView.ScaleFactor;
            Height = parentLayerView.ActualHeight;
        }

        #region handle drag events
        private void LeftHandleDragMonitor_DragMoved(DragEventArgs args)
        {
            //Updates the graphics to go with the new width

            //Shift the position
            double leftOffset = Canvas.GetLeft(this);
            leftOffset += args.deltaX;

            Canvas.SetLeft(this, leftOffset);

            //Adjust the width
            double newWidth = Width - args.deltaX;
            if (newWidth < 0)
                newWidth = 0;

            Width = newWidth;
        }

        private void RightHandleDragMonitor_DragMoved(DragEventArgs args)
        {
            //Updates the graphics to go with the new width

            //Adjust the width
            double newWidth = Width + args.deltaX;
            if (newWidth < 0)
                newWidth = 0;

            Width = newWidth;
        }

        private void RightHandleDragMonitor_DragReleased(DragEventArgs args)
        {
            //Reset the layout
            UpdateInterface();

            //Send the event
            double endTime = timelineEntry.endTime + args.totalDeltaX / parentLayerView.ScaleFactor;

            if (UserResized != null)
                UserResized(this, timelineEntry.startTime, endTime);
        }

        private void LeftHandleDragMonitor_DragReleased(DragEventArgs args)
        {
            //Reset the layout
            UpdateInterface();

            //Send the event
            double startTime = timelineEntry.startTime + args.totalDeltaX / parentLayerView.ScaleFactor;

            if (UserResized != null)
                UserResized(this, startTime, timelineEntry.endTime);
        }

        private void MoveDragMonitor_DragMoved(DragEventArgs args)
        {
            //Update the graphics to go with the new position
            double newLeft = Canvas.GetLeft(this) + args.deltaX;
            Canvas.SetLeft(this, newLeft);
        }

        private void MoveDragMonitor_DragReleased(DragEventArgs args)
        {
            //Reset the layout
            UpdateInterface();

            //Find the new start and end times
            double scaledDelta = args.totalDeltaX / parentLayerView.ScaleFactor;

            double startTime = timelineEntry.startTime + scaledDelta;
            double endTime = timelineEntry.endTime + scaledDelta;

            //Send the event
            if (UserResized != null)
                UserResized(this, startTime, endTime);
        }
        #endregion
    }
}
