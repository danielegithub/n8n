using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.data
{

    // Repository pattern per operazioni comuni
    public interface IConversationRepository
    {
        Task<ConversationHistory> AddAsync(ConversationHistory conversation);
        Task<IEnumerable<ConversationHistory>> GetBySessionAsync(string sessionId);
        Task<ConversationHistory?> GetByIdAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<IEnumerable<ConversationHistory>> GetRecentAsync(int count = 10);
    }

    public class ConversationRepository : IConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationHistory> AddAsync(ConversationHistory conversation)
        {
            _context.ConversationHistory.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<IEnumerable<ConversationHistory>> GetBySessionAsync(string sessionId)
        {
            return await _context.ConversationHistory
                .Where(c => c.SessionId == sessionId && c.DeletedAt == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<ConversationHistory?> GetByIdAsync(int id)
        {
            return await _context.ConversationHistory
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var conversation = await GetByIdAsync(id);
            if (conversation != null)
            {
                conversation.DeletedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ConversationHistory>> GetRecentAsync(int count = 10)
        {
            return await _context.ConversationHistory
                .Where(c => c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }

    // Servizio per gestire le operazioni sui corsi
    public interface ICorsoService
    {
        Task<IEnumerable<Corso>> SearchAsync(string searchTerm);
        Task<Corso?> GetByCodeAsync(string codiceCorso);
        Task<IEnumerable<Corso>> GetByAreaAsync(string area);
        Task<IEnumerable<Corso>> GetActiveCourses();
    }

    public class CorsoService : ICorsoService
    {
        private readonly ApplicationDbContext _context;

        public CorsoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Corso>> SearchAsync(string searchTerm)
        {
            return await _context.Corsi
                .Where(c => EF.Functions.ILike(c.DenominazioneAttualeCorso!, $"%{searchTerm}%") ||
                           EF.Functions.ILike(c.DescrizioneEstesa!, $"%{searchTerm}%") ||
                           EF.Functions.ILike(c.ParoleChiave!, $"%{searchTerm}%"))
                .ToListAsync();
        }

        public async Task<Corso?> GetByCodeAsync(string codiceCorso)
        {
            return await _context.Corsi
                .FirstOrDefaultAsync(c => c.CodiceCorso == codiceCorso);
        }

        public async Task<IEnumerable<Corso>> GetByAreaAsync(string area)
        {
            return await _context.Corsi
                .Where(c => c.AreaFormazione == area)
                .ToListAsync();
        }

        public async Task<IEnumerable<Corso>> GetActiveCourses()
        {
            return await _context.Corsi
                .Where(c => c.SospesoSoppresso != true)
                .ToListAsync();
        }
    }
}