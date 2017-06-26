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

        private decimal scrubberTargetTime = 0;   //Used for snapping the scrubber to the beginning/ending of timeline events while dragging

        private List<TimelineLayerView> layers = new List<TimelineLayerView>();

        private bool isDragging = false;
        private double prevDragPos = 0;


        public TimelineView()
        {
            InitializeComponent();
        }

        public void AddLayer(TimelineLayerView layer)
        {
            //Add it to the list
            layers.Add(layer);

            //Add it to the stack panel
            layerStackPanel.Children.Add(layer);

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

            //Set the height
            layer.Height = LayerHeight;

            //Set the spacing
            Thickness spacing = layer.Margin;

            spacing.Bottom = LayerSpacing / 2;
            spacing.Top = LayerSpacing / 2;

            layer.Margin = spacing;

            //Set the alignment
            layer.HorizontalAlignment = HorizontalAlignment.Stretch;
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
            isDragging = true;
            prevDragPos = e.GetPosition(this).X;

            scrubberTargetTime = SelectedTime;
        }

        private void scrubHandle_MouseMove(object sender, MouseEventArgs e)
        {
            //Only move the scrubber if we're dragging
            if (!isDragging)
            {
                return;
            }

            //Compute the change in mouse position
            double newX = e.GetPosition(this).X;
            double delta = newX - prevDragPos;
            prevDragPos = newX;

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
            isDragging = false;
        }
    }
}
