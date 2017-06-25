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

        /// <summary>
        /// The height of each layer
        /// </summary>
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
            scrubber.Pan = Pan;
            scrubber.ScaleFactor = ScaleFactor;

            //Update the layers
            foreach (TimelineLayerView layer in layers)
            {
                UpdateLayer(layer);
            }
        }
        
        /// <summary>
        /// Updates the given layer
        /// </summary>
        /// <param name="layer"></param>
        private void UpdateLayer(TimelineLayerView layer)
        {
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
    }
}
