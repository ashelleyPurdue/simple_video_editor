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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool isPanning = false;
		private double prevPanPos;

		public MainWindow()
		{
			InitializeComponent();

            //Set up the timeline view
            TimelineLayerView layerA = new TimelineLayerView();
            TimelineLayerView layerB = new TimelineLayerView();

            layerA.AddEvent(new TimelineEvent("0-10", 0, 10, null));
            layerB.AddEvent(new TimelineEvent("0-10", 0, 10, null));
            
            layerA.AddEvent(new TimelineEvent("20-25", 20, 25, null));
            layerB.AddEvent(new TimelineEvent("20-25", 20, 25, null));

            timelineView.AddLayer(layerA);
            timelineView.AddLayer(layerB);
        }

		private void timelineView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
                //Capture the mouse, so things don't break if the user drags out of the border
                Mouse.Capture(timelineView, CaptureMode.Element);

                //Start panning
				isPanning = true;
				prevPanPos = e.GetPosition(timelineView).X;
			}
		}

		private void timelineView_MouseMove(object sender, MouseEventArgs e)
		{
			if (isPanning)
			{
				//Compute the delta pos
				double currPanPos = e.GetPosition(timelineView).X;
				double delta = currPanPos - prevPanPos;
				prevPanPos = currPanPos;

				//Pan it by the delta
				timelineView.Pan += delta;
			}
		}

		private void timelineView_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
                //Stop panning
                Mouse.Capture(timelineView, CaptureMode.None);
				isPanning = false;
			}
		}

		private void timelineView_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            //Cap the scale factor at zero
            double newScaleFactor = timelineView.ScaleFactor + e.Delta * 0.01;
            if (newScaleFactor < 0)
            {
                newScaleFactor = 0;
            }

            timelineView.ScaleFactor = newScaleFactor;
		}
	}
}
