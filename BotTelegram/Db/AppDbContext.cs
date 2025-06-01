using Microsoft.EntityFrameworkCore;
using AdTorrBot.BotTelegram.Db.Model;
using AdTorrBot.BotTelegram.Db.Model.TorrserverModel;
namespace AdTorrBot.BotTelegram.Db
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<SettingsBot> SettingsBot {  get; set; }
        public virtual DbSet<SettingsTorrserverBot> SettingsTorrserverBot { get; set; }
        public virtual DbSet<User>User { get; set; }
        public virtual DbSet<TextInputFlag> TextInputFlag { get; set; }
        public virtual DbSet<BitTorrConfig> BitTorrConfig { get; set; }
        public virtual DbSet<ServerArgsConfig> ServerArgsConfig { get; set; }
        public virtual DbSet<Profiles> Profiles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");   
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BitTorrConfig>()
                .HasIndex(b => new { b.IdChat, b.NameProfileBot })
                .IsUnique();
            modelBuilder.Entity<Profiles>()
                .HasIndex(p=>p.Login)
                .IsUnique();
        }

    }
}
