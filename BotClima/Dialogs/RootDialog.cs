using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
//using
using Newtonsoft.Json;
using System.Net.Http;

namespace ClimaBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string city;
        private string state;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        internal static async void GetCityID(string cidade, string estado)
        {
            var uri = $"http://apiadvisor.climatempo.com.br/api/v1/locale/city?name={cidade}&state={estado}&token=ce32a735c1d3d1a0c93313c53af6e011";
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string cidadePartialJsonString = await response.Content.ReadAsStringAsync();
                        cidadePartialJsonString = cidadePartialJsonString.Replace("[", "");
                        cidadePartialJsonString = cidadePartialJsonString.Replace("]", "");
                        var citydata = JsonConvert.DeserializeObject<Deserialize>(cidadePartialJsonString);
                        await GetDataByID(citydata.Id);
                    }
                }
            }
        }

        internal static async Task GetDataByID(long id)
        {
            var uri = $"http://apiadvisor.climatempo.com.br/api/v1/forecast/locale/{id}/days/15?token=ce32a735c1d3d1a0c93313c53af6e011";
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var dadosJsonString = await response.Content.ReadAsStringAsync();
                        var welcome = Deserialize.FromJson(dadosJsonString);
                    }
                }
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;
            if((length>0) && (activity.Text.Contains(",")))
            {
                string[] splitString = activity.Text.Split(',');
                city = splitString[0];
                state = splitString[1].Trim(' ');
                GetCityID(city, state);
            }
            

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }
    }
}