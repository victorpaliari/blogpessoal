using blogpessoal.Data;
using blogpessoal.Model;
using Microsoft.EntityFrameworkCore;

namespace blogpessoal.Service.Implements
{
    public class TemaService : ITemaService
    {
        private readonly AppDbContext _context;
    public TemaService(AppDbContext context)
            {
                _context = context;
            }

        public async Task<IEnumerable<Tema>> GetAll()
        {
            return await _context.Temas.ToListAsync();
        }

        //Método equivalente a = SELECT * FROM tb_Temas where id = id;
        public async Task<Tema?> GetById(long id)
        {
            try
            {
                //equivalente a instrução:
                //SELECT * FROM tb_Temas where id = id_procurado;
                var Tema = await _context.Temas.FirstAsync(i => i.Id == id);
                return Tema;
            }
            catch
            {
                return null;
            }
            
            
            }

        public async Task<IEnumerable<Tema>> GetByDescricao(string descricao)
        {
            var Tema = await _context.Temas
                .Where(propa => propa.Descricao.Contains(descricao))
                .ToListAsync();
            return Tema;
        }

        public async Task<Tema?> Create(Tema Tema)
        {
            await _context.AddAsync(Tema);
            await _context.SaveChangesAsync();

            return Tema;
        }
        public async Task<Tema?> Update(Tema Tema)
        {
            var TemaUpdate = await _context.Temas.FindAsync(Tema.Id);

            if (TemaUpdate is null) 
                return null;

            _context.Entry(TemaUpdate).State = EntityState.Detached;
                _context.Entry(Tema).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Tema;
            

        }

        public async Task Delete(Tema Tema)
        {
            _context.Remove(Tema);

            await _context.SaveChangesAsync();
        }

    }
}
