using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
//using
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace ClimaBot.Dialogs
{
    [Serializable]
    public class LocalidadeDialog : LuisDialog<object>  
    {
        private string city;
        private string state;
        private string forecastMsg;
        private const int numDaysForecast = 3;

        public LocalidadeDialog(ILuisService service) : base(service) { }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"O significado de '{result.Query}' é desconhecido neste momento.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Sobre")]
        public async Task Sobre(IDialogContext context, LuisResult result)
        {
            string aboutString = string.Format("SLMClimaBot v0.1>>>>Maratona Bots Brasil\n\r");
            aboutString = aboutString + string.Format("Desenvolvido por Sergio Luiz Machado\n\r");
            aboutString = aboutString + string.Format("Telegram: @SLMClimaBot\n\r");
            aboutString = aboutString + string.Format("Skype: https://join.skype.com/bot/0674751f-91d7-467a-b425-bcf354018896\n\r");
            await context.PostAsync(aboutString);
            context.Wait(MessageReceived);
        }

        [LuisIntent("BuscaPrevisao")]
        public async Task BuscaPrevisao(IDialogContext context, LuisResult result)
        {
            if (result.TryFindEntity("city", out EntityRecommendation entity))
            {
                city = entity.Entity.ToString();
                if (entity.Resolution.Count>0)
                {
                    entity.Resolution.TryGetValue("values", out object objstate);
                    string JSonState = JsonConvert.SerializeObject(objstate);
                    List<string> states = (List<string>)JsonConvert.DeserializeObject(JSonState, typeof(List<string>));
                    state = states[0].ToString();

                    await context.PostAsync($"Procurando...");
                    var uri = $"http://apiadvisor.climatempo.com.br/api/v1/locale/city?name={city}&state={state}&token=ce32a735c1d3d1a0c93313c53af6e011";
                    using (var client = new HttpClient())
                    {
                        using (var response = await client.GetAsync(uri))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string cityJsonString = await response.Content.ReadAsStringAsync();
                                cityJsonString = cityJsonString.Replace("[", "").Replace("]", "");

                                var citydata = Deserialize.FromJson(cityJsonString);
                                var id = citydata.Id;

                                var uriForeCast = $"http://apiadvisor.climatempo.com.br/api/v1/forecast/locale/{id}/days/15?token=ce32a735c1d3d1a0c93313c53af6e011";
                                using (var cliForeCast = new HttpClient())
                                {
                                    using (var respForeCast = await cliForeCast.GetAsync(uriForeCast))
                                    {
                                        if (respForeCast.IsSuccessStatusCode)
                                        {
                                            var forecastDataJsonString = await respForeCast.Content.ReadAsStringAsync();
                                            var forecast = Deserialize.FromJson(forecastDataJsonString);
                                            await context.PostAsync($"Previsão do tempo para {city.ToUpper()}, {state.ToUpper()}");

                                            forecastMsg = "";
                                            for (int days = 0; days < numDaysForecast; days++)
                                            {
                                                forecastMsg = forecastMsg + string.Format($"Data: {forecast.Data[days].DateBr}\n\r");
                                                forecastMsg = forecastMsg + string.Format($"Humidade - Mínima: {forecast.Data[days].Humidity.Min}; Máxima: {forecast.Data[days].Humidity.Max}\n\r");
                                                forecastMsg = forecastMsg + string.Format($"Chuva - Precipitação: {forecast.Data[days].Rain.Precipitation}; Probabilidade: {forecast.Data[days].Rain.Probability}%\n\r");
                                                forecastMsg = forecastMsg + string.Format($"Temperatura - Mínima: {forecast.Data[days].Temperature.Min}°C; Máxima: {forecast.Data[days].Temperature.Max}°C\n\n");
                                                await context.PostAsync(forecastMsg);
                                                forecastMsg = "";
                                            }                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }
    }
}