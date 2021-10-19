using Microsoft.EntityFrameworkCore;

namespace BastardBot.Common.DB
{
    public class BastardDBContext : DbContext
    {
        public BastardDBContext()
        { 
        }
        public BastardDBContext(DbContextOptions<BastardDBContext> options) : base(options)
        {
        }

        public BastardDBContext(string connectionString) : base(new DbContextOptionsBuilder<BastardDBContext>().UseSqlServer(connectionString).Options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public virtual DbSet<Insult> NewInsults { get; set; }
        public virtual DbSet<InsultResponse> NewResponses { get; set; }

        public virtual DbSet<DialogPhrase> ChatPhrases { get; set; }

        public virtual DbSet<PhraseCategory> ChatPhraseCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<InsultResponseAssociation>().HasIndex(ia => new { ia.InsultId, ia.ResponseID }).IsUnique();


            modelBuilder.Entity<Insult>().HasIndex(i => i.Text).IsUnique();
            modelBuilder.Entity<InsultResponse>().HasIndex(i => i.Text).IsUnique();


            modelBuilder.Entity<DialogPhrase>().HasIndex(i => i.Text).IsUnique();
            modelBuilder.Entity<PhraseCategory>().HasIndex(i => i.Text).IsUnique();
        }
    }
}
