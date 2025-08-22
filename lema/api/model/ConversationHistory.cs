using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.model
{


    // Modello per conversation_history
    [Table("conversation_history")]
    public class ConversationHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_message")]
        public string UserMessage { get; set; } = string.Empty;

        [Required]
        [Column("ai_response")]
        public string AiResponse { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("is_valid")]
        public bool IsValid { get; set; } = true;

        [Column("session_id")]
        [MaxLength(255)]
        public string SessionId { get; set; }

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }
    }
}