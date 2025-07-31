namespace retoSquadmakers.Presentation.Controllers;

public static class HomeController
{
    public static void MapHomeEndpoints(this WebApplication app)
    {
        app.MapGet("/", GetHomeAsync)
            .WithName("Home")
            .WithOpenApi()
            .WithSummary("Endpoint principal de la API");
    }

    private static IResult GetHomeAsync()
    {
        return Results.Ok(new
        {
            nombre = "API de Chistes - RetoSquadmakers",
            version = "1.0.0",
            descripcion = "Sistema de Autenticación con OAuth 2.0 y JWT",
            endpoints = new
            {
                swagger = "/swagger",
                login = "/api/auth/login",
                googleLogin = "/api/auth/external/google-login",
                createUser = "/api/auth/create-user",
                userInfo = "/api/usuario",
                admin = "/api/admin"
            }
        });
    }
}