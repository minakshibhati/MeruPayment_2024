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
    
    public partial class tblTripReceiptCalculator
    {
        public int SeqID { get; set; }
        public int TripID { get; set; }
        public int JobID { get; set; }
        public string TripType { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public Nullable<System.DateTime> TripStartTime { get; set; }
        public Nullable<System.DateTime> TripEndTime { get; set; }
        public string PickupLocation { get; set; }
        public string DropLocation { get; set; }
        public string SiebelSubscriberID { get; set; }
        public string CabRegistration { get; set; }
        public Nullable<decimal> MeteredFare { get; set; }
        public Nullable<decimal> ParkingCharges { get; set; }
        public Nullable<decimal> ConvenienceCharges { get; set; }
        public Nullable<decimal> RedeemAmount { get; set; }
        public Nullable<decimal> CouponAmount { get; set; }
        public Nullable<decimal> ServiceTaxOnTripFare { get; set; }
        public Nullable<decimal> ServiceTaxOnOthers { get; set; }
        public Nullable<decimal> TotalServiceTax { get; set; }
        public Nullable<decimal> TotalTripAmount { get; set; }
        public string CustomerPaidAmount { get; set; }
        public string ProductType { get; set; }
        public Nullable<System.DateTime> RecordedDate { get; set; }
        public int Status { get; set; }
        public Nullable<System.DateTime> LastUpdatedate { get; set; }
        public string UpdateRemarks { get; set; }
        public Nullable<decimal> TotalDiscountAmount { get; set; }
        public Nullable<decimal> AfterDiscountNetAmount { get; set; }
        public string BrandTypeName { get; set; }
        public Nullable<decimal> TollCharges { get; set; }
        public Nullable<decimal> Surcharges { get; set; }
        public string CustomerEmail { get; set; }
        public Nullable<System.DateTime> BookingTime { get; set; }
        public Nullable<System.DateTime> PickUpTime { get; set; }
        public string CustomerPickUpAddress { get; set; }
        public string DestinationAddress { get; set; }
        public Nullable<double> HiredDistance { get; set; }
        public Nullable<double> OncallDistance { get; set; }
        public string PaymentMode { get; set; }
        public Nullable<decimal> WaitingFare { get; set; }
        public Nullable<double> ShadowAmount { get; set; }
        public string JobBrand { get; set; }
        public Nullable<System.DateTime> CabAssignedTime { get; set; }
        public string JobTypeDes { get; set; }
        public string TripTypeDes { get; set; }
        public Nullable<int> PragathiStatus { get; set; }
        public Nullable<System.DateTime> PragathiLastUpdatedate { get; set; }
        public string PragathiUpdateRemarks { get; set; }
        public string CorporateCode { get; set; }
        public Nullable<byte> IsRideShare { get; set; }
        public Nullable<int> MatchedJobID { get; set; }
        public Nullable<byte> RSTripSequence { get; set; }
        public Nullable<byte> Retriggered { get; set; }
        public Nullable<decimal> CrossSellingFare { get; set; }
        public Nullable<decimal> DriverPartnerPayment { get; set; }
        public string CorporateCompanyName { get; set; }
        public Nullable<decimal> ActualRunningFare { get; set; }
        public Nullable<decimal> MeruShare { get; set; }
        public Nullable<decimal> TMPDifferenceAmount { get; set; }
        public string TMPNoShowCar { get; set; }
        public string TMPNoShowDriver { get; set; }
        public Nullable<decimal> TMPBookingFare { get; set; }
        public Nullable<decimal> TMPDispatchFare { get; set; }
        public Nullable<int> WaitingTimeInMinutes { get; set; }
        public Nullable<decimal> CustomerPaidInCash { get; set; }
        public string BookingType { get; set; }
        public Nullable<int> NoOfEmp { get; set; }
        public string TollURL { get; set; }
        public string DutySlipURL { get; set; }
        public string EmpShift { get; set; }
        public string MonroeTripType { get; set; }
    }
}
