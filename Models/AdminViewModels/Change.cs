namespace GlobalEvent.Models.AdminViewModels
{
    public class Change
    {
        public int ID { get; set; } 
        public string Name { get; set; }
        public string Last { get; set; }
        public string Email { get; set; } 
        public string Phone { get; set; }   
        public string Extention { get; set; }
        public string Occupation { get; set; } 
        public string Company { get; set; } 
        public int ParentID { get; set; }
    }
}