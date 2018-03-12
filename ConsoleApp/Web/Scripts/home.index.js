function JugadaNueva(viewModel) {
    var self = this;
    self.Inicial = new Movimiento(self, null, viewModel);
    self.Inicial.EsInicial(true);
    self.Movimientos = ko.observableArray([self.Inicial]);


    self.AgregarMovimiento = function(movimiento) {
        self.Movimientos.push(movimiento);
    }

}

function Movimiento(jugada, movimientoAnterior, viewModel) {
    var self = this;
    self.MovimientoSiguiente = ko.observable(null);
    self.MovimientoAnterior = ko.observable(movimientoAnterior);
    self.Confirmado = ko.observable(false);
    self.Comprar = ko.observable(true);    
    self.Relacion = ko.observable(null);

    self.EsInicial = ko.observable(false);

    self.AmountCompra = ko.observable(0);
    self.AmountVenta = ko.observable(0);
    self.TotalCompra = ko.observable(0);
    self.TotalVenta = ko.observable(0);

    //Siguiente
    self.Siguiente = function () {
        var nuevoMovimiento = new Movimiento(jugada, self, viewModel);
        jugada.AgregarMovimiento(nuevoMovimiento);

        self.MovimientoSiguiente(nuevoMovimiento);
        self.Confirmado(true);
    }
    //
    //Cargar Pair
    self.RelacionesFiltradas = ko.observableArray([]);
    self.RelacionEscrita = ko.observable();
    self.RelacionSeleccionada = ko.observable();
    self.ConfirmarRelacion = function () {
        setTimeout(function () {
            self.Relacion(self.RelacionSeleccionada());
            if (self.MovimientoAnterior() != null) {
                self.Comprar(!(self.MovimientoAnterior().Relacion().Principal == self.Relacion().Principal || self.MovimientoAnterior().Relacion().Secundaria == self.Relacion().Principal));
                if (self.MovimientoAnterior().Relacion().Principal == self.Relacion().Principal) {
                    self.AmountCompra((self.MovimientoAnterior().AmountCompra()));
                    self.AmountVenta((self.MovimientoAnterior().AmountVenta()));
                    self.TotalCompra((self.AmountCompra() * self.Relacion().Compra).toFixed(8));
                    self.TotalVenta((self.AmountVenta() * self.Relacion().Venta).toFixed(8));
                }
                if (self.MovimientoAnterior().Relacion().Secundaria == self.Relacion().Principal) {
                    self.AmountCompra((self.MovimientoAnterior().TotalCompra()));
                    self.AmountVenta((self.MovimientoAnterior().TotalVenta()));
                    self.TotalCompra((self.AmountCompra() * self.Relacion().Compra).toFixed(8));
                    self.TotalVenta((self.AmountVenta() * self.Relacion().Venta).toFixed(8));
                }
                if (self.MovimientoAnterior().Relacion().Principal == self.Relacion().Secundaria) {
                    self.TotalCompra((self.MovimientoAnterior().AmountCompra()));
                    self.TotalVenta((self.MovimientoAnterior().AmountVenta()));
                    self.AmountCompra((self.TotalCompra() / self.Relacion().Compra).toFixed(8));
                    self.AmountVenta((self.TotalVenta() / self.Relacion().Venta).toFixed(8));
                }
                if (self.MovimientoAnterior().Relacion().Secundaria == self.Relacion().Secundaria) {
                    self.TotalCompra((self.MovimientoAnterior().TotalCompra()));
                    self.TotalVenta((self.MovimientoAnterior().TotalVenta()));
                    self.AmountCompra((self.TotalCompra() / self.Relacion().Compra).toFixed(8));
                    self.AmountVenta((self.TotalVenta() / self.Relacion().Venta).toFixed(8));
                }
            }
        }, 251);
        
    }
    self.Filtro = ko.computed(function () {
        var filtro = self.RelacionEscrita();
        var i = 0;
        var resultado = ko.utils.arrayFilter(viewModel.Relaciones(), function (prod) {
            if (i === 20) return false;
            var anterior = self.MovimientoAnterior();
            var r = MatchPF(prod.Nombre, filtro) && (anterior == null || (anterior != null && anterior.Comprar() && MatchPF(prod.Nombre, anterior.Relacion().Principal)) || (anterior != null && !anterior.Comprar() && MatchPF(prod.Nombre, anterior.Relacion().Secundaria)));
            if (r != null && r != false) i++;
            return r;
        });
        if (resultado.length === 1) {
            self.RelacionSeleccionada(resultado[0]);
        } else {
            self.RelacionesFiltradas(resultado);
        }
    }).extend({ throttle: 250 });
    //
    //Calcular amount y total
    self.AmountCompraChanged = function (obj, event) {
        self.AmountVenta(self.AmountCompra());
        self.TotalCompra((self.AmountCompra() * self.Relacion().Compra).toFixed(8));
        self.TotalVenta((self.AmountVenta() * self.Relacion().Venta).toFixed(8));
    }
    self.AmountVentaChanged = function (obj, event) {
        self.AmountCompra(self.AmountVenta());
        self.TotalCompra((self.AmountCompra() * self.Relacion().Compra).toFixed(8));
        self.TotalVenta((self.AmountVenta() * self.Relacion().Venta).toFixed(8));
    }
    self.TotalCompraChanged = function (obj, event) {
        self.TotalVenta(self.TotalCompra());
        self.AmountCompra((self.TotalCompra() / self.Relacion().Compra).toFixed(8));
        self.AmountVenta((self.TotalVenta() / self.Relacion().Venta).toFixed(8));
    }
    self.TotalVentaChanged = function (obj, event) {
        self.TotalCompra(self.TotalVenta());
        self.AmountCompra((self.TotalCompra() / self.Relacion().Compra).toFixed(8));
        self.AmountVenta((self.TotalVenta() / self.Relacion().Venta).toFixed(8));
    }
    //
}

function Relacion(datos) {
    var self = this;
    self.Nombre = datos.Nombre;
    self.Principal = datos.Nombre.split('_')[0];
    self.Secundaria = datos.Nombre.split('_')[1];
    self.FechaDeActualizacion = datos.FechaDeActualizacion;    
    self.Volumen = datos.Volumen;
    self.MayorPrecioDeVentaAjecutada = Number(datos.MayorPrecioDeVentaAjecutada);
    self.Compra = Number(datos.Compra);
    self.Venta = Number(datos.Venta);
    self.DeltaEjecutado = datos.DeltaEjecutado;
    self.DeltaActual = datos.DeltaActual;
}

//function EstadoServicio(datos) {
//    var self = this;
//    if (datos != null) {
//        self.UltimoUpdate = datos.UltimoUpdate;
//        self.UpdateEnProgreso = datos.UpdateEnProgreso;
//        self.CantidadDeRelaciones = datos.CantidadDeRelaciones;
//        self.RelacionesActualizadas = datos.RelacionesActualizadas;
//        self.Paginas = datos.Paginas;
//    }
//}

function MatchPF(string, filtro) {
    if (filtro == null || filtro == undefined) return false;
    var f = filtro.split('_');
    var t = string.split('_');
    if (f.length == 1) return t[0] == f[0] || t[1] == f[0];
    return (f[0] == "" || t[0] == f[0] )&& (f[1] == "" || t[1] == f[1]);
}
function RefrescarEstado(viewModel) {
    $.getJSON(urlObtenerEstado,
        function (data) {
            viewModel.Estado(data.Data);
        });
}

function viewModel() {
    var self = this;
    self.Cargando = ko.observable(true);
    self.Relaciones = ko.observableArray([]);
    self.Jugadas = ko.observableArray([]);
    self.JugadaNueva = new JugadaNueva(self);
    self.Estado = ko.observable(null);

    self.CargarRelaciones = function () {
        self.Cargando(true);        
        $.getJSON(urlListarRelaciones,
            function (data) {
                if (!evaluateResponse(data)) {
                    return;
                }
                var temp = [];
                data.Data.forEach(function (element, index, array) {
                    temp.push(new Relacion(element));
                });
                self.Relaciones(temp);
                self.Cargando(false);
            });
    };

    self.ActualizarServidor = function () {
        $.getJSON(urlActualizarServidor);        
    }

    RefrescarEstado(self);
    setInterval(function () {
        RefrescarEstado(self);
    }, 2000);
}

$(document).ready(function () {
    window.model = new viewModel();
    window.model.CargarRelaciones();
    ko.applyBindings(window.model);
});