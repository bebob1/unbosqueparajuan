using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Persistence;
using UnBosqueParaJuan.Models;
using Umbraco.Cms.Core.Web;

namespace UnBosqueParaJuan.Controllers
{
    public class FormRegistroSiembraController : SurfaceController
    {
        private readonly IContentService _contentService;
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly Recaptcha _recaptcha;

        public FormRegistroSiembraController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IContentService contentService,
            IPublishedContentQuery publishedContentQuery,
            Recaptcha recaptcha)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentService = contentService;
            _publishedContentQuery = publishedContentQuery;
            _recaptcha = recaptcha;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FormRegistroSiembra(FormRegistroSiembra model)
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
            if (!_recaptcha.ReCaptchaPassed(recaptchaResponse))
            {
                ModelState.AddModelError(string.Empty, "Validación de captcha incorrecta, por favor intente nuevamente");
                return CurrentUmbracoPage();
            }

            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            try
            {
                TempData["success"] = true;
                
                var rootNodes = _publishedContentQuery.ContentAtRoot();
                var home = rootNodes?.FirstOrDefault(x => x.ContentType.Alias == "inicio");
                var formularioRegistroSiembra = home?.Children?.FirstOrDefault(x => x.ContentType.Alias == "formularioRegistroSiembra");

                if (formularioRegistroSiembra == null)
                {
                    ModelState.AddModelError("", "No se encontró el formulario de registro");
                    return CurrentUmbracoPage();
                }

                string nombre_nodo_item_siembra = model.Nombre + "-" + model.Numero_Arboles + "-" + model.Fecha;
                
                var parentNode = _contentService.GetById(formularioRegistroSiembra.Id);
                if (parentNode == null)
                {
                    ModelState.AddModelError("", "Error al procesar la solicitud");
                    return CurrentUmbracoPage();
                }

                var itemRegistro = _contentService.Create(nombre_nodo_item_siembra, parentNode.Key, "itemRegistro");
                itemRegistro.SetValue("nombre", model.Nombre);
                itemRegistro.SetValue("email", model.Email);
                DateTime fechis = DateTime.Parse(model.Fecha);
                itemRegistro.SetValue("fecha", fechis);
                itemRegistro.SetValue("lugarDeLaSiembra", model.Lugar);
                int num_arboles = 0; 
                int.TryParse(model.Numero_Arboles, out num_arboles);
                itemRegistro.SetValue("numeroDeArboles", num_arboles);
                itemRegistro.SetValue("especies", model.Especies);

                var saveResult = _contentService.Save(itemRegistro);
                var respuestaCreacion = _contentService.Publish(itemRegistro, Array.Empty<string>());

                if (respuestaCreacion.Success)
                {
                    TempData["success"] = true;
                }
                else
                {
                    ModelState.AddModelError("", "Error al procesar la solicitud, por favor intente nuevamente");
                    return CurrentUmbracoPage();
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error al procesar la solicitud, por favor intente nuevamente");
                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }
    }
}