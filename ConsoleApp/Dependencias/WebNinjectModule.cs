
using Ninject.Modules;
using Servicios;
using Servicios.Imp;
using Servicios.Interfaces;

namespace Molinos.Scato.Dependencias
{
    public class WebNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IOperador, Operador>().To<Operador>().InSingletonScope();

        }
    }
}
