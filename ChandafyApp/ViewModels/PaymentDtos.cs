namespace ChandafyApp.ViewModels;


public class CreatePaymentDto
{
    public List<ChandaTypeDto>? ChandaTypes { get; set; }
    public List<ChandaAmount> ChandaAmounts { get; set; }
}

public class PaymentSummaryDto
{
    public List<PaymentMethodDto> PaymentMethods { get; set; }
    public List<ChandaAmount> ChandaAmounts { get; set; }
    public List<ChandaSummaryItem> SummaryItems { get; set; }
}


public class PaymentMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Rule { get; set; }
}

public class ChandaTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ChandaAmount
{
    public int ChandaTypeId { get; set; }   // <-- Add this
    public double Amount { get; set; }
}

public class ChandaSummaryItem
{
    public string Name { get; set; }
    public double Amount { get; set; }
}