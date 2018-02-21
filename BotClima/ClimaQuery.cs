using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using
using Newtonsoft.Json;
using System.Net.Http;

namespace ClimaBot
{
    public partial class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, ClimaBot.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, ClimaBot.Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };

        internal static async void ObterIDCidade(string cidade, string estado)
        {
            var uri = $"http://apiadvisor.climatempo.com.br/api/v1/locale/city?name={cidade}&state={estado}&token=ce32a735c1d3d1a0c93313c53af6e011";
            string urix = uri.Replace(" ", "%20");
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(urix))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string cidadePartialJsonString = await response.Content.ReadAsStringAsync();
                        cidadePartialJsonString = cidadePartialJsonString.Replace("[", "");
                        cidadePartialJsonString = cidadePartialJsonString.Replace("]", "");
                        var citydata = JsonConvert.DeserializeObject<Welcome>(cidadePartialJsonString);
                        ObterDadosPorID(citydata.Id);
                    }
                }
            }
        }

        internal static async void ObterDadosPorID(long id)
        {
            var uri = $"http://apiadvisor.climatempo.com.br/api/v1/forecast/locale/{id}/days/15?token=ce32a735c1d3d1a0c93313c53af6e011";
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var dadosJsonString = await response.Content.ReadAsStringAsync();
                        var welcome = Welcome.FromJson(dadosJsonString);
                    }
                }
            }
        }
    }
}