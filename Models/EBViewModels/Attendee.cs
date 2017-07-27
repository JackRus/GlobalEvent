namespace GlobalEvent.Models.EBViewModels
{
    public class Attendee
    {
        public string order_id { get; set; }
        public string quantity { get; set; }
        public string ticket_class_name { get; set; }
        public bool cancelled { get; set; }
        public bool refunded { get; set; }
        public string resource_uri { get; set; }
        public Profile profile { get; set; }
        public string created { get; set; }

    }
}