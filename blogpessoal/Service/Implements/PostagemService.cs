using blogpessoal.Data;
using blogpessoal.Model;
using Microsoft.EntityFrameworkCore;

namespace blogpessoal.Service.Implements
{
    public class PostagemService : IPostagemService
    {
        private readonly AppDbContext _context;
    public PostagemService(AppDbContext context)
            {
                _context = context;
            }

        public async Task<IEnumerable<Postagem>> GetAll()
        {
            return await _context.Postagens
                .Include(p => p.Tema)
                .Include(p => p.Usuario)
                .ToListAsync();
        }

        //Método equivalente a = SELECT * FROM tb_postagens where id = id;
        public async Task<Postagem?> GetById(long id)
        {
            try
            {
                //equivalente a instrução:
                //SELECT * FROM tb_postagens where id = id_procurado;
                var Postagem = await _context.Postagens
                    .Include(p => p.Tema)
                    .Include(p => p.Usuario)
                    .FirstAsync(i => i.Id == id);
                return Postagem;
            }
            catch
            {
                return null;
            }
            
            
            }

        public async Task<IEnumerable<Postagem>> GetByTitulo(string titulo)
        {
            var Postagem = await _context.Postagens
                .Include(p => p.Tema)
                .Include(p => p.Usuario)
                .Where(propa => propa.Titulo.Contains(titulo))
                .ToListAsync();
            return Postagem;
        }

        public async Task<Postagem?> Create(Postagem postagem)
        {
            
            if (postagem.Tema is not null)
            {
                var BuscaTema = await _context.Temas.FindAsync(postagem.Tema.Id);

                if (BuscaTema is null)
                    return null;

                postagem.Tema = BuscaTema;
            }
            
            //procurar tema cujo id == id tema recebido atraves da requisição
            postagem.Usuario = postagem.Usuario is not null ? _context.Users.FirstOrDefault(u => u.Id == postagem.Usuario.Id) : null;
            await _context.AddAsync(postagem);
            await _context.SaveChangesAsync();

            return postagem;
        }
        public async Task<Postagem?> Update(Postagem postagem)
        {
            var PostagemUpdate = await _context.Postagens.FindAsync(postagem.Id);

            if (PostagemUpdate is null) 
                return null;

            if (postagem.Tema is not null)
            {
                var BuscaTema = await _context.Temas.FindAsync(postagem.Tema.Id);

                if (BuscaTema is null)
                    return null;
            }

            postagem.Usuario = postagem.Usuario is not null ? _context.Users.FirstOrDefault(u => u.Id == postagem.Usuario.Id) : null;
            _context.Entry(PostagemUpdate).State = EntityState.Detached;
                _context.Entry(postagem).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return postagem;           

        }

        public async Task Delete(Postagem postagem)
        {
            _context.Remove(postagem);

            await _context.SaveChangesAsync();
        }

    }
}
