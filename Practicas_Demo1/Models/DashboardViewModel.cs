public class DashboardViewModel
{
    public List<RequestStatusData> RequestStatusData { get; set; }
    public List<GroupConsumptionData> GroupConsumptionData { get; set; }
    public List<DailyRequestsData> DailyRequestsData { get; set; }
    public decimal GrandTotalAmount { get; set; }
}

public class RequestStatusData
{
    public string Status { get; set; }
    public int TotalRequests { get; set; }
}

public class GroupConsumptionData
{
    public string FisGroup { get; set; }
    public decimal TotalAmount { get; set; }
}

public class DailyRequestsData
{
    public DateTime RequestDate { get; set; }
    public int TotalRequests { get; set; }
}