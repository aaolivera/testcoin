using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositorio
{
    public interface IRepositorio
    {
        /// <summary>
        /// Devuelve una entidad dada su clave primaria
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="id">clave primaria de la entidad</param>
        /// <returns>La entidad que posee esa clave o null si no se encuentra ninguna</returns>
        TEntidad Obtener<TEntidad>(object id) where TEntidad : class;

        /// <summary>
        /// Devuelve una entidad dada su clave primaria con estado Unchanged
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="id">clave primaria de la entidad</param>
        /// <returns>La entidad que posee esa clave con estado Unchanged o null si no se encuentra ninguna</returns>
        TEntidad ObtenerUnchanged<TEntidad>(object id) where TEntidad : class;

        /// <summary>
        /// Devuelve una unica entidad que cumple con la condición
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe devolverse o no</param>
        /// <returns>La unica entidad que se corresponde con la condicion o null si no la encuentra</returns>
        /// <exception cref="InvalidOperationException">Se lanza cuando más de una entidad comple con la condición</exception>
        TEntidad Obtener<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;

        TEntidad Obtener<TEntidad>(IEnumerable<Expression<Func<TEntidad, object>>> includes, Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;

        TEntidad ObtenerPrimero<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;

        /// <summary>
        /// Devuelve la entidad más reciente que cumple la condición
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="filtro">Expresion que dada una entidad devuelve si debe devolverse o no</param>
        /// <param name="columnaFecha">Indica cual es la columna de tipo Datetime que se debe usar para el ordenamiento</param>
        /// <returns>La entidad más reciente que se corresponde con la condicion o null si no encuentra ninguna</returns>
        TEntidad ObtenerMasReciente<TEntidad>(Expression<Func<TEntidad, bool>> filtro,
                                              Expression<Func<TEntidad, DateTime>> columnaFecha) where TEntidad : class;

        TProyeccion ObtenerProyeccion<TEntidad, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class;

        TProyeccion ObtenerMayor<TEntidad, TOrden, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class where TOrden : IComparable;

        TEntidad ObtenerMenor<TEntidad, TOrden>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden)
            where TEntidad : class
            where TOrden : IComparable;

        TEntidad ObtenerMayor<TEntidad, TOrden>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden)
            where TEntidad : class
            where TOrden : IComparable;

        TProyeccion ObtenerMenor<TEntidad, TOrden, TProyeccion>(Expression<Func<TEntidad, bool>> filtro, Expression<Func<TEntidad, TOrden>> columnaOrden, Expression<Func<TEntidad, TProyeccion>> proyeccion)
            where TEntidad : class
            where TOrden : IComparable;

        /// <summary>
        /// Lista todas las entidades que complen con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe devolverse o no</param>
        /// <returns>Todas las entidades que cumplen con la condicion</returns>
        IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion = null) where TEntidad : class;
        IList<TEntidad> Listar<TEntidad>(IEnumerable<Expression<Func<TEntidad, object>>> includes, Expression<Func<TEntidad, Boolean>> condicion = null) where TEntidad : class;
        IList<TProyeccion> Listar<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, Boolean>> condicion = null) where TEntidad : class;
        List<TProyeccion> ListarDistintos<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, Boolean>> condicion, int maxResultados) where TEntidad : class;
        IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion, int maxResultados) where TEntidad : class;
        List<TProyeccion> Listar<TEntidad, TProyeccion>(Expression<Func<TEntidad, TProyeccion>> proyeccion, Expression<Func<TEntidad, Boolean>> condicion, int maxResultados) where TEntidad : class;


        decimal Sumar<TEntidad>(Expression<Func<TEntidad, decimal>> proyeccion, Expression<Func<TEntidad, bool>> filtro = null) where TEntidad : class;

        /// <summary>
        /// Lista todas las entidades que cumplen con la condicion devolviendo solo una pagina de resultados
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="consulta">Consulta paginada que contiene la condicion, el orden y la pagina a devolver</param>
        /// <returns>Todas las entidades que cumplen con la condicion paginando el resultado</returns>
        // ListaPaginada<TEntidad> Listar<TEntidad>(ConsultaPaginada<TEntidad> consulta) where TEntidad : class;

        /// <summary>
        /// Devuelve la cantidad de entidades en el repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a contar</typeparam>
        /// <returns>La cantidad de entidades de un tipo determinado en el repositorio</returns>
        int Contar<TEntidad>() where TEntidad : class;

        /// <summary>
        /// Devuelve la cantidad de entidades en el repositorio que cumplen con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a contar</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe contarse o no</param>
        /// <returns>La cantidad de entidades de un tipo determinado en el repositorio</returns>
        int Contar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion) where TEntidad : class;

        /// <summary>
        /// Devuelve si existe una entidad que cumpla con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a comprobar</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe contarse o no</param>
        /// <returns>true si existe alguna entidad que compla con la condición</returns>
        bool Existe<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion) where TEntidad : class;

        /// <summary>
        /// Agrega una entidad al repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a agregar</typeparam>
        /// <param name="entidad">Entidad a agregar</param>
        /// <returns>Entidad agregada</returns>
        TEntidad Agregar<TEntidad>(TEntidad entidad) where TEntidad : class;

        /// <summary>
        /// Remueve una entidad del repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a remover</typeparam>
        /// <param name="entidad">Entidad a remover</param>
        /// <returns>Entidad removida</returns>
        TEntidad Remover<TEntidad>(TEntidad entidad) where TEntidad : class;

        /// <summary>
        /// Remueve una entidad del repositorio dada su clave primaria
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a remover</typeparam>
        /// <param name="id">clave de la entidad a remover</param>
        /// <returns>Entidad removida</returns>
        TEntidad Remover<TEntidad>(object id) where TEntidad : class;

        void RemoverTodos<TEntidad>(IEnumerable<TEntidad> entidades) where TEntidad : class;

        /// <summary>
        /// Persiste los cambios hechos al repositorio
        /// </summary>
        /// <returns>Cantidad de entidades agregadas/actualizadas</returns>
        int GuardarCambios();
    }
}
