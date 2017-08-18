namespace GlobalEvent.Models.AdminViewModels
{
	public class Issue // technical issues
	{
		public int ID { get; set; }
		public string Body { get; set; } // description, text-area
		public string Type { get; set; } // issue type
		public string AdminName { get; set; }
		public string AdminID { get; set; }
		public string Date { get; set; }
		public string Time { get; set; }
		public bool Solved { get; set; } // yes/no
		public string Status { get; set; } // multiple choice
		public int VID { get; set; }
	
        public Issue()
        {
            this.Solved = false;
        }
    }
}