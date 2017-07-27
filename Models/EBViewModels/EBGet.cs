using System.Net.Http;
using System.Threading.Tasks;

namespace GlobalEvent.Models.EBViewModels
{
    public class EBGet
	{
		public string responseE { get; set;}
        public string url { get; set; }

        public EBGet(string urlString)
        {
            this.url = urlString;
            SendRequest().Wait();
        }
        
        private async Task SendRequest()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(this.url);
                    response.EnsureSuccessStatusCode(); // Throw if not success

                    responseE = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    responseE = ($"Request exception: {e.Message}");
                }
            }
        }
	}
}