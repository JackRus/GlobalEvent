namespace GlobalEvent.Models.AdminViewModels
{
	public class Note
    {
        public int ID { get; set; } 
        public string Description { get; set; } // description, details
        public string Date { get; set; }
        public string Time { get; set; }
        public bool Important { get; set; } // important or not
        public int AdminID { get; set; } // who made it
        public string AdminName { get; set; }
        public int VID { get; set; }
        public bool SeenByAdmin { get; set; }

        public Note()
        {
            this.Important = false;
            this.SeenByAdmin = true;
        }
    }
}