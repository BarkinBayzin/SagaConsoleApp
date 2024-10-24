namespace SagaConsoleApp_v2.Entities
{
    public class CrmOpportunity
    {
        public Guid OpportunityId { get; private set; }
        public string GhTur { get; private set; }
        public Guid CustomerId { get; private set; }

        public static CrmOpportunity GetMockCrmOpportunity(string ghTur)
        {
            return new CrmOpportunity(Guid.NewGuid(), ghTur, Guid.NewGuid())
            {
                //OpportunityId = Guid.Parse("a5380152-636c-ef11-bfe2-0022489aad43"),
                //GhTur = "GHTUR--0010335-24",
                //CustomerId = Guid.Parse("400827f6-3440-ee11-bdf3-0022489bd701")
                //BuManagers = new List<CrmUser>
                //    {
                //new CrmUser
                //{
                //    UserId = "b3cb5a99-fbb7-ed11-83ff-0022489bd91f",
                //    AzureAdId = "8c1c87b5-1c89-49aa-b92b-09f0d478798c",
                //    FullName = "Hamdi Beşli",
                //    EmailAddress = "hamdi.besli@glasshouse.com.tr",
                //    Title = "Cloud BU Sales Manager"
                //},
                //new CrmUser
                //{
                //    UserId = "fb69d6c2-c5a7-ed11-aad1-0022489bd91f",
                //    AzureAdId ="67311cb1-bf32-4912-91a8-36f2c16b82ec",
                //    FullName = "Savaş Yıldırım",
                //    EmailAddress = "savas.yildirim@glasshouse.com.tr",
                //    Title = "Product Manager"
                //}
                //},
                //PreSales = new List<CrmUser>(),
                //PreSales = new List<CrmUser>(),
                //OpportunityId = Guid.Parse("a5380152-636c-ef11-bfe2-0022489aad43"),
                //GlassHouseId = "GHTUR--0010335-24",
                //Name = "Otomasyon Test GH4 renewal",
                //CustomerName = "ÇEKİNO AŞ - OTOMASYON TEST ŞİRKETİ",
                //TechnicalEndUserName = "ÇEKİNO AŞ - OTOMASYON TEST ŞİRKETİ",
                //TechnicalEndUserId = Guid.Parse("400827f6-3440-ee11-bdf3-0022489bd701"),
                //PricingId = "0962111f-6976-4862-0458-08dcce7e724f",
                //CommitmentTag = false,
                //Owner = new CrmUser
                //{
                //    UserId = "fb69d6c2-c5a7-ed11-aad1-0022489bd91f",
                //    AzureAdId = "67311cb1-bf32-4912-91a8-36f2c16b82ec",
                //    FullName = "Savaş Yıldırım",
                //    EmailAddress = "savas.yildirim@glasshouse.com.tr",
                //    Title = "Product Manager"
                //},
                //SalesType = new Salestype
                //{
                //    Id = 3,
                //    Name = "Renewal"
                //},
                //StateCode = new Statecode
                //{
                //    Id = 1,
                //    Name = "Won"
                //},
                //Stage = new Stage
                //{
                //    Id = 4,
                //    Name = "04- Commit Audit(Preparation)"
                //},
                //InitialOpportunityId = Guid.Parse("91c76205-596c-ef11-bfe2-0022489aad43"),
                //CustomerId = Guid.Parse("400827f6-3440-ee11-bdf3-0022489bd701"),
                //ContractId = "GHTUR--0010330-24",
                //OrderId = Guid.Parse("fb7248ca-666c-ef11-a670-00224899bfb5"),
                //SharepointPath = "https://ghtr.sharepoint.com/sites/crm-dev-test/account/ÇEKİNO AŞ - OTOMASYON TEST ŞİRKETİ_400827f63440ee11bdf30022489bd701/salesorder/Otomasyon Test GH4 renewal_fb7248ca666cef11a67000224899bfb5",
                //HasPaymentPlan = true,
                //TechnicalEndUserAccountId = "ACC-02324-H7F5",
                //CustomerAccountId = "ACC-02324-H7F5",
                //IsCloudProject = null // or true/false
                // };
                //};     
            };
        }
        public CrmOpportunity(Guid opportunityId, string ghTur, Guid customerId) 
        {
            OpportunityId = opportunityId;
            GhTur = ghTur;
            CustomerId = customerId;
        }
    }
}
