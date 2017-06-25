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
		public TimelineEvent timelineEvent { get; private set; }

		public TimelineEventControl(TimelineEvent timelineEvent)
		{
			this.timelineEvent = timelineEvent;
			InitializeComponent();
		}
	}
}
