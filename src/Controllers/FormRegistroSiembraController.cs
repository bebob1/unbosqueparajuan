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
        // RECAPTCHA DISABLED - private readonly Recaptcha _recaptcha;
        private readonly ILogger<FormRegistroSiembraController> _logger;

        public FormRegistroSiembraController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IContentService contentService,
            IPublishedContentQuery publishedContentQuery,
            // RECAPTCHA DISABLED - Recaptcha recaptcha,
            ILogger<FormRegistroSiembraController> logger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentService = contentService;
            _publishedContentQuery = publishedContentQuery;
            // RECAPTCHA DISABLED - _recaptcha = recaptcha;
            _logger = logger;
            
            _logger.LogInformation("=== FormRegistroSiembraController INSTANTIATED ===");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FormRegistroSiembra(FormRegistroSiembra model)
        {
            _logger.LogInformation("=== FormRegistroSiembra POST METHOD CALLED ===");
            _logger.LogInformation("Model data - Nombre: {Nombre}, Email: {Email}, Fecha: {Fecha}", 
                model?.Nombre, model?.Email, model?.Fecha);
            
            // TEMPORARILY DISABLED FOR TESTING
            /*
            var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();
            _logger.LogDebug("Recaptcha response received: {HasResponse}", !string.IsNullOrEmpty(recaptchaResponse));
            
            if (!_recaptcha.ReCaptchaPassed(recaptchaResponse))
            {
                _logger.LogWarning("Recaptcha validation FAILED");
                ModelState.AddModelError(string.Empty, "Validación de captcha incorrecta, por favor intente nuevamente");
                return CurrentUmbracoPage();
            }
            
            _logger.LogInformation("Recaptcha validation PASSED");
            */
            _logger.LogInformation("Recaptcha validation SKIPPED (disabled for testing)");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is INVALID");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("ModelState error: {Error}", error.ErrorMessage);
                }
                return CurrentUmbracoPage();
            }
            
            _logger.LogInformation("ModelState is VALID");

            try
            {
                TempData["success"] = true;
                
                _logger.LogInformation("Searching for content nodes...");
                var rootNodes = _publishedContentQuery.ContentAtRoot();
                _logger.LogInformation("Root nodes count: {Count}", rootNodes?.Count() ?? 0);
                
                var home = rootNodes?.FirstOrDefault(x => x.ContentType.Alias == "inicio");
                _logger.LogInformation("Home node found: {Found}, ID: {Id}", home != null, home?.Id);
                
                var formularioRegistroSiembra = home?.Children?.FirstOrDefault(x => x.ContentType.Alias == "formularioRegistroSiembra");
                _logger.LogInformation("FormularioRegistroSiembra node found: {Found}, ID: {Id}", 
                    formularioRegistroSiembra != null, formularioRegistroSiembra?.Id);

                if (formularioRegistroSiembra == null)
                {
                    _logger.LogError("FormularioRegistroSiembra content node NOT FOUND in content tree");
                    _logger.LogError("Available children of home: {Children}", 
                        string.Join(", ", home?.Children?.Select(c => c.ContentType.Alias) ?? Array.Empty<string>()));
                    ModelState.AddModelError("", "No se encontró el formulario de registro");
                    return CurrentUmbracoPage();
                }

                string nombre_nodo_item_siembra = model.Nombre + "-" + model.Numero_Arboles + "-" + model.Fecha;
                _logger.LogInformation("Creating new item: {NodeName}", nombre_nodo_item_siembra);
                
                var parentNode = _contentService.GetById(formularioRegistroSiembra.Id);
                if (parentNode == null)
                {
                    _logger.LogError("Parent node NOT FOUND by ID: {Id}", formularioRegistroSiembra.Id);
                    ModelState.AddModelError("", "Error al procesar la solicitud");
                    return CurrentUmbracoPage();
                }
                
                _logger.LogInformation("Parent node found: {Name}", parentNode.Name);

                var itemRegistro = _contentService.Create(nombre_nodo_item_siembra, parentNode.Key, "itemRegistro");
                _logger.LogInformation("Item created with ID: {Id}", itemRegistro.Id);
                
                itemRegistro.SetValue("nombre", model.Nombre);
                itemRegistro.SetValue("email", model.Email);
                DateTime fechis = DateTime.Parse(model.Fecha);
                itemRegistro.SetValue("fecha", fechis);
                itemRegistro.SetValue("lugarDeLaSiembra", model.Lugar);
                int num_arboles = 0; 
                int.TryParse(model.Numero_Arboles, out num_arboles);
                itemRegistro.SetValue("numeroDeArboles", num_arboles);
                itemRegistro.SetValue("especies", model.Especies);
                
                _logger.LogInformation("All values set, saving...");

                var saveResult = _contentService.Save(itemRegistro);
                _logger.LogInformation("Save result: {Result}", saveResult.Success);
                
                var respuestaCreacion = _contentService.Publish(itemRegistro, Array.Empty<string>());
                _logger.LogInformation("Publish result: {Result}", respuestaCreacion.Success);

                if (respuestaCreacion.Success)
                {
                    _logger.LogInformation("=== FORM SUBMISSION SUCCESSFUL ===");
                    TempData["success"] = true;
                }
                else
                {
                    _logger.LogError("Publish FAILED: {Messages}", 
                        string.Join(", ", respuestaCreacion.EventMessages.GetAll().Select(m => m.Message)));
                    ModelState.AddModelError("", "Error al procesar la solicitud, por favor intente nuevamente");
                    return CurrentUmbracoPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== EXCEPTION in FormRegistroSiembra: {Message} ===", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                ModelState.AddModelError("", "Error al procesar la solicitud, por favor intente nuevamente");
                return CurrentUmbracoPage();
            }

            _logger.LogInformation("Redirecting to current Umbraco page");
            return RedirectToCurrentUmbracoPage();
        }
    }
}