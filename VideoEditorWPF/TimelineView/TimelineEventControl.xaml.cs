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
	/// Interaction logic for TimelineEventControl.xaml
	/// </summary>
	public partial class TimelineEventControl : UserControl
	{
        #region subscribable events
        public delegate void UserResizeHandler(double startTime, double endTime);
        public event UserResizeHandler UserResized;

        #endregion

        public TimelineEvent timelineEvent { get; private set; }

        private MouseDragMonitor leftHandleDragMonitor;
        private MouseDragMonitor rightHandleDragMonitor;

		public TimelineEventControl(TimelineEvent timelineEvent)
		{
			this.timelineEvent = timelineEvent;
			InitializeComponent();

            //Initialize the drag monitors
            leftHandleDragMonitor = new MouseDragMonitor(leftHandle, MouseButton.Left);
            rightHandleDragMonitor = new MouseDragMonitor(rightHandle, MouseButton.Left);

            leftHandleDragMonitor.DragMoved += LeftHandleDragMonitor_DragMoved;
            rightHandleDragMonitor.DragMoved += RightHandleDragMonitor_DragMoved;

            leftHandleDragMonitor.DragReleased += LeftHandleDragMonitor_DragReleased;
            rightHandleDragMonitor.DragReleased += RightHandleDragMonitor_DragReleased;
		}

        #region handle drag events
        private void LeftHandleDragMonitor_DragMoved(DragEventArgs args)
        {
            // TODO: Update the graphics to go with the new width
        }

        private void RightHandleDragMonitor_DragMoved(DragEventArgs args)
        {
            // TODO: Update the graphics to go with the new width
        }

        private void RightHandleDragMonitor_DragReleased(DragEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void LeftHandleDragMonitor_DragReleased(DragEventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
