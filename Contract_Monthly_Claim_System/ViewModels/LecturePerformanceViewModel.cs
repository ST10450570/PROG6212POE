namespace Contract_Monthly_Claim_System.ViewModels
{
    public class LecturerPerformanceViewModel
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageHours { get; set; }
        public double SuccessRate { get; set; }
    }
}