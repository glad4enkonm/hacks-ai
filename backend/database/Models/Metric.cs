using Dapper.Contrib.Extensions;

namespace database.Models;

[Table("`Metric`")]
public class Metric: IEntity
{
    [Key]
    public ulong MetricId { get; set; }
    public DateTime CalculationDate { get; set; }
    public string Loaction { get; set; }
    public decimal PositiveOverNegative { get; set; }
    public decimal EducationalDistance { get; set; }
    public decimal CityDistrictMetric { get; set; }
    public decimal ReserectionalDistance { get; set; }
    public ulong GetId ()
    {
        return MetricId;
    }
}
