function JugadaNueva(viewModel) {
    var self = this;
    self.Movimientos = ko.observableArray([]);
    self.PrimerMovimiento = null;
    self.UltimoMovimiento = null;
    self.RelacionSeleccionada = ko.observable();
    

    self.RelacionesFiltradas = ko.observableArray(viewModel.Relaciones());
    self.Filtro = ko.computed(function () {
        var filtro = self.RelacionSeleccionada();
        var i = 0;
        var resultado = ko.utils.arrayFilter(viewModel.Relaciones(), function (prod) {
            if (i === 20) return false;
            var r = MatchPF(prod.Nombre, filtro) && (self.UltimoMovimiento === null || (self.UltimoMovimiento !== null && prod.Nombre != self.UltimoMovimiento.Relacion.Nombre && (MatchPF(prod.Nombre, self.UltimoMovimiento.Relacion.Principal) || MatchPF(prod.Nombre, self.UltimoMovimiento.Relacion.Secundaria))));
            if (r != null && r != false) i++;
            return r;
        });
        if (resultado.length === 1) {
            self.UltimoMovimiento = new Movimiento(resultado[0]);
            if (self.PrimerMovimiento === null) {
                self.PrimerMovimiento = self.UltimoMovimiento;
            }
            self.Movimientos.push(self.UltimoMovimiento);
            self.RelacionSeleccionada(null);
        } else {
            self.RelacionesFiltradas(resultado);
        }
    }).extend({throttle: 250});
}

function Movimiento(relacion) {
    var self = this;
    self.Relacion = relacion;
}

function Relacion(datos) {
    var self = this;
    self.Nombre = datos.Nombre;
    self.Principal = datos.Nombre.split('_')[0];
    self.Secundaria = datos.Nombre.split('_')[1];
    self.FechaDeActualizacion = datos.FechaDeActualizacion;   
    self.MayorPrecioDeVentaAjecutada = datos.MayorPrecioDeVentaAjecutada;
    self.Volumen = datos.Volumen;
    self.Compra = datos.Compra;
    self.Venta = datos.Venta;
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