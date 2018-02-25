using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
//using
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace ClimaBot.Dialogs
{
    [LuisModel("2fed9e6e-0061-4cf0-a4fc-59bb78bcca0d", "3bf3531c2c834a39a7c4de1052cee987")]
    [Serializable]
    public class LocalidadeDialog : LuisDialog<object>  
    {
        private string city;
        private string state;
        private const int numDaysForecast = 3;

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
            await context.PostAsync("Este aplicativo foi desenvolvido por Sergio Luiz Machado para a maratona de programação Bots.");
            context.Wait(MessageReceived);
        }

        public bool TryFindTitle(LuisResult result, out ICollection<object> title)
        {
            if (result.TryFindEntity("city", out EntityRecommendation entity))
            {
                title = entity.Resolution.Values;
                return true;
            }
            title = null;
            return false;
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
                    List<string> stateList = (List<string>)JsonConvert.DeserializeObject(JSonState, typeof(List<string>));
                    state = stateList[0].ToString();
                    await context.PostAsync($"Obtendo informações para {city}, {state}");
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
                                            await context.PostAsync($"Previsão do tempo para {city}, {state}");
                                            for (int days = 0; days < numDaysForecast; days++)
                                            {
                                                await context.PostAsync($"Data: {forecast.Data[days].DateBr}");
                                                await context.PostAsync($"Humidade - Mínima: {forecast.Data[days].Humidity.Min}; Máxima: {forecast.Data[days].Humidity.Max}");
                                                await context.PostAsync($"Chuva - Precipitação: {forecast.Data[days].Rain.Precipitation}; Probabilidade: {forecast.Data[days].Rain.Probability}%");
                                                await context.PostAsync($"Temperatura - Mínima: {forecast.Data[days].Temperature.Min}°; Máxima: {forecast.Data[days].Temperature.Max}°");
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