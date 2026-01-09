using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace UnBosqueParaJuan.Controllers
{
    public class Recaptcha
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public Recaptcha(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public bool ReCaptchaPassed(string gRecaptchaResponse)
        {
            if (string.IsNullOrEmpty(gRecaptchaResponse))
                return false;

            string? secretKey = _configuration["GoogleRecaptcha:SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Google reCAPTCHA secret key not configured");
            }

            try
            {
                var res = _httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={gRecaptchaResponse}").Result;
                
                if (res.StatusCode != HttpStatusCode.OK)
                    return false;

                string JSONres = res.Content.ReadAsStringAsync().Result;
                dynamic? JSONdata = JObject.Parse(JSONres);
                
                if (JSONdata == null)
                    return false;

                string respuestaPeticion = JSONdata.success?.ToString() ?? "false";
                decimal puntajePeticion = JSONdata.score != null ? (decimal)JSONdata.score : 0;
                
                if (respuestaPeticion.ToLower() != "true" && puntajePeticion < 0.5m)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}