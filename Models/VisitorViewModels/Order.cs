using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEvent.Data;
using GlobalEvent.Models.EBViewModels;
using GlobalEvent.Models.OwnerViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GlobalEvent.Models.VisitorViewModels
{
	public class Order
    {
        public int ID { get; set; }
        public int Number { get; set; } 
        public int Amount { get; set; }
        public bool Cancelled { get; set; }
        public int CheckedIn { get; set; }
        public bool Full { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhone { get; set; }
        public string Date { get; set; }     //date / time
        public string Time { get; set; }  
        public int EID { get; set; }   //date / time
        public string TicketType { get; set; }
        //public string DirectUrl { get; set; }
    
        public Order()
        {
            this.Number = 0;
            this.Amount = 0;
            this.CheckedIn = 0;
            this.Full = false;
            this.Cancelled = false;
        }

        public static async Task OrderUpdate(ApplicationDbContext _newContext, string urlString, int EID)
        {
            // get request to Eventbrite
            var text = new EBGet(urlString);
            // deserializing json to Atendee list.
            var visitors = JsonConvert.DeserializeObject<Attendees>(text.responseE);

            // finds the latest added order or 0
            var lastOrder = _newContext.Orders.Count() == 0 ? 0 : _newContext.Orders.Last().Number;

            // remove all existing orders
            var query = visitors.attendees
                .Where(s => 
                    Int32.Parse(s.order_id) > lastOrder 
                    && !s.cancelled 
                    && !s.refunded)
                .GroupBy(x => x.order_id)
                .Select(y => new { 
                    Number = y.Key, 
                    Count = y.Count()})
                .OrderBy(o => o.Number).ToList();

            var queryDelete = visitors.attendees
                .Where(s => s.cancelled || s.refunded)
                .GroupBy(x => x.order_id)
                .Select(y => new { 
                    Number = y.Key, 
                    Count = y.Count()})
                .OrderBy(o => o.Number).ToList();
            
            // create temporary container 
            List<Order> orders = new List<Order>();

            // update orders
            if (query != null)
            {
                // create and collect new orders
                foreach(var a in query)
                {
                    Attendee attendee = visitors.attendees
                        .FirstOrDefault(x => x.order_id == a.Number);
                    
                    var order = new Order();
                    order.Number = Int32.Parse(a.Number);
                    order.Amount = a.Count;
                    order.EID = EID;
                    order.OwnerName = attendee.profile.name;
                    order.OwnerEmail = attendee.profile.email;
                    order.Date = attendee.created.Substring(0,10);
                    order.Time = attendee.created.Substring(11,5);
                    order.TicketType = attendee.ticket_class_name;
                    orders.Add(order);
                }
                
                // add new orders
                Event e = await _newContext.Events
                    .Include(x => x.Orders)
                    .FirstOrDefaultAsync(x => x.ID == EID);
                e.Orders.AddRange(orders);
                
                //update db
                _newContext.Events.Update(e);

                // reset container
                orders = new List<Order>();
                
                // collect refunded and cancelled
                foreach(Order o in e.Orders)
                    foreach(var a in queryDelete)
                        if (a.Number == o.Number.ToString())
                            orders.Add(o);

                // remove refunded and cancelled
                _newContext.Orders.RemoveRange(orders);
            }
            // save changes
            await _newContext.SaveChangesAsync();
        }

        public static async Task Increment (string number, ApplicationDbContext _db)
        {
            Order o = await _db.Orders.FirstOrDefaultAsync(x => x.Number.ToString() == number);
			o.CheckedIn++;
            o.Full = o.CheckedIn >= o.Amount ? true : false;
            _db.Orders.Update(o);
            await _db.SaveChangesAsync();
        }
    }
}
                   