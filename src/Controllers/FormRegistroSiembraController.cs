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
using Umbraco.Extensions;

namespace UnBosqueParaJuan.Controllers
{
    public class FormRegistroSiembraController : SurfaceController
    {
        private readonly IContentService _contentService;
        private readonly IPublishedContentQuery _publishedContentQuery;
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
            ILogger<FormRegistroSiembraController> logger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentService = contentService;
            _publishedContentQuery = publishedContentQuery;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult FormRegistroSiembra(FormRegistroSiembra model)
        {
            // Paso 1: Verificar petición
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  CONTROLLER METHOD CALLED - INICIO DEL PROCESO             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            
            Console.WriteLine($"✓ PASO 1: Información de la petición");
            Console.WriteLine($"   - Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"   - Request Path: {Request.Path}");
            Console.WriteLine($"   - Request Method: {Request.Method}");
            Console.WriteLine($"   - Content-Type: {Request.ContentType ?? "NULL"}");
            Console.WriteLine($"   - Has Form: {Request.HasFormContentType}");
            
            if (Request.HasFormContentType && Request.Form != null && Request.Form.Keys.Any())
            {
                Console.WriteLine("   📋 Valores del Form recibidos:");
                foreach (var key in Request.Form.Keys)
                {
                    var value = Request.Form[key].ToString();
                    Console.WriteLine($"      [{key}] = '{(string.IsNullOrEmpty(value) ? "EMPTY" : value)}'");
                }
            }
            else
            {
                Console.WriteLine("   ⚠ WARNING: Request.Form está vacío");
            }
            
            // Paso 2: Verificar modelo
            Console.WriteLine("✓ PASO 2: Verificando modelo recibido");
            if (model == null)
            {
                Console.WriteLine("   ✗ WARNING: El modelo es NULL");
                ModelState.AddModelError("", "No se recibieron datos del formulario");
                return CurrentUmbracoPage();
            }
            
            Console.WriteLine("   ✓ Modelo recibido correctamente");
            Console.WriteLine($"   📝 Valores del modelo:");
            Console.WriteLine($"      Nombre: '{model.Nombre ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Nombre) ? "❌" : "✓")}");
            Console.WriteLine($"      Email: '{model.Email ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Email) ? "❌" : "✓")}");
            Console.WriteLine($"      Fecha: '{model.Fecha ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Fecha) ? "❌" : "✓")}");
            Console.WriteLine($"      Lugar: '{model.Lugar ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Lugar) ? "❌" : "✓")}");
            Console.WriteLine($"      Numero_Arboles: '{model.Numero_Arboles ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Numero_Arboles) ? "❌" : "✓")}");
            Console.WriteLine($"      Especies: '{model.Especies ?? "NULL"}' {(string.IsNullOrWhiteSpace(model.Especies) ? "❌" : "✓")}");

            // Paso 3: Validar ModelState
            Console.WriteLine("✓ PASO 3: Validando ModelState");
            Console.WriteLine($"   - ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"   - Total Errores: {ModelState.ErrorCount}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("   ⚠ ModelState INVÁLIDO - Detalles:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"      Campo '{key}':");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"         - {error.ErrorMessage ?? "Sin mensaje"}");
                            if (error.Exception != null)
                            {
                                Console.WriteLine($"         - Exception: {error.Exception.Message}");
                            }
                        }
                    }
                }
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                return CurrentUmbracoPage();
            }
            
            Console.WriteLine("   ✓ ModelState VÁLIDO");

            try
            {
                // Paso 4: Buscar estructura de contenido
                Console.WriteLine("✓ PASO 4: Buscando estructura de contenido");
                var rootNodes = _publishedContentQuery.ContentAtRoot();
                Console.WriteLine($"   - Nodos raíz: {rootNodes?.Count() ?? 0}");
                
                if (rootNodes == null || !rootNodes.Any())
                {
                    Console.WriteLine("   ✗ No se encontraron nodos raíz");
                    ModelState.AddModelError("", "Error de configuración del sistema");
                    return CurrentUmbracoPage();
                }
                
                // Paso 5: Buscar nodo Home
                Console.WriteLine("✓ PASO 5: Buscando nodo 'Inicio'");
                var home = rootNodes.FirstOrDefault(x => x.ContentType.Alias == "Inicio");
                
                if (home == null)
                {
                    Console.WriteLine("   ✗ Nodo 'Inicio' NO encontrado");
                    Console.WriteLine($"   - Aliases: {string.Join(", ", rootNodes.Select(n => $"'{n.ContentType.Alias}'"))}");
                    ModelState.AddModelError("", "Error de configuración: página de inicio no encontrada");
                    return CurrentUmbracoPage();
                }
                
                Console.WriteLine($"   ✓ Nodo 'Inicio' encontrado (ID: {home.Id})");
                
                // Paso 6: Buscar nodo del formulario
                Console.WriteLine("✓ PASO 6: Buscando nodo 'formularioRegistroSiembra'");
                var formularioRegistroSiembra = home.Children()?.FirstOrDefault(x => x.ContentType.Alias == "formularioRegistroSiembra");
                
                if (formularioRegistroSiembra == null)
                {
                    Console.WriteLine("   ✗ Nodo 'formularioRegistroSiembra' NO encontrado");
                    var childAliases = home.Children()?.Select(c => $"'{c.ContentType.Alias}'") ?? Array.Empty<string>();
                    Console.WriteLine($"   - Aliases hijos: {string.Join(", ", childAliases)}");
                    ModelState.AddModelError("", "Error de configuración: contenedor de registros no encontrado");
                    return CurrentUmbracoPage();
                }
                
                Console.WriteLine($"   ✓ Nodo encontrado (ID: {formularioRegistroSiembra.Id})");

                // Paso 7: Preparar datos
                string nombre_nodo_item_siembra = $"{model.Nombre}-{model.Numero_Arboles}-{model.Fecha}";
                Console.WriteLine($"✓ PASO 7: Nombre del nodo: '{nombre_nodo_item_siembra}'");
                
                // Paso 8: Obtener nodo padre
                Console.WriteLine("✓ PASO 8: Obteniendo nodo padre");
                var parentNode = _contentService.GetById(formularioRegistroSiembra.Id);
                
                if (parentNode == null)
                {
                    Console.WriteLine($"   ✗ No se pudo obtener nodo con ID {formularioRegistroSiembra.Id}");
                    ModelState.AddModelError("", "Error interno al procesar la solicitud");
                    return CurrentUmbracoPage();
                }
                
                Console.WriteLine($"   ✓ Nodo padre obtenido");

                // Paso 9: Crear item
                Console.WriteLine("✓ PASO 9: Creando nuevo nodo 'itemRegistro'");
                var itemRegistro = _contentService.Create(nombre_nodo_item_siembra, parentNode.Key, "itemRegistro");
                Console.WriteLine($"   ✓ Nodo creado (ID: {itemRegistro.Id})");
                
                // Paso 10: Asignar valores
                Console.WriteLine("✓ PASO 10: Asignando valores");
                
                itemRegistro.SetValue("nombre", model.Nombre);
                Console.WriteLine($"   ✓ nombre = '{model.Nombre}'");
                
                itemRegistro.SetValue("email", model.Email);
                Console.WriteLine($"   ✓ email = '{model.Email}'");
                
                DateTime fechis = DateTime.Parse(model.Fecha);
                itemRegistro.SetValue("fecha", fechis);
                Console.WriteLine($"   ✓ fecha = {fechis:yyyy-MM-dd}");
                
                itemRegistro.SetValue("lugarDeLaSiembra", model.Lugar);
                Console.WriteLine($"   ✓ lugarDeLaSiembra = '{model.Lugar}'");
                
                int num_arboles = int.TryParse(model.Numero_Arboles, out var parsed) ? parsed : 0;
                itemRegistro.SetValue("numeroDeArboles", num_arboles);
                Console.WriteLine($"   ✓ numeroDeArboles = {num_arboles}");
                
                itemRegistro.SetValue("especies", model.Especies);
                Console.WriteLine($"   ✓ especies = '{model.Especies}'");

                // Paso 11: Guardar
                Console.WriteLine("✓ PASO 11: Guardando nodo");
                var saveResult = _contentService.Save(itemRegistro);
                Console.WriteLine($"   - Save Success: {saveResult.Success}");
                
                if (!saveResult.Success)
                {
                    Console.WriteLine("   ✗ Save FALLÓ");
                    var messages = saveResult.EventMessages.GetAll().Select(m => m.Message).ToArray();
                    Console.WriteLine($"   - Mensajes: {string.Join(" | ", messages)}");
                    ModelState.AddModelError("", "Error al guardar la información");
                    return CurrentUmbracoPage();
                }
                
                Console.WriteLine("   ✓ Guardado exitoso");
                
                // Paso 12: Publicar
                Console.WriteLine("✓ PASO 12: Publicando nodo");
                var respuestaCreacion = _contentService.Publish(itemRegistro, Array.Empty<string>());
                Console.WriteLine($"   - Publish Success: {respuestaCreacion.Success}");

                if (!respuestaCreacion.Success)
                {
                    Console.WriteLine("   ✗ Publish FALLÓ");
                    var publishMessages = respuestaCreacion.EventMessages.GetAll().Select(m => m.Message).ToArray();
                    Console.WriteLine($"   - Mensajes: {string.Join(" | ", publishMessages)}");
                    ModelState.AddModelError("", "Error al publicar la información");
                    return CurrentUmbracoPage();
                }
                
                Console.WriteLine("   ✓ Publicación exitosa");
                Console.WriteLine($"   ✓ Registro ID {itemRegistro.Id} creado");

                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ✓✓✓ PROCESO COMPLETADO EXITOSAMENTE ✓✓✓                  ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                
                TempData["success"] = true;
                return CurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ✗✗✗ EXCEPCIÓN CAPTURADA ✗✗✗                              ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return CurrentUmbracoPage();
            }
        }
    }
}