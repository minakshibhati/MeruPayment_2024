using MeruCommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentBO;
using MeruPaymentDAL.EntityModel;

namespace MeruPaymentDAL
{
    public class ExternalRequestDAL
    {
        private LogHelper _logHelper;
        public ExternalRequestDAL()
        {
            _logHelper = new LogHelper("ExternalRequestDAL()");
        }

        public Tuple<string, string, TripDetailBO> GetLatestTripDetailByMobile(string Mobile)
        {
            _logHelper.MethodName = "GetLatestTripDetailByMobile(string Mobile)";
            TripDetailBO tripDetail = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var tripReceiptData = (from r in db.tblTripReceiptCalculators
                                           where r.CustomerMobileNo == Mobile
                                           select r).OrderByDescending(o => o.TripEndTime).FirstOrDefault();

                    if (tripReceiptData == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find trip detail for mobile {0}", Mobile));
                        return new Tuple<string, string, TripDetailBO>(
                            "500",
                            string.Format("Unable to find trip detail for mobile {0}", Mobile),
                            null);
                    }

                    tripDetail = new TripDetailBO
                    {
                        TripId = tripReceiptData.TripID,
                        TripStartDate = Convert.ToDateTime(tripReceiptData.TripStartTime),
                        TripEndDate = Convert.ToDateTime(tripReceiptData.TripEndTime)
                    };

                    return new Tuple<string, string, TripDetailBO>(
                   "200",
                   "Success",
                   tripDetail);
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting trip receipt detail from DB");
                return new Tuple<string, string, TripDetailBO>(
                    "500",
                    ex.Message,
                    tripDetail);
            }
        }

        public bool IsCustomerOnTrip(string Mobile)
        {
            _logHelper.MethodName = "IsCustomerOnTrip(string Mobile)";
            bool returnData = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var onKerbTripDetail = (from r in db.tblTripMasters
                                        join k in db.tblKerbTripsCustomersDatas on r.TripID equals k.TripID
                                        where k.MobileNo == Mobile
                                        select new { r, k }).FirstOrDefault();

                    returnData = (onKerbTripDetail != null);
                    if (!returnData)
                    {
                        var onTripDetail = (from r in db.tblTripMasters
                                        join j in db.tblJobBookings on r.JobID equals j.JobID
                                        where j.CustomerMobileNo == Mobile
                                        select new { r, j }).FirstOrDefault();
                        returnData = (onTripDetail != null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while checking on trip detail from DB");
            }
            return returnData;
        }
    }
}
