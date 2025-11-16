namespace MaterialQ.Models.DataModels
{
    public class RFQModel
    {
        public string FromCompany { get; set; }
        public string ToCompany { get; set; }
        public List<RFQItemModel> Items { get; set; }
    }
}
