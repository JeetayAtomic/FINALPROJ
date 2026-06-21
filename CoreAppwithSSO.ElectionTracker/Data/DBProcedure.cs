namespace CoreAppwithSSO.ElectionTracker.Data
{
    public static class DBProcedure
    {
        public const string GetBoothList = "[ELT].[GetBoothList]";
        public const string SearchBoothByFilter = "[ELT].[BoothSearch]";

        public const string GetWardList = "[ELT].[GetWardList]";
        public const string SearchWardByFilter = "[ELT].[WardSearch]";

        public const string GetSectorList = "[ELT].[GetSectorList]";
        public const string SearchSectorByFilter = "[ELT].[SectorSearch]";

        public const string GetConstituencyList = "[ELT].[GetConstituencyList]";
        public const string SearchConstituencyByFilter = "[ELT].[ConstituencySearch]";
    }
}
