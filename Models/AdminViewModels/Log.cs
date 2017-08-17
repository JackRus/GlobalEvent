namespace GlobalEvent.Models.AdminViewModels
{
    public class Log // admins actions
    {
        public int ID { get; set; } 
        public string Type { get; set; } // type of action (ex: checkin help)
        public string Description { get; set; } // description
        public string AdminName { get; set; } 
        public int AdminID { get; set; }   
        public string Date { get; set; }
        public string Time { get; set; } 
    }
}