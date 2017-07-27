namespace GlobalEvent.Models.AdminViewModels
{
	public class Note
    {
        public int ID { get; set; } 
        public string Body { get; set; } // description, details
  
        public string Date { get; set; }
        public string Time { get; set; }
        public bool Status { get; set; } // important or not
        public int AdminID { get; set; } // who made it
        public string AdminName { get; set; }
        public int VID { get; set; }

        public Note()
        {
            this.Status = false;
        }
    }
}