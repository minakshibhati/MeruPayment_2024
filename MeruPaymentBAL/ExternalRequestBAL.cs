using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class ExternalRequestBAL
    {
        private LogHelper _logHelper;
        public ExternalRequestBAL()
        {
            _logHelper = new LogHelper("ExternalRequestBAL()");
        }

        public Tuple<string, string, TripDetailBO> GetLatestTripDetailByMobile(string Mobile)
        {
            _logHelper.MethodName = "GetLatestTripDetailByMobile(string Mobile)";
            try
            {
                ExternalRequestDAL externalRequestDAL = new ExternalRequestDAL();
                Tuple<string, string, TripDetailBO> returnTripDetail = externalRequestDAL.GetLatestTripDetailByMobile(Mobile);
                return returnTripDetail;
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "An error occured while getting latest trip detail for mobile no:" + Mobile);
                return new Tuple<string, string, TripDetailBO>("500", "Failed", null);
            }
        }

        public bool IsCustomerOnTrip(string Mobile) 
        {
            bool returnValue = false;
            _logHelper.MethodName = "IsCustomerOnTrip(string Mobile)";
            try
            {
                ExternalRequestDAL externalRequestDAL = new ExternalRequestDAL();
                returnValue = externalRequestDAL.IsCustomerOnTrip(Mobile);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "An error occured while checking on trip for mobile no:" + Mobile);
            }
            return returnValue;
        }
    }
}
