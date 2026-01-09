using System.ComponentModel.DataAnnotations;

namespace UnBosqueParaJuan.Models
{
    public class FormRegistroSiembra
    {
        [Required(ErrorMessage = "Ingrese su nombre completo")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese un email")]
        [Display(Name = "Correo")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese una fecha")]
        [Display(Name = "Fecha")]
        public string Fecha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese el lugar de siembra")]
        [Display(Name = "Lugar de la siembra")]
        public string Lugar { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese el número de árboles")]
        [Display(Name = "Número de árboles")]
        public string Numero_Arboles { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese especie")]
        [Display(Name = "Especies")]
        public string Especies { get; set; } = string.Empty;
    }
}