using System;

namespace GlobalEvent.Models.OwnerViewModels
{
	public class Visit
	{
		public Visit()
		{
			this.Time = DateTime.Now.ToString("HH:mm");
			this.Present = false;
		}

		public Visit(int VID, bool present)
		{
			this.VID = VID;
			this.Time = DateTime.Now.ToString("HH:mm");
			this.Present = present;

		}
		public int ID { get; set; }
		public int VID { get; set; }
		public string Time { get; set; }
		public bool Present { get; set; }
	}
}