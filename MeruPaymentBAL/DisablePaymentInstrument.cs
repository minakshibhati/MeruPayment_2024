using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class DisablePaymentInstrument
    {
        AuthCardDAL authCardDAL = null;
        ExternalRequestBAL externalRequestBAL = null;
        LogHelper logHelper = null;
        int dayDiff = 0;

        public DisablePaymentInstrument()
        {
            logHelper = new LogHelper("DisablePaymentInstrument()");
            authCardDAL = new AuthCardDAL();
            externalRequestBAL = new ExternalRequestBAL();
            dayDiff = Convert.ToInt32(ConfigurationManager.AppSettings["Service_Or_Card_Unused_DayDiff"]);
        }

        public void Process()
        {
            logHelper.MethodName = "Process()";
            try
            {
                CustomerDAL customerDAL = new CustomerDAL();
                Tuple<string, string, List<CustomerBO>> returnCustomerList = customerDAL.GetCustomerDetailByCustomerStatus(CustomerStatus.Active);

                if (returnCustomerList == null || returnCustomerList.Item1 != "200")
                {
                    logHelper.WriteInfo("Inactive customer not found for payment method soft delete");
                }

                if (returnCustomerList.Item3 == null || returnCustomerList.Item3.Count == 0)
                {
                    logHelper.WriteInfo("Inactive customer not found for payment method soft delete"); 
                }

                foreach (var item in returnCustomerList.Item3)
                {
                    ProcessIndividual(item.Contact);
                }
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "An error occured while processing disable payment instruction.");
            }
        }

        public void ProcessIndividual(string Mobile)
        {
            logHelper.MethodName = "ProcessIndividual(string Mobile)";

            DateTime LatestCardAddedDate = DateTime.MinValue;
            DateTime LatestTripDate = DateTime.MinValue;
            DateTime LatestDate = DateTime.MinValue;

            try
            {
                Tuple<string, string, CardBO> returnCardDetail = authCardDAL.GetLatestValidCardByMobile(Mobile);
                if (returnCardDetail.Item1 == "200")
                {
                    LatestCardAddedDate = returnCardDetail.Item3.RecordCreatedDateTime;
                    LatestDate = LatestCardAddedDate;
                }

                Tuple<string, string, TripDetailBO> returnTripEndDetail = externalRequestBAL.GetLatestTripDetailByMobile(Mobile);
                if (returnTripEndDetail.Item1 == "200")
                {
                    LatestTripDate = returnTripEndDetail.Item3.TripEndDate;
                }

                if (externalRequestBAL.IsCustomerOnTrip(Mobile))
                {
                    LatestTripDate = DateTime.Now;
                }

                if (LatestCardAddedDate < LatestTripDate)
                {
                    LatestDate = LatestTripDate;
                }

                TimeSpan dateTimeSpan = DateTime.Now.Subtract(LatestDate);
                if (dateTimeSpan.Days > dayDiff)
                {
                    authCardDAL.DeleteAllCardByMobile(Mobile);
                }
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "An error occured while processing disable payment instruction.");
            }
        }
    }
}
