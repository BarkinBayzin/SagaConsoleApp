namespace SagaConsoleApp_v2.Services
{
    public class CrmIntegrationOptions
    {
        public const string SectionName = "CrmIntegrationOptions";

        public const string TagName = "CRMOptions";

        public string Header { get; set; } = "X-Api-Key";

        public string Host { get; set; } = "http://....../api";
        public string Users { get; set; }

        public string Me { get; set; }

        public string GHTur { get; set; }

        public string SubmitToCrm { get; set; } = "/Opportunities/{0}/Submit";
        public string OpportunityPrincipal { get; set; }

        public string Reopen { get; set; }

        public string GHTur_getById { get; set; }

        public bool OverrideUser { get; set; }

        public string OverrideUserId { get; set; } = Guid.NewGuid().ToString();
        public string AutomationOpportunities { get; set; }

        public string Upgrade { get; set; }

        public string GetSubmitToCrmEndPoint(string ghTur)
        {
            return $"{this.Host}{string.Format(this.SubmitToCrm, ghTur)}";
        }
        // Diğer metotlar...
    }

}
