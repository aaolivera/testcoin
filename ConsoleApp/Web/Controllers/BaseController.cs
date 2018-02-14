using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        protected JsonResult SuccessResponse(object data = null, string successMessage = null, int? maxLength = null)
        {
            var retorno = this.Json(
                new { Success = true, Messages = successMessage != null ? new List<string> { successMessage } : new List<string>(), Data = data ?? new { } },
                JsonRequestBehavior.AllowGet);
            if (maxLength.HasValue)
            {
                retorno.MaxJsonLength = maxLength;
            }

            return retorno;
        }
        
        protected JsonResult ErrorResponse(string errorMessage = null, object data = null)
        {
            return this.Json(new { Success = false, Messages = new List<string> { errorMessage ?? "error" }, Data = data ?? new { } },
                JsonRequestBehavior.AllowGet);
        }

        protected JsonResult ErrorResponse(List<string> errorMessages, object data = null)
        {
            return this.Json(new { Success = false, Messages = errorMessages, Data = data ?? new { } },
                JsonRequestBehavior.AllowGet);
        }

        protected JsonResult DevolverErroresModelState()
        {
            var errores = new List<string>();
            foreach (ModelState modelState in this.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errores.Add(error.ErrorMessage);
                }
            }

            return this.ErrorResponse(errores);
        }
    }
}