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
    /// Interaction logic for TimelineView.xaml
    /// </summary>
    public partial class TimelineView : UserControl, IPannableZoomable
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
            get { return m_pan; }
            set
            {
                m_pan = value;
                UpdateInterface();
            }
        }
        private double m_pan = 0;

        #endregion

        #region subscribable events

        public delegate void EventMovedHandler(TimelineEvent timelineEvent, decimal newStartTime);

        public event EventMovedHandler eventMoved;      //Raised after the user drags and drops an event to a new location

        #endregion

        //The height of each layer
        public double LayerHeight
        {
            get { return m_layerHeight; }
            set
            {
                m_layerHeight = value;
                UpdateInterface();
            }
        }
        private double m_layerHeight = 100;

        //How far apart the layers should be spaced
        public double LayerSpacing
        {
            get { return m_layerSpacing; }
            set
            {
                m_layerSpacing = value;
                UpdateInterface();
            }
        }
        private double m_layerSpacing = 5;

        private List<TimelineLayerView> layers = new List<TimelineLayerView>();

        #region scrubber fields

        //The time that the scrubber is pointing to
        public decimal SelectedTime
        {
            get { return m_selectedTime; }
            set
            {
                m_selectedTime = value;
                UpdateScrubber();
            }
        }
        private decimal m_selectedTime = 0;

        private bool isDraggingScrubber = false;
        private double scrubberPrevDragPos = 0;

        private decimal scrubberTargetTime = 0;   //Used for snapping the scrubber to the beginning/ending of timeline events while dragging

        #endregion

        #region TimelineEvent moving fields

        private bool isDraggingEvent = false;
        private decimal eventDragClickTime = 0;     //The time in the timeline that the user clicked when they started dragging.

        private double previewRectPrevDragPos = 0;

        private TimelineEvent eventDragged = null;
        #endregion


        public TimelineView()
        {
            InitializeComponent();

            //Make it so the preview rectangle starts out with no parent
            //TODO: Find a cleaner way to do this.
            grid.Children.Remove(previewRect);
        }

        public void AddLayer(TimelineLayerView layer)
        {
            //Add it to the list
            layers.Add(layer);

            //Set the layer's alignment
            layer.HorizontalAlignment = HorizontalAlignment.Stretch;
            layer.VerticalAlignment = VerticalAlignment.Stretch;
            layer.Margin = new Thickness(0);

            //Create a new row in the grid for the layer
            if (layerGrid.RowDefinitions.Count < layers.Count)
                layerGrid.RowDefinitions.Add(new RowDefinition());

            int rowIndex = layerGrid.RowDefinitions.Count - 1;

            layerGrid.RowDefinitions[rowIndex].MinHeight = 0;

            //Put the layer in the grid
            Grid.SetColumn(layer, 0);
            Grid.SetRow(layer, rowIndex);
            layerGrid.Children.Add(layer);

            //Add a splitter
            layerGrid.RowDefinitions.Add(new RowDefinition());

            GridSplitter splitter = new GridSplitter();

            splitter.Height = 5;
            splitter.VerticalAlignment = VerticalAlignment.Bottom;
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;

            splitter.ResizeBehavior = GridResizeBehavior.BasedOnAlignment;
            splitter.ResizeDirection = GridResizeDirection.Rows;

            //splitter.Style = Resources["DarkGridSplitterStyle"] as Style;

            Grid.SetColumn(splitter, 0);
            Grid.SetRow(splitter, rowIndex);
            layerGrid.Children.Add(splitter);

            //Subscribe to the layer's events.
            layer.SizeChanged += layer_SizeChanged;
            layer.MouseDown += layer_MouseDown;
            layer.MouseMove += layer_MouseMove;
            layer.MouseUp += layer_MouseUp;

            //Update this layer
            UpdateLayer(layer);
        }


        //Misc methods

        private void UpdateInterface()
        {
            //Update the scrubber
            UpdateScrubber();

            //Update the layers
            foreach (TimelineLayerView layer in layers)
            {
                UpdateLayer(layer);
            }
        }

        private void UpdateScrubber()
        {
            //Update the handle's position
            double pos = IPannableZoomableUtils.LocalToGlobalPos((double)SelectedTime, this);

            Thickness margin = scrubHandle.Margin;
            margin.Left = pos - scrubHandle.Width / 2;
            scrubHandle.Margin = margin;

            //Update the seek line's position
            margin = scrubberRedLine.Margin;
            margin.Left = pos;
            margin.Top = scrubHandle.Margin.Top + scrubHandle.Height;
            scrubberRedLine.Margin = margin;
        }

        private void UpdateLayer(TimelineLayerView layer)
        {
            //Updates the given layer

            //set the pan and scale factors
            layer.Pan = Pan;
            layer.ScaleFactor = ScaleFactor;
        }

        private decimal[] GetSnapPoints()
        {
            //Returns a list of all points that the scrubber should snap to.

            List<decimal> snapPoints = new List<decimal>();

            //Add the beginnings/endings of all events
            foreach (TimelineLayerView layer in layers)
            {
                for (int i = 0; i < layer.NumEvents; i++)
                {
                    TimelineEvent timelineEvent = layer.GetEvent(i);

                    snapPoints.Add(timelineEvent.startTime);
                    snapPoints.Add(timelineEvent.endTime);
                }
            }

            return snapPoints.ToArray();
        }

        private void SnapToPoint()
        {
            //Get all the snap points
            decimal[] snapPoints = GetSnapPoints();

            //Don't go on if there are no snap points
            if (snapPoints.Length == 0)
            {
                return;
            }

            //Find the closest snap point
            decimal closestSnapPoint = 0;
            decimal closestSnapDistance = decimal.MaxValue;

            foreach (decimal point in snapPoints)
            {
                //Update the closest snap point
                decimal dist = Math.Abs(point - (decimal)scrubberTargetTime);

                if (dist < closestSnapDistance)
                {
                    closestSnapDistance = dist;
                    closestSnapPoint = point;
                }
            }

            //Find the scrubber's physical distance from the snap point
            double physicalScrubberPos = IPannableZoomableUtils.LocalToGlobalPos((double)scrubberTargetTime, this);
            double physicalSnapPos = IPannableZoomableUtils.LocalToGlobalPos((double)closestSnapPoint, this);

            double physicalDistance = Math.Abs(physicalScrubberPos - physicalSnapPos);

            //Snap to the point if it's close enough
            const double SNAP_MARGIN = 5;
            if (physicalDistance < SNAP_MARGIN)
            {
                SelectedTime = closestSnapPoint;
            }
        }


        //Event handlers

        private void scrubberGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            //change the scrub pos to the place we clicked on
            double clickedPos = e.GetPosition(this).X;
            SelectedTime = (decimal)IPannableZoomableUtils.GlobalToLocalPos(clickedPos, this);
        }

        private void scrubHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Only start dragging if it's a left-click
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            //Capture the mouse so things don't break if the user moves the mouse beyond this control's borders
            Mouse.Capture(scrubHandle, CaptureMode.Element);

            //Start dragging
            isDraggingScrubber = true;
            scrubberPrevDragPos = e.GetPosition(this).X;

            scrubberTargetTime = SelectedTime;
        }

        private void scrubHandle_MouseMove(object sender, MouseEventArgs e)
        {
            //Only move the scrubber if we're dragging
            if (!isDraggingScrubber)
            {
                return;
            }

            //Compute the change in mouse position
            double newX = e.GetPosition(this).X;
            double delta = newX - scrubberPrevDragPos;
            scrubberPrevDragPos = newX;

            //Scale it then add it to the scrub pos
            scrubberTargetTime += (decimal)(delta / ScaleFactor);
            SelectedTime = scrubberTargetTime;

            //If the scrubber's target pos is close to a "snap point", snap the ScrubPos there.
            SnapToPoint();
        }

        private void scrubHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Un-capture the mouse
            Mouse.Capture(scrubHandle, CaptureMode.None);

            //Stop dragging
            isDraggingScrubber = false;
        }

        private void layer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Update the layer that was changed
            TimelineLayerView layer = (TimelineLayerView)sender;
            layer.UpdateInterface();
        }

        private void layer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TimelineLayerView layer = (TimelineLayerView)sender;

            //Don't go on unless it's a left click
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            //Check if we're clicking on an event
            decimal timeClicked = (decimal)IPannableZoomableUtils.GlobalToLocalPos(e.GetPosition(this).X, layer);
            TimelineEvent eventClicked = layer.GetEventAt(timeClicked);

            //If we clicked on an event, start dragging it
            if (eventClicked != null)
            {
                //Start dragging
                eventDragged = eventClicked;
                eventDragClickTime = timeClicked;
                previewRectPrevDragPos = e.GetPosition(this).X;
                isDraggingEvent = true;

                Mouse.Capture(layer, CaptureMode.Element);

                //Show the preview rectangle
                previewRect.Visibility = Visibility.Visible;
                layer.eventsCanvas.Children.Add(previewRect);

                //Set the preview rectangle's size
                previewRect.Height = layer.ActualHeight;
                previewRect.Width = (double)(eventClicked.endTime - eventClicked.startTime) * ScaleFactor;

                //Set the preview rectangle's position
                Thickness rectMargin = previewRect.Margin;

                rectMargin.Top = layer.Margin.Top;
                rectMargin.Left = (double)eventClicked.startTime * ScaleFactor;

                previewRect.Margin = rectMargin;
            }

        }

        private void layer_MouseMove(object sender, MouseEventArgs e)
        {
            TimelineLayerView layer = (TimelineLayerView)sender;

            //Skip if we're not dragging an event
            if (!isDraggingEvent)
            {
                return;
            }

            //Get the delta x so we can move the preview rectangle by it
            double deltaX = e.GetPosition(this).X - previewRectPrevDragPos;
            previewRectPrevDragPos = e.GetPosition(this).X;

            //Move the preview rectangle
            Thickness rectMargin = previewRect.Margin;
            rectMargin.Left += deltaX;
            previewRect.Margin = rectMargin;
        }

        private void layer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TimelineLayerView layer = (TimelineLayerView)sender;

            //Don't go on if it's not the left button
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            //Don't go on if we're not dragging an event
            if (!isDraggingEvent)
            {
                return;
            }

            //Calculate the new start time for the event
            decimal timeDropped = (decimal)IPannableZoomableUtils.GlobalToLocalPos(e.GetPosition(this).X, layer);
            decimal deltaTime = timeDropped - eventDragClickTime;
            decimal newStartTime = eventDragged.startTime + deltaTime;

            //Raise the eventMoved event
            if (eventMoved != null)
            {
                eventMoved(eventDragged, newStartTime);
            }

            //Stop dragging
            isDraggingEvent = false;
            Mouse.Capture(layer, CaptureMode.None);
            eventDragged = null;

            //Hide the preview rectangle
            previewRect.Visibility = Visibility.Collapsed;
            layer.eventsCanvas.Children.Remove(previewRect);

            //Update the layer
            layer.UpdateInterface();
        }
    }
}
