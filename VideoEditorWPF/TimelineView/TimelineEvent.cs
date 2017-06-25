using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditorWPF
{
	public class TimelineEvent
	{
		public string name;

		public decimal startTime;
		public decimal endTime;
		public Object data;

		public TimelineEvent(string name, decimal startTime, decimal endTime, Object data)
		{
			this.name = name;
			this.startTime = startTime;
			this.endTime = endTime;
			this.data = data;
		}
	}
}
