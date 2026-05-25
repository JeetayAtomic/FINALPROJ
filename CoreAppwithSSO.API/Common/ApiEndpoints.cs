namespace CoreAppwithSSO.API.Common
{
    public class ApiEndpoints
    {
        public const string Clients = "/shared/tmc/clients";
        public const string DangeroursGood = "/shared/tmc/dangerousgoods";
        public const string Servicelevels = "/shared/tmc/servicelevels";
        public const string Shipinstructions = "/shared/tmc/shipinstructions";
        public const string OrderBill = "/shared/tmc/orders?bill";
        public const string TraceBill = "/shared/tmc/traces?bill";
        public const string SealsBill = "/shared/tmc/seals?bill";
        public const string UpsertPreOrder = "/shared/tmc/opp";
        public const string UpsertPreOrderDetails = "/shared/tmc/opp/details";
        public const string UpsertPreOrderShipInstructions = "/shared/tmc/opp/shipinstructions";
    }
}
