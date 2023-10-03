﻿using blogpessoal.Model;
using Microsoft.EntityFrameworkCore;

namespace blogpessoal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Postagem>().ToTable("tb_postagens");
            modelBuilder.Entity<Tema>().ToTable("tb_temas");
            modelBuilder.Entity<User>().ToTable("tb_usuarios");

            _ = modelBuilder.Entity<Postagem>()
              
                //tipo da relação
                .HasOne(_ => _.Tema)
                //outro lado da relação
                .WithMany(t => t.Postagem)
                //tipo da chave
                .HasForeignKey("TemaId")
                //Apaga todos os filhos de um tema mãe
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<Postagem>()

                //tipo da relação
                .HasOne(_ => _.Usuario)
                //outro lado da relação
                .WithMany(t => t.Postagem)
                //tipo da chave
                .HasForeignKey("UsuarioId")
                //Apaga todos os filhos de um tema mãe
                .OnDelete(DeleteBehavior.Cascade);


        }

        //Registrar DbSET - Objeto responsável por criar a tabela
        public DbSet<Postagem> Postagens { get; set; } = null!;
        public DbSet<Tema> Temas { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        //metodo para persistir o async
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var insertedEntries = this.ChangeTracker.Entries()
                                   .Where(x => x.State == EntityState.Added)
                                   .Select(x => x.Entity);

            foreach (var insertedEntry in insertedEntries)
            {
                //Se uma propriedade da Classe Auditable estiver sendo criada. 
                if (insertedEntry is Auditable auditableEntity)
                {
                    auditableEntity.Data = new DateTimeOffset(DateTime.Now, new TimeSpan(-3, 0, 0));
                }
            }

            var modifiedEntries = ChangeTracker.Entries()
                       .Where(x => x.State == EntityState.Modified)
                       .Select(x => x.Entity);

            foreach (var modifiedEntry in modifiedEntries)
            {
                //Se uma propriedade da Classe Auditable estiver sendo atualizada.  
                if (modifiedEntry is Auditable auditableEntity)
                {
                    auditableEntity.Data = DateTimeOffset.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
