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
        private const int numDaysForecast = 3;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;            
            int length = (activity.Text ?? string.Empty).Length;

            if((length>0) && (activity.Text.Contains(",")))
            {
                string[] splitString = activity.Text.Split(',');
                city = splitString[0];
                state = splitString[1].Trim(' ');

                var uri = $"http://apiadvisor.climatempo.com.br/api/v1/locale/city?name={city}&state={state}&token=ce32a735c1d3d1a0c93313c53af6e011";
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string cityJsonString = await response.Content.ReadAsStringAsync();
                            cityJsonString = cityJsonString.Replace("[", "");
                            cityJsonString = cityJsonString.Replace("]", "");
                            var citydata = JsonConvert.DeserializeObject<Deserialize>(cityJsonString);
                            var id = citydata.Id;

                            var uriForeCast = $"http://apiadvisor.climatempo.com.br/api/v1/forecast/locale/{id}/days/15?token=ce32a735c1d3d1a0c93313c53af6e011";
                            using (var cliForeCast = new HttpClient())
                            {
                                using (var respForeCast = await cliForeCast.GetAsync(uriForeCast))
                                {
                                    if (respForeCast.IsSuccessStatusCode)
                                    {
                                        var dataJsonString = await respForeCast.Content.ReadAsStringAsync();
                                        var welcome = Deserialize.FromJson(dataJsonString);
                                        await context.PostAsync($"Previsão do tempo para {city}, {state}");
                                        for(int days = 0; days < numDaysForecast; days++)
                                        {
                                            await context.PostAsync($"Data: {welcome.Data[days].DateBr}");
                                            await context.PostAsync($"Humidade - Mínima: {welcome.Data[days].Humidity.Min}; Máxima: {welcome.Data[days].Humidity.Max}");
                                            await context.PostAsync($"Chuva - Precipitação: {welcome.Data[days].Rain.Precipitation}; Probabilidade: {welcome.Data[days].Rain.Probability}%");
                                            await context.PostAsync($"Temperatura - Mínima: {welcome.Data[days].Temperature.Min}°; Máxima: {welcome.Data[days].Temperature.Max}°");
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                await context.PostAsync("Formato: Cidade, Estado/nExemplo: Rio de Janeiro, RJ");
            }         

            context.Wait(MessageReceivedAsync);
        }
    }
}