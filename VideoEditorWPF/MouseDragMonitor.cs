using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VideoEditorWPF
{
    /// <summary>
    /// Provides more comprehensive events for clicking and dragging on an object
    /// </summary>
    public class MouseDragMonitor
    {
        public delegate void DragEventHandler(DragEventArgs args);

        public event DragEventHandler DragStarted;
        public event DragEventHandler DragMoved;
        public event DragEventHandler DragReleased;

        private MouseButton buttonWatched;
        private UIElement elementWatched;

        private bool isDragging = false;
        private Point prevMousePos;
        private Point totalMouseDelta;

        public MouseDragMonitor(UIElement elementWatched, MouseButton buttonWatched)
        {
            this.elementWatched = elementWatched;
            this.buttonWatched = buttonWatched;

            //Subscribe to the UIElement's mouse events
            elementWatched.MouseDown += ElementWatched_MouseDown;
            elementWatched.MouseMove += ElementWatched_MouseMove;
            elementWatched.MouseUp += ElementWatched_MouseUp;
        }

        private void ElementWatched_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Don't do anything if we're already dragging
            if (isDragging)
                return;

            //Don't do anything if it's not the button we're paying attention to
            if (e.ChangedButton != buttonWatched)
                return;

            //Start dragging
            isDragging = true;
            prevMousePos = e.GetPosition(null);
            totalMouseDelta = new Point(0, 0);

            Mouse.Capture(elementWatched, CaptureMode.Element);     //Capture the mouse so the user can safely drag the mouse out of the watched object's bounds

            //Send the drag started event
            if (DragStarted != null)
                DragStarted(new DragEventArgs(buttonWatched, 0, 0, 0, 0));
        }

        private void ElementWatched_MouseMove(object sender, MouseEventArgs e)
        {
            //Don't go on if we're not currently dragging
            if (!isDragging)
                return;

            //Calculate the delta movement so we can put it in the event args
            Point currentMousePos = e.GetPosition(null);

            double deltaX = currentMousePos.X - prevMousePos.X;
            double deltaY = currentMousePos.Y - prevMousePos.Y;

            totalMouseDelta.X += deltaX;
            totalMouseDelta.Y += deltaY;

            //Save the current mouse pos as the previous one, so we can compute the delta again next time
            prevMousePos = currentMousePos;

            //Fire the drag moved event
            if (DragMoved != null)
                DragMoved(new DragEventArgs(buttonWatched, deltaX, deltaY, totalMouseDelta.X, totalMouseDelta.Y));
        }

        private void ElementWatched_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Don't go on if the we're not dragging
            if (!isDragging)
                return;

            //Don't go on if the wrong button was released.
            if (e.ChangedButton != buttonWatched)
                return;

            //Stop dragging
            isDragging = false;
            Mouse.Capture(elementWatched, CaptureMode.None);

            //Fire the drag stopped event
            if (DragReleased != null)
                DragReleased(new DragEventArgs(buttonWatched, 0, 0, totalMouseDelta.X, totalMouseDelta.Y));
        }
    }

    public class DragEventArgs
    {
        public MouseButton button;

        public double deltaX;
        public double deltaY;

        public double totalDeltaX;  //The total x movement since the user started dragging
        public double totalDeltaY;  //The total y movement since the user started dragging

        public DragEventArgs(MouseButton button, double deltaX, double deltaY, double totalDeltaX, double totalDeltaY)
        {
            this.button = button;

            this.deltaX = deltaX;
            this.deltaY = deltaY;

            this.totalDeltaX = totalDeltaX;
            this.totalDeltaY = totalDeltaY;
        }
    }
}
