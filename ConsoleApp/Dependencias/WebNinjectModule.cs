
using Ninject.Modules;
using Repositorio;
using Servicios.Imp;
using Servicios.Interfaces;
using System.Data.Entity;
using System.ServiceModel;

namespace Molinos.Scato.Dependencias
{
    public class WebNinjectModule : NinjectModule
    {
        public override void Load()
        {

            Bind<IEstadoOperador, EstadoOperador>().To<EstadoOperador>().InSingletonScope();
            Bind<IOperador, Operador>().To<Operador>().InScope(ctx => OperationContext.Current);
            Bind<DbContext>().To<ConsoleDbContext>().InScope(ctx => OperationContext.Current);
            Bind<IRepositorio, RepositorioEF>().To<RepositorioEF>().InScope(ctx => OperationContext.Current);
            
        }
    }
}
