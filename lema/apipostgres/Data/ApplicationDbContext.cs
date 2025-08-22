

public class ApplicationDbContext : DbContext
{
      public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
      {
      }

      public DbSet<ConversationHistory> ConversationHistory { get; set; }
      public DbSet<Corso> Corsi { get; set; }
      public DbSet<Document> Documents { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);

            // Configurazione ConversationHistory
            modelBuilder.Entity<ConversationHistory>(entity =>
            {
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd()
                        .UseIdentityByDefaultColumn();

                  entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                  entity.Property(e => e.IsValid)
                        .HasDefaultValue(true);

                  entity.HasIndex(e => e.SessionId);
                  entity.HasIndex(e => e.CreatedAt);
            });

            // Configurazione Corso
            modelBuilder.Entity<Corso>(entity =>
            {
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd()
                        .UseIdentityByDefaultColumn();

                  entity.HasIndex(e => e.CodiceCorso);
                  entity.HasIndex(e => e.TipoCorso);
                  entity.HasIndex(e => e.AreaFormazione);
            });

            // Configurazione Document
            modelBuilder.Entity<Document>(entity =>
            {
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Id)
                        .ValueGeneratedOnAdd()
                        .UseIdentityByDefaultColumn();

                  entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                  // Indici per performance
                  entity.HasIndex(e => e.OriginalTitle);
                  entity.HasIndex(e => e.ChunkIndex);
                  entity.HasIndex(e => e.Category);
                  entity.HasIndex(e => e.IsChunked);

                  // Full text search (PostgreSQL)
                  entity.HasIndex(e => new { e.Title, e.Content })
                        .HasMethod("gin")
                        .IsTsVectorExpressionIndex("english");
            });
      }
}