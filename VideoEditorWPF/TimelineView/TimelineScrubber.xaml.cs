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
    /// Interaction logic for TimelineScrubber.xaml
    /// </summary>
    public partial class TimelineScrubber : UserControl, IPannableZoomable
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

        /// <summary>
        /// The position of the scrubber in the timeline
        /// </summary>
        public double ScrubPos
        {
            get { return m_scrubPos; }
            set
            {
                m_scrubPos = value;
                UpdateInterface();
            }
        }
        private double m_scrubPos = 0;

        private bool isDragging = false;
        private double prevDragPos = 0;

        public TimelineScrubber()
        {
            InitializeComponent();
            UpdateInterface();
        }


        //Misc methods

        private void UpdateInterface()
        {
            //Update the handle's position
            double pos = IPannableZoomableUtils.LocalToGlobalPos(ScrubPos, this);

            Thickness margin = scrubHandle.Margin;
            margin.Left = pos - scrubHandle.Width / 2;
            scrubHandle.Margin = margin;

            //Update the seek line's position
            margin = seekLine.Margin;
            margin.Left = pos;
            seekLine.Margin = margin;
        }


        //Event handlers

        private void line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //change the scrub pos to the place we clicked on
            double clickedPos = e.GetPosition(this).X;
            ScrubPos = IPannableZoomableUtils.GlobalToLocalPos(clickedPos, this);
        }

        private void scrubHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Start dragging
            isDragging = true;
            prevDragPos = e.GetPosition(this).X;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
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
            //Stop dragging
            isDragging = false;
        }
    }
}
