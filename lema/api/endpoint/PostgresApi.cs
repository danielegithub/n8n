using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.endpoint
{
    public static class PostgresApi
    {
        public static IEndpointRouteBuilder MapPostgresEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/postgres").WithTags("PostgreSQL API");

            group.MapGet("/conversations", async (ApplicationDbContext db, int page = 1, int size = 20, string? sessionId = null) =>
            {
                var query = db.ConversationHistory.Where(c => c.DeletedAt == null);

                if (!string.IsNullOrEmpty(sessionId))
                    query = query.Where(c => c.SessionId == sessionId);

                var conversations = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Results.Ok(conversations);
            });

            group.MapGet("/conversations/{id}", async (int id, ApplicationDbContext db) =>
            {
                var conversation = await db.ConversationHistory
                    .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);

                return conversation is not null ? Results.Ok(conversation) : Results.NotFound();
            });

            group.MapPost("/conversations", async (ConversationHistory conversation, ApplicationDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(conversation.UserMessage) || string.IsNullOrWhiteSpace(conversation.AiResponse))
                    return Results.BadRequest("UserMessage e AiResponse sono obbligatori");

                conversation.Id = 0; // Reset ID per insert
                conversation.CreatedAt = DateTimeOffset.UtcNow;

                if (string.IsNullOrEmpty(conversation.SessionId))
                    conversation.SessionId = Guid.NewGuid().ToString();

                db.ConversationHistory.Add(conversation);
                await db.SaveChangesAsync();

                return Results.Created($"/api/postgres/conversations/{conversation.Id}", conversation);
            });

            group.MapGet("/corsi", async (ApplicationDbContext db, int page = 1, int size = 20,
                string? area = null, string? search = null) =>
            {
                var query = db.Corsi.AsQueryable();

                if (!string.IsNullOrEmpty(area))
                    query = query.Where(c => c.AreaFormazione == area);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(c =>
                        EF.Functions.ILike(c.DenominazioneAttualeCorso!, $"%{search}%") ||
                        EF.Functions.ILike(c.CodiceCorso!, $"%{search}%"));

                var corsi = await query
                    .OrderBy(c => c.DenominazioneAttualeCorso)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Results.Ok(corsi);
            });

            group.MapGet("/corsi/{id}", async (int id, ApplicationDbContext db) =>
            {
                var corso = await db.Corsi.FindAsync(id);
                return corso is not null ? Results.Ok(corso) : Results.NotFound();
            });

            group.MapPost("/corsi", async (Corso corso, ApplicationDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(corso.CodiceCorso))
                    return Results.BadRequest("CodiceCorso è obbligatorio");

                var exists = await db.Corsi.AnyAsync(c => c.CodiceCorso == corso.CodiceCorso);
                if (exists)
                    return Results.Conflict("Codice corso già esistente");

                corso.Id = 0; // Reset ID per insert
                db.Corsi.Add(corso);
                await db.SaveChangesAsync();

                return Results.Created($"/api/postgres/corsi/{corso.Id}", corso);
            });

            // ============= DOCUMENTS =============

            group.MapGet("/documents", async (ApplicationDbContext db, int page = 1, int size = 20,
                string? category = null, string? search = null) =>
            {
                var query = db.Documents.AsQueryable();

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(d => d.Category == category);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(d =>
                        EF.Functions.ILike(d.Title!, $"%{search}%") ||
                        EF.Functions.ILike(d.Content!, $"%{search}%"));

                var documents = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Results.Ok(documents);
            });

            group.MapGet("/documents/{id}", async (int id, ApplicationDbContext db) =>
            {
                var document = await db.Documents.FindAsync(id);
                return document is not null ? Results.Ok(document) : Results.NotFound();
            });

            group.MapPost("/documents", async (Document document, ApplicationDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(document.Title))
                    return Results.BadRequest("Title è obbligatorio");

                document.Id = 0; // Reset ID per insert
                document.CreatedAt = DateTime.UtcNow;
                document.DocLength = document.Content?.Length;

                db.Documents.Add(document);
                await db.SaveChangesAsync();

                return Results.Created($"/api/postgres/documents/{document.Id}", document);
            });

            return app;
        }
    }
}