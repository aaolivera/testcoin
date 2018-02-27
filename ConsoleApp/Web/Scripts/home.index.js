function JugadaNueva(viewModel) {
    var self = this;
    var inicial = new Movimiento(self, viewModel);
    self.Movimientos = ko.observableArray([inicial]);


    self.AgregarMovimiento = function(movimiento) {
        self.Movimientos.push(self.UltimoMovimiento);
    }

}

function Movimiento(jugada, viewModel) {
    var self = this;
    self.MovimientoSiguiente = ko.observable(null);
    self.Confirmado = ko.observable(false);
    self.Comprar = ko.observable(true);    
    self.Relacion = ko.observable(null);

    self.AmountCompra = ko.observable(0);
    self.AmountVenta = ko.observable(0);

    self.TotalCompra = ko.observable(0);
    self.TotalVenta = ko.observable(0);

    self.RelacionesFiltradas = ko.observableArray(viewModel.Relaciones());
    self.RelacionEscrita = ko.observable();
    self.RelacionSeleccionada = ko.observable();
    self.ConfirmarRelacion = function () {
        setTimeout(function () {
            self.Relacion(self.RelacionSeleccionada());
        }, 251);
        
    }
    self.Filtro = ko.computed(function () {
        var filtro = self.RelacionEscrita();
        var i = 0;
        var resultado = ko.utils.arrayFilter(viewModel.Relaciones(), function (prod) {
            if (i === 20) return false;
            var r = MatchPF(prod.Nombre, filtro);
                //&& (self.UltimoMovimiento === null || (self.UltimoMovimiento !== null && prod.Nombre != self.UltimoMovimiento.Relacion.Nombre && (MatchPF(prod.Nombre, self.UltimoMovimiento.Relacion.Principal) || MatchPF(prod.Nombre, self.UltimoMovimiento.Relacion.Secundaria))));
            if (r != null && r != false) i++;
            return r;
        });
        if (resultado.length === 1) {
            self.RelacionSeleccionada(resultado[0]);
        } else {
            self.RelacionesFiltradas(resultado);
        }
    }).extend({ throttle: 250 });
    
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
        self.AmountCompra((self.TotalCompra() * self.Relacion().Compra).toFixed(8));
        self.AmountVenta((self.TotalVenta() * self.Relacion().Venta).toFixed(8));
    }
    self.TotalVentaChanged = function (obj, event) {
        self.TotalCompra(self.TotalVenta());
        self.AmountCompra((self.TotalCompra() * self.Relacion().Compra).toFixed(8));
        self.AmountVenta((self.TotalVenta() * self.Relacion().Venta).toFixed(8));
    }
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
    }, 500);
}

$(document).ready(function () {
    window.model = new viewModel();
    window.model.CargarRelaciones();
    ko.applyBindings(window.model);
});