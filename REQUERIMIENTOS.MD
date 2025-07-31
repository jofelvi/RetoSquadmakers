EJERCICIO 2 - Gestión de Chistes
*
Validaremos tu ingenio para poder hacer consumo con APIs de terceros. Además, deberás poder gestionar todos los chistes de un usuario previamente autenticado.

NOTA: A partir de este punto, todos los endpoints deberán estar protegido con JWT (rol: "user" o "admin").

Fuentes Externas:
https://api.chucknorris.io/
https://icanhazdadjoke.com/api (Requiere el encabezado Accept: application/json)
ENDPOINTS:

GET /api/chistes/aleatorio:
Devuelve un chiste aleatorio. Si se pasa un query param origen con valor "Chuck" o "Dad", se obtiene de la API correspondiente. Si el valor no es válido, se devuelve un error. Si no se especifica origen, puede devolver un chiste de forma aleatoria entre las fuentes externas o locales.
GET /api/chistes/emparejados:
- Protegido con JWT (rol: "user" o "admin").
- Lanza en paralelo 5 peticiones a Chuck Norris API y 5 a Dad Joke API.
- Empareja los resultados 1 a 1.
- Devuelve un JSON con una lista de objetos con la siguiente estructura:
            [
              {
                 "chuck": "Chuck Norris counted to infinity. Twice.",
                 "dad": "Why did the math book look sad? Because it had too many problems.",
                 "combinado": "Chuck Norris counted to infinity. Also, the math book had too many problems."
              },
              ...
            ]
            - El campo combinado debe ser una composición creativa de ambos chistes.
           - Requisitos: Uso de HttpClientFactory, async/await, Task.WhenAll, manejo de errores y ILogger.
GET /api/chistes/combinado:
- Combina fragmentos de varios chistes obtenidos de APIs externas y/o chistes locales para generar uno nuevo. Sé creativo en la composición.
Chistes Almacenados Localmente:
POST /api/chistes:
- Guarda un nuevo chiste en la base de datos. El autor del chiste será el usuario autenticado.
GET /api/chistes/filtrar:
- Recibe query params como minPalabras, contiene, autorId, tematicaId.
- Devuelve una lista de chistes que cumplan con los criterios de filtro (ej. chistes locales que contengan más de 5 palabras y la palabra "Chuck", o chistes de un autor específico, etc.).
PUT /api/chistes/{id}:
- Actualiza el texto de un chiste existente por su id. Asegúrate de que solo el autor o un administrador puedan actualizarlo.
DELETE /api/chistes/{id}:
- Elimina un chiste por su id. Asegúrate de que solo el autor o un administrador puedan eliminarlo.

Objetivo: Validar comprensión y uso de LINQ (Where, Select, Any, Count, OrderBy, etc.) y la protección de endpoints por medio de JWT.

EJERCICIO 3 - Sistema de Notificaciones con Inyección de Dependencias
*
Implementar un sistema sencillo de envío de notificaciones (por email o SMS) que demuestre la correcta aplicación de:
Inyección de Dependencias en .NET Core.
Principio de Inversión de Dependencias (D de SOLID).
Separación de lógica de infraestructura de la lógica de negocio.
Capacidad de testear componentes desacoplados.
Tareas:
Definir una interfaz INotificador que permita enviar un mensaje a un destinatario.
Crear dos implementaciones de esa interfaz:
- EmailNotificador: que simule el envío de correos electrónicos (ej. con Console.WriteLine o ILogger).
- SmsNotificador: que simule el envío de mensajes SMS (ej. con Console.WriteLine o ILogger).
Desarrollar un servicio ServicioDeAlertas que dependa de INotificador y lo utilice para enviar un mensaje de alerta.
Configurar la inyección de dependencias en el contenedor de .NET Core, de modo que el sistema utilice por defecto el EmailNotificador.
(Opcional): Hacer que el canal de notificación (email o SMS) sea configurable desde appsettings.json.
Bonus: Crear un test unitario que pruebe el ServicioDeAlertas usando un mock de INotificador.

Endpoint para Notificaciones:
POST /api/notificaciones/enviar:
- Protegido con JWT (rol: "admin").
- Recibe en el cuerpo de la petición JSON:
  {
    "destinatario": "ejemplo@dominio.com",
    "mensaje": "Este es un mensaje de prueba.",
    "tipoNotificacion": "email" // o "sms"
  }
- Utiliza el ServicioDeAlertas para enviar la notificación según el tipoNotificacion especificado.
- Ejemplo de aplicación: Un administrador podría usar este endpoint para enviar notificaciones a usuarios sobre nuevos chistes, actualizaciones, etc.

Objetivo: Implementación y uso de servicios desacoplados del modelo de negocio, definición, consumo y validación de funcionalidad.

https://icanhazdadjoke.com/api#api-response-format

esta es la informacion que me da la api de dad joke
API
icanhazdadjoke.com can be used as an API for fetching a random joke, a specific joke, or searching for jokes in a variety of formats.

Calling the API
Authentication
No authentication is required to use the icanhazdadjoke.com API. Enjoy :)

API response format
All API endpoints follow their respective browser URLs, but we adjust the response formatting to be more suited for an API based on the provided HTTP Accept header.

Accepted Accept headers:

text/html - HTML response (default response format)
application/json - JSON response
text/plain - Plain text response
Note: Requests made via curl which do not set an Accept header will respond with text/plain by default.
Endpoints
Fetch a random dad joke
GET https://icanhazdadjoke.com/ fetch a random dad joke.

Fetching a random joke as JSON:

$ curl -H "Accept: application/json" https://icanhazdadjoke.com/
{
  "id": "R7UfaahVfFd",
  "joke": "My dog used to chase people on a bike a lot. It got so bad I had to take his bike away.",
  "status": 200
}

https://icanhazdadjoke.com/search?term=ss

https://api.chucknorris.io/

https://api.chucknorris.io/jokes/categories =  ["animal","career","celebrity","dev","explicit","fashion","food","history","money","movie","music","political","religion","science","sport","travel"]

Usage
Retrieve a random chuck joke in JSON format.

https://api.chucknorris.io/jokes/random
Example response:

{
"icon_url" : "https://api.chucknorris.io/img/avatar/chuck-norris.png",
"id" : "fQGz8GRiRYy9heGwbmA1zg",
"url" : "",
"value" : "Chuck Norris once walked into a bar and said, "My c*ck is two inches". The bartender laughed and Chuck Norris said, "From the Ground"."
}
Get me a new one ... (press "r" to refresh)
Retrieve a random chuck norris joke from a given category.

https://api.chucknorris.io/jokes/random?category={category}
Retrieve a list of available categories.

https://api.chucknorris.io/jokes/categories
Free text search.

https://api.chucknorris.io/jokes/search?query={query}


EJERCICIO 4 - Endpoint Matemático (Opcional si el tiempo es limitado)
Crea 2 endpoints matemáticos para poder gestionar valores ofrecidos por el usuario.

NOTA: A partir de este punto, todos los endpoints deberán estar protegido con JWT (rol: "user" o "admin").

ENDPOINTS:

GET /api/matematicas/mcm:
- Recibe un query param numbers con una lista de números enteros (ej. ?numbers=1,2,3,4).
- La respuesta debe ser el mínimo común múltiplo (MCM) de esos números.
GET /api/matematicas/siguiente-numero:
- Recibe un query param number con un número entero.
- La respuesta será ese número + 1.

Objetivo: Validar y dar uso a parámetros ofrecidos por un cliente.