//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeruPaymentDAL.EntityModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblTripMaster
    {
        public int TripID { get; set; }
        public string TripNo { get; set; }
        public int JobID { get; set; }
        public byte TripTypeID { get; set; }
        public Nullable<int> MappingID { get; set; }
        public int TripStartID { get; set; }
        public int TripEndID { get; set; }
        public string TripStartTime { get; set; }
        public string TripEndTime { get; set; }
        public Nullable<double> DistFromLastTripToCurrTrip { get; set; }
        public Nullable<double> OnCallDistcane { get; set; }
        public string ScheduledTripNo { get; set; }
        public string CreditCardID { get; set; }
        public Nullable<int> LoginCount { get; set; }
        public Nullable<double> TripStartKilometer { get; set; }
        public Nullable<double> TripEndKilometer { get; set; }
        public Nullable<decimal> TotalTripFare { get; set; }
        public string ServiceTax { get; set; }
        public string EduCess { get; set; }
        public string SecEduCess { get; set; }
        public Nullable<byte> PaymentMode { get; set; }
        public string MDTKerbJobID { get; set; }
        public Nullable<int> SubscriberID { get; set; }
        public string CabRegistration { get; set; }
        public string PickupLocation { get; set; }
        public string DropLocation { get; set; }
        public Nullable<double> TotalRevenueDistance { get; set; }
        public Nullable<decimal> BookingFee { get; set; }
        public Nullable<bool> FeedbackSMS { get; set; }
        public Nullable<decimal> AirportCharges { get; set; }
        public Nullable<int> TripOncallWaitingTime { get; set; }
        public Nullable<decimal> TripOncallWaitingCharge { get; set; }
        public Nullable<decimal> TripWaitingChargeforCorporate { get; set; }
        public Nullable<int> TotalWaitTime { get; set; }
        public Nullable<decimal> WaitingFare { get; set; }
        public string RRNumber { get; set; }
        public Nullable<byte> RetentionServiceStatus { get; set; }
        public Nullable<decimal> ExpectedAmount { get; set; }
        public Nullable<decimal> ShadowAmount { get; set; }
        public Nullable<bool> EmailSendStatus { get; set; }
        public Nullable<float> CabLatitude_TS { get; set; }
        public Nullable<float> CabLongitude_TS { get; set; }
        public Nullable<int> ZoneID_TS { get; set; }
        public Nullable<decimal> CouponAmount { get; set; }
        public Nullable<decimal> RedeemAmount { get; set; }
        public Nullable<int> AdminEmailSendStatus { get; set; }
        public Nullable<byte> BrandTypeID { get; set; }
        public Nullable<decimal> DeviceTotalFare { get; set; }
        public Nullable<decimal> RunningFare { get; set; }
        public Nullable<System.DateTime> EmailSentDateTime { get; set; }
        public Nullable<int> WalletTSStatus { get; set; }
        public string WalletType { get; set; }
        public Nullable<int> NoOfAttempts { get; set; }
        public string GPSValueSync { get; set; }
        public Nullable<decimal> AdditionalFare { get; set; }
        public Nullable<decimal> TollCharges { get; set; }
        public Nullable<System.DateTime> TSReceivedTime { get; set; }
        public Nullable<System.DateTime> TEReceivedTime { get; set; }
        public Nullable<double> GoogleTotalDistance { get; set; }
        public Nullable<double> GoogleCumulateDistance { get; set; }
        public Nullable<byte> ProductTypeID { get; set; }
        public Nullable<byte> IsRideShare { get; set; }
        public Nullable<decimal> CrossSellingFare { get; set; }
        public Nullable<int> DistanceCalStatusID { get; set; }
        public Nullable<double> CalcHiredDistance { get; set; }
        public Nullable<decimal> CalcFare { get; set; }
        public Nullable<double> CalcOncallDistance { get; set; }
        public Nullable<int> RideTime { get; set; }
        public Nullable<decimal> RideTimeFare { get; set; }
        public Nullable<decimal> SGST { get; set; }
        public Nullable<decimal> CGST { get; set; }
        public Nullable<decimal> IGST { get; set; }
        public Nullable<byte> TSOutCallStatus { get; set; }
        public Nullable<decimal> PWSCash { get; set; }
        public Nullable<double> AirportToll { get; set; }
        public Nullable<byte> Garage_Drop_Distance { get; set; }
    }
}
