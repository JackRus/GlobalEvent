namespace GlobalEvent.Models.EBViewModels
{
    public class Cost
    {
        public string currency { get; set; } // type
        public decimal value { get; set; } // price 
        public string major_value { get; set; }
        public string display { get; set; } // formatted string
    }
}