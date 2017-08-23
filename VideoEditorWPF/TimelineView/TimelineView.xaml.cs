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
        public event UserResizeHandler entryResized;    //Fires when the user tries to resize this entry
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
        public double SelectedTime
        {
            get { return m_selectedTime; }
            set
            {
                m_selectedTime = value;
                UpdateScrubber();
            }
        }
        private double m_selectedTime = 0;

        private MouseDragMonitor scrubberDragMonitor;

        private double scrubberTargetTime = 0;   //Used for snapping the scrubber to the beginning/ending of timeline entries while dragging

        #endregion


        public TimelineView()
        {
            InitializeComponent();

            //Subscribe to the scrubber drag monitor, so we can be informed when the user drags the scrubber
            scrubberDragMonitor = new MouseDragMonitor(scrubHandle, MouseButton.Left);
            scrubberDragMonitor.DragStarted += scrubHandle_DragStarted;
            scrubberDragMonitor.DragMoved += scrubHandle_DragMoved;
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
            layer.entryResized += layer_entryResized;

            //Update this layer
            UpdateLayer(layer);
        }

        private void layer_entryResized(TimelineEntryControl sender, double startTime, double endTime)
        {
            //Bubble the event up
            if (entryResized != null)
                entryResized(sender, startTime, endTime);
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

        private double[] GetSnapPoints()
        {
            //Returns a list of all points that the scrubber should snap to.

            List<double> snapPoints = new List<double>();

            //Add the beginnings/endings of all entries
            foreach (TimelineLayerView layer in layers)
            {
                for (int i = 0; i < layer.NumEntries; i++)
                {
                    TimelineEntry timelineEntry = layer.GetEntry(i);

                    snapPoints.Add(timelineEntry.startTime);
                    snapPoints.Add(timelineEntry.endTime);
                }
            }

            return snapPoints.ToArray();
        }

        private void SnapToPoint()
        {
            //Get all the snap points
            double[] snapPoints = GetSnapPoints();

            //Don't go on if there are no snap points
            if (snapPoints.Length == 0)
            {
                return;
            }

            //Find the closest snap point
            double closestSnapPoint = 0;
            double closestSnapDistance = double.MaxValue;

            foreach (double point in snapPoints)
            {
                //Update the closest snap point
                double dist = Math.Abs(point - scrubberTargetTime);

                if (dist < closestSnapDistance)
                {
                    closestSnapDistance = dist;
                    closestSnapPoint = point;
                }
            }

            //Find the scrubber's physical distance from the snap point
            double physicalScrubberPos = IPannableZoomableUtils.LocalToGlobalPos(scrubberTargetTime, this);
            double physicalSnapPos = IPannableZoomableUtils.LocalToGlobalPos(closestSnapPoint, this);

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
            SelectedTime = IPannableZoomableUtils.GlobalToLocalPos(clickedPos, this);
        }

        private void scrubHandle_DragStarted(DragEventArgs args)
        {
            //Start the target time at its current time
            scrubberTargetTime = SelectedTime;
        }

        private void scrubHandle_DragMoved(DragEventArgs args)
        {
            //Move the scrubber when the user drags it
            scrubberTargetTime += args.deltaX / ScaleFactor;
            SelectedTime = scrubberTargetTime;

            //If the scrubber's target pos is close to a "snap point", snap the ScrubPos there.
            SnapToPoint();
        }

        private void layer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Update the layer that was changed
            TimelineLayerView layer = (TimelineLayerView)sender;
            layer.UpdateInterface();
        }
    }
}
