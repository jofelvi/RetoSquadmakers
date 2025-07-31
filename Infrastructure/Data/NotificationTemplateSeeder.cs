using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Infrastructure.Persistence;

namespace retoSquadmakers.Infrastructure.Data;

public static class NotificationTemplateSeeder
{
    public static async Task SeedNotificationTemplatesAsync(ApplicationDbContext context)
    {
        // Check if templates already exist
        if (await context.NotificationTemplates.AnyAsync())
            return;

        var templates = new[]
        {
            new NotificationTemplate
            {
                TemplateId = "chiste_created_author",
                Name = "Chiste Publicado - Autor",
                Type = "Email",
                Subject = "¡Tu chiste ha sido publicado exitosamente!",
                Content = @"¡Hola {{authorName}}!

Tu chiste ha sido publicado exitosamente en nuestra plataforma de humor.

📝 **Tu chiste:**
""{{chisteText}}""

📅 **Fecha de publicación:** {{publishDate}}
🌍 **Origen:** {{origen}}
🆔 **ID del chiste:** #{{chisteId}}

¡Gracias por compartir tu humor con nosotros! Esperamos que inspire sonrisas en muchas personas.

¡Que tengas un día lleno de risas!

---
El equipo de RetoSquadmakers 😄",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new NotificationTemplate
            {
                TemplateId = "chiste_created_admin",
                Name = "Nuevo Chiste - Administradores",
                Type = "Email",
                Subject = "Nuevo chiste publicado en la plataforma",
                Content = @"Hola {{adminName}},

Se ha publicado un nuevo chiste en la plataforma RetoSquadmakers.

👤 **Autor:** {{authorName}}
📝 **Chiste:** ""{{chisteText}}""
📅 **Fecha:** {{publishDate}}
🌍 **Origen:** {{origen}}
🆔 **ID:** #{{chisteId}}

Puedes revisar la actividad y gestionar el contenido desde el panel de administración.

---
Sistema de Notificaciones RetoSquadmakers",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new NotificationTemplate
            {
                TemplateId = "welcome_user",
                Name = "Bienvenida Usuario",
                Type = "Email",
                Subject = "¡Bienvenido a RetoSquadmakers!",
                Content = @"¡Hola {{userName}}!

¡Te damos la bienvenida a RetoSquadmakers! 🎉

Ahora puedes:
✅ Crear y compartir tus chistes favoritos
✅ Explorar el humor de otros usuarios
✅ Disfrutar de una gran variedad de chistes

¡Esperamos que disfrutes tu experiencia con nosotros!

---
El equipo de RetoSquadmakers 😄",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            new NotificationTemplate
            {
                TemplateId = "system_maintenance",
                Name = "Mantenimiento del Sistema",
                Type = "Email",
                Subject = "Mantenimiento programado - RetoSquadmakers",
                Content = @"Hola {{userName}},

Te informamos que realizaremos un mantenimiento programado en la plataforma.

🕐 **Inicio:** {{startTime}}
🕐 **Fin estimado:** {{endTime}}
⚠️ **Impacto:** {{impactDescription}}

Durante este tiempo, algunas funcionalidades podrían no estar disponibles.

¡Gracias por tu comprensión!

---
El equipo técnico de RetoSquadmakers",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.NotificationTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}