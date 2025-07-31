using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace retoSquadmakers.Presentation.Controllers;

[ApiController]
[Route("api/matematicas")]
[Authorize] // Todos los endpoints requieren autenticación JWT (rol: "user" o "admin")
public class MatematicasController : ControllerBase
{
    private readonly ILogger<MatematicasController> _logger;

    public MatematicasController(ILogger<MatematicasController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /api/matematicas/mcm - Calcula el mínimo común múltiplo (MCM) de una lista de números
    /// </summary>
    /// <param name="numbers">Lista de números enteros separados por comas (ej. 1,2,3,4)</param>
    /// <returns>El mínimo común múltiplo de los números proporcionados</returns>
    [HttpGet("mcm")]
    public IActionResult CalcularMCM([FromQuery] string numbers)
    {
        try
        {
            _logger.LogInformation("Calculando MCM para números: {Numbers}", numbers);

            if (string.IsNullOrWhiteSpace(numbers))
                return BadRequest(new { error = "El parámetro 'numbers' es requerido" });

            // Parsear los números de la cadena
            var numerosList = new List<int>();
            var numerosArray = numbers.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (numerosArray.Length == 0)
                return BadRequest(new { error = "Debe proporcionar al menos un número" });

            foreach (var numeroStr in numerosArray)
            {
                if (!int.TryParse(numeroStr.Trim(), out int numero))
                    return BadRequest(new { error = $"'{numeroStr.Trim()}' no es un número entero válido" });

                if (numero <= 0)
                    return BadRequest(new { error = "Todos los números deben ser enteros positivos" });

                numerosList.Add(numero);
            }

            // Calcular el MCM
            long mcm = CalcularMCMDeNumeros(numerosList);

            _logger.LogInformation("MCM calculado: {MCM} para números: {Numbers}", mcm, string.Join(",", numerosList));

            return Ok(new 
            { 
                numbers = numerosList,
                mcm = mcm,
                message = $"El mínimo común múltiplo de [{string.Join(", ", numerosList)}] es {mcm}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular MCM para números: {Numbers}", numbers);
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/matematicas/siguiente-numero - Devuelve el siguiente número (número + 1)
    /// </summary>
    /// <param name="number">Número entero</param>
    /// <returns>El número + 1</returns>
    [HttpGet("siguiente-numero")]
    public IActionResult SiguienteNumero([FromQuery] int number)
    {
        try
        {
            _logger.LogInformation("Calculando siguiente número para: {Number}", number);

            int siguienteNumero = number + 1;

            _logger.LogInformation("Siguiente número calculado: {Number} + 1 = {SiguienteNumero}", number, siguienteNumero);

            return Ok(new 
            { 
                numeroOriginal = number,
                siguienteNumero = siguienteNumero,
                message = $"El siguiente número de {number} es {siguienteNumero}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular siguiente número para: {Number}", number);
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Calcula el mínimo común múltiplo de una lista de números
    /// </summary>
    /// <param name="numeros">Lista de números enteros positivos</param>
    /// <returns>El MCM de todos los números</returns>
    private static long CalcularMCMDeNumeros(List<int> numeros)
    {
        if (numeros.Count == 0)
            throw new ArgumentException("La lista no puede estar vacía");

        if (numeros.Count == 1)
            return numeros[0];

        long mcm = numeros[0];
        for (int i = 1; i < numeros.Count; i++)
        {
            mcm = CalcularMCMDeDosNumeros(mcm, numeros[i]);
        }

        return mcm;
    }

    /// <summary>
    /// Calcula el mínimo común múltiplo de dos números
    /// </summary>
    /// <param name="a">Primer número</param>
    /// <param name="b">Segundo número</param>
    /// <returns>MCM de a y b</returns>
    private static long CalcularMCMDeDosNumeros(long a, long b)
    {
        return Math.Abs(a * b) / CalcularMCD(a, b);
    }

    /// <summary>
    /// Calcula el máximo común divisor usando el algoritmo de Euclides
    /// </summary>
    /// <param name="a">Primer número</param>
    /// <param name="b">Segundo número</param>
    /// <returns>MCD de a y b</returns>
    private static long CalcularMCD(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}