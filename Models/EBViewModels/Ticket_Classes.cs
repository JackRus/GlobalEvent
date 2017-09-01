using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.OwnerViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GlobalEvent.Models.EBViewModels
{
    public class Ticket_Classes
    {
        public Ticket_Classes() 
        {
            this.donation = false;
            this.free = false;
            this.quantity_total = 0;
            this.quantity_sold = 0;
        }

        public string resource_uri { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool donation { get; set; }
        public bool free { get; set; }
        public int quantity_total { get; set; }
        public string on_sale_status { get; set; }
        public int quantity_sold { get; set; }
        public Cost cost { get; set; }
    
    
        public static async Task UpdateEB (ApplicationDbContext _db, int EID)
        {
            Event e = await _db.Events.FirstOrDefaultAsync(x => x.ID == EID);
            var url = $"https://www.eventbriteapi.com/v3/events/{e.EventbriteID}/ticket_classes/?token={e.HttpBase}";

            // get request to Eventbrite
            var text = new EBGet(url);
            var ebTickets = JsonConvert.DeserializeObject<EBTickets>(text.responseE);
            List<Ticket> myTickets = await _db.Tickets.Where(x => x.EID == e.ID).ToListAsync();
            
            foreach (var t in ebTickets.Ticket_Classes)
            {   
                var ticket = myTickets.SingleOrDefault(x => x.Type == t.name);
                if (ticket == null)
                {
                    t.Create(e);
                }
                else
                {
                    ticket.Description = "[EB] " + t.description;
                    ticket.Price = t.free ? 0 : t.cost.value;
                    ticket.Limit = t.quantity_total;
                    _db.Tickets.Update(ticket);   
                }
            }
            _db.Events.Update(e);
            await _db.SaveChangesAsync();
        }

        private void Create (Event e)
        {
            Ticket newT = new Ticket();
            newT.Description = $"[EB] {this.description}";
            newT.Type = this.name;
            newT.Limit = this.quantity_total;
            newT.Price = this.free ? 0 : this.cost.value;
            newT.EID = e.ID;

            e.Tickets.Add(newT);
        }
    
    }
}