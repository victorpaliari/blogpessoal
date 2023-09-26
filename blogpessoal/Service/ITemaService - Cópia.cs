using blogpessoal.Model;

namespace blogpessoal.Service
{
    public interface ITemaService
    {
        Task<IEnumerable<Tema>> GetAll();
        //IEnuberable carrega todas as colections que possuem índices
        Task<Tema?> GetById(long id);

        Task<IEnumerable<Tema>> GetByDescricao(string descricao);

        Task<Tema?> Create(Tema Tema);
        Task<Tema?> Update(Tema Tema);
        Task Delete(Tema Tema);




    }
}
