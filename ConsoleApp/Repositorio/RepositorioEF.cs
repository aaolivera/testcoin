using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Repositorio
{
    public class RepositorioEF : IRepositorio
    {
        private const int SqlFkError = 547;
        
        private readonly DbContext context;

        public RepositorioEF(DbContext context)
        {
            this.context = context;
        }

        private IDbSet<TEntidad> Set<TEntidad>() where TEntidad : class
        {
            return context.Set<TEntidad>();
        }

        public TEntidad Obtener<TEntidad>(object id) where TEntidad : class
        {
            return Set<TEntidad>().Find(id);
        }

        public TEntidad ObtenerUnchanged<TEntidad>(object id) where TEntidad : class
        {
            var entity = Set<TEntidad>().Find(id);
            context.Entry(entity).State = EntityState.Unchanged;
            return entity;
        }
        
        public TEntidad Obtener<TEntidad>(Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            return Set<TEntidad>().SingleOrDefault(filtro);
        }

        public TEntidad Obtener<TEntidad>(IEnumerable<Expression<Func<TEntidad, object>>> includes, Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            foreach (var i in includes)
            {
                resultado = resultado.Include(i);
            }
            return resultado.SingleOrDefault(filtro);
        }

        public TEntidad ObtenerPrimero<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class
        {
            return Set<TEntidad>().FirstOrDefault(condicion);
        }

        public TEntidad ObtenerMasReciente<TEntidad>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, DateTime>> columnaFecha) where TEntidad : class
        {
            return Set<TEntidad>().Where(filtro).OrderByDescending(columnaFecha).FirstOrDefault();
        }

        public TProyeccion ObtenerProyeccion<TEntidad, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class
        {
            return Set<TEntidad>().Where(filtro).Select(proyeccion).FirstOrDefault();
        }

        public TProyeccion ObtenerMayor<TEntidad, TOrden, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class
            where TOrden : IComparable
        {
            return Set<TEntidad>().Where(filtro).OrderByDescending(columnaOrden).Select(proyeccion).FirstOrDefault();
        }

        public TEntidad ObtenerMenor<TEntidad, TOrden>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden)
            where TEntidad : class
            where TOrden : IComparable
        {
            return Set<TEntidad>().Where(filtro).OrderBy(columnaOrden).FirstOrDefault();
        }

        public TEntidad ObtenerMayor<TEntidad, TOrden>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden)
            where TEntidad : class
            where TOrden : IComparable
        {
            return Set<TEntidad>().Where(filtro).OrderByDescending(columnaOrden).FirstOrDefault();
        }

        public TProyeccion ObtenerMenor<TEntidad, TOrden, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class
            where TOrden : IComparable
        {
            return Set<TEntidad>().Where(filtro).OrderBy(columnaOrden).Select(proyeccion).FirstOrDefault();
        }

        public IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, bool>> filtro = null) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            if (filtro != null)
            {
                resultado = resultado.Where(filtro);
            }
            return resultado.ToList();
        }

        public IList<TEntidad> Listar<TEntidad>(IEnumerable<Expression<Func<TEntidad, object>>> includes, Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            foreach (var i in includes)
            {
                resultado = resultado.Include(i);
            }
            
            if (filtro != null)
            {
                resultado = resultado.Where(filtro);
            }
            return resultado.ToList();
        }

        public IList<TProyeccion> Listar<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, bool>> filtro = null) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            if (filtro != null)
            {
                resultado = resultado.Where(filtro);
            }
            return resultado.Select(proyeccion).ToList();  
        }

        public decimal Sumar<TEntidad>(Expression<Func<TEntidad, decimal>> proyeccion, Expression<Func<TEntidad, bool>> filtro = null) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            if (filtro != null)
            {
                resultado = resultado.Where(filtro);
            }
            return resultado.Select(proyeccion).DefaultIfEmpty(0).Sum();
        }

        public IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, bool>> condicion, int maxResultados) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            if (condicion != null)
            {
                resultado = resultado.Where(condicion);
            }
            return resultado.Take(maxResultados).ToList();
        }

        public List<TProyeccion> Listar<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, Boolean>> condicion, int maxResultados) where TEntidad : class
        {
            IQueryable<TEntidad> entidades = Set<TEntidad>();
            if (condicion != null)
            {
                entidades = entidades.Where(condicion);
            }
            entidades =  entidades.Take(maxResultados);
            return entidades.Select(proyeccion).ToList();
        }

        public List<TProyeccion> ListarDistintos<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, Boolean>> condicion, int maxResultados) where TEntidad : class
        {
            IQueryable<TEntidad> entidades = Set<TEntidad>();
            if (condicion != null)
            {
                entidades = entidades.Where(condicion).GroupBy(proyeccion).Select(g => g.FirstOrDefault());
            }
            entidades = entidades.Take(maxResultados);
            return entidades.Select(proyeccion).ToList();
        }

        public int Contar<TEntidad>() where TEntidad : class
        {
            return Set<TEntidad>().Count();
        }

        public int Contar<TEntidad>(Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            return Set<TEntidad>().Count(filtro);
        }

        public bool Existe<TEntidad>(Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            // Esto genera una query más optima que usar un Any()
            return Set<TEntidad>().Where(filtro).Select(x => 1).FirstOrDefault() != 0;
        }

        public TEntidad Agregar<TEntidad>(TEntidad entidad) where TEntidad : class
        {
            return Set<TEntidad>().Add(entidad);
        }

        public TEntidad Remover<TEntidad>(object id) where TEntidad : class
        {
            return Remover(Obtener<TEntidad>(id));
        }

        public TEntidad Remover<TEntidad>(TEntidad entidad) where TEntidad : class
        {
            return Set<TEntidad>().Remove(entidad);
        }

        public void RemoverTodos<TEntidad>(IEnumerable<TEntidad> entidades) where TEntidad : class
        {
            foreach (var entidad in entidades)
            {
                Set<TEntidad>().Remove(entidad);
            }
        }

        public int GuardarCambios()
        {
            try {
                return context.SaveChanges();
            }
            catch (DataException e)
            {
                if (ObtenerCodigoError(e) == SqlFkError)
                {
                    throw new EntidadReferenciadaException(string.Empty, e);
                }
                throw;
            }
        }

        private int ObtenerCodigoError(DataException e)
        {
            var code = 0;
            if(e.InnerException != null)
            {
                var sqlEx = e.InnerException.InnerException as SqlException;
                if (sqlEx != null)
                {
                    code = sqlEx.Number;
                }
            }
            return code;
        }
    }
}
