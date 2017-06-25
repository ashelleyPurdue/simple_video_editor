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

        //The position of the scrubber in the timeline
        public double ScrubPos
        {
            get { return m_scrubPos; }
            set
            {
                m_scrubPos = value;
                UpdateScrubber();
            }
        }
        private double m_scrubPos = 0;

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
            double pos = IPannableZoomableUtils.LocalToGlobalPos(ScrubPos, this);

            Thickness margin = scrubHandle.Margin;
            margin.Left = pos - scrubHandle.Width / 2;
            scrubHandle.Margin = margin;

            //Update the seek line's position
            margin = scrubberRedLine.Margin;
            margin.Left = pos;
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

        private void scrubberClickLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //change the scrub pos to the place we clicked on
            double clickedPos = e.GetPosition(this).X;
            ScrubPos = IPannableZoomableUtils.GlobalToLocalPos(clickedPos, this);
        }

        private void scrubHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Capture the mouse so things don't break if the user moves the mouse beyond this control's borders
            Mouse.Capture(scrubHandle, CaptureMode.Element);

            //Start dragging
            isDragging = true;
            prevDragPos = e.GetPosition(this).X;
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
            ScrubPos += delta / ScaleFactor;
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
