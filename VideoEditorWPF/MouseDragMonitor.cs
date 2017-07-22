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

        private MouseButton buttonClicked;
        private UIElement elementWatched;

        private bool isDragging = false;

        public MouseDragMonitor(UIElement elementWatched)
        {
            this.elementWatched = elementWatched;

            //Subscribe to the UIElement's mouse events
            elementWatched.MouseDown += ElementWatched_MouseDown;
            elementWatched.MouseMove += ElementWatched_MouseMove;
            elementWatched.MouseUp += ElementWatched_MouseUp;
        }

        private void ElementWatched_MouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ElementWatched_MouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ElementWatched_MouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class DragEventArgs
    {
        public MouseButton button;
        public double deltaX;
        public double deltaY;
    }
}
