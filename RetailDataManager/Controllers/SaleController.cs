using Microsoft.AspNet.Identity;
using RMDataManager.Library.DataAccess;
using RMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RetailDataManager.Controllers
{
    [Authorize]
    public class SaleController : ApiController
    {
        [Authorize(Roles = "Cashier")]
        public void Post(SaleModel sale)
        {
            SaleData saleData = new SaleData();
            // user id is taken from authorization
            string userId = RequestContext.Principal.Identity.GetUserId();

            saleData.SaveSale(sale, userId);
        }

        /// <summary>
        /// Gets the sale reports from the database when api request api/sale/GetSalesReport comes in.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager")]
        [Route("GetSalesReport")]
        public List<SaleReportModel> GetSalesReport()
        {
            SaleData data = new SaleData();
            return data.GetSaleReport();
        }
    }
}
