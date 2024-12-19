namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation
{
    public class SecurityNamespaceIds
    {
        public static readonly HashSet<SecurityNamespaceId> All = new HashSet<SecurityNamespaceId>();

        private static SecurityNamespaceId ToId(string namespaceId)
        {
            var val = new SecurityNamespaceId(Guid.Parse(namespaceId));

            if (All.Contains(val) == false)
            {
                All.Add(val);
            }

            return val;
        }

        public static readonly SecurityNamespaceId AccountAdminSecurity = ToId("11238E09-49F2-40C7-94D0-8F0307204CE4");
        public static readonly SecurityNamespaceId Analytics = ToId("58450C49-B02D-465A-AB12-59AE512D6531");
        public static readonly SecurityNamespaceId AnalyticsViews = ToId("D34D3680-DFE5-4CC6-A949-7D9C68F73CBA");
        public static readonly SecurityNamespaceId AuditLog = ToId("A6CC6381-A1CA-4B36-B3C1-4E65211E82B6");
        public static readonly SecurityNamespaceId Boards = ToId("251E12D9-BEA3-43A8-BFDB-901B98C0125E");
        public static readonly SecurityNamespaceId BoardsExternalIntegration = ToId("5AB15BC8-4EA1-D0F3-8344-CAB8FE976877");
        public static readonly SecurityNamespaceId Build = ToId("33344D9C-FC72-4D6F-ABA5-FA317101A7E9");
        public static readonly SecurityNamespaceId BuildAdministration = ToId("302ACACA-B667-436D-A946-87133492041C");
        public static readonly SecurityNamespaceId Chat = ToId("BC295513-B1A2-4663-8D1A-7017FD760D18");
        public static readonly SecurityNamespaceId Collection = ToId("3E65F728-F8BC-4ECD-8764-7E378B19BFA7");
        public static readonly SecurityNamespaceId CrossProjectWidgetView = ToId("093CBB02-722B-4AD6-9F88-BC452043FA63");
        public static readonly SecurityNamespaceId CSS = ToId("83E28AD4-2D72-4CEB-97B0-C7726D5502C3");
        public static readonly SecurityNamespaceId DashboardsPrivileges = ToId("8ADF73B7-389A-4276-B638-FE1653F7EFC7");
        public static readonly SecurityNamespaceId DataProvider = ToId("7FFA7CF4-317C-4FEA-8F1D-CFDA50CFA956");
        public static readonly SecurityNamespaceId DiscussionThreads = ToId("0D140CAE-8AC1-4F48-B6D1-C93CE0301A12");
        public static readonly SecurityNamespaceId DistributedTask = ToId("101EAE8C-1709-47F9-B228-0E476C35B3BA");
        public static readonly SecurityNamespaceId Environment = ToId("83D4C2E6-E57D-4D6E-892B-B87222B7AD20");
        public static readonly SecurityNamespaceId EventPublish = ToId("7CD317F2-ADC6-4B6C-8D99-6074FAEAF173");
        public static readonly SecurityNamespaceId EventSubscriber = ToId("2BF24A2B-70BA-43D3-AD97-3D9E1F75622F");
        public static readonly SecurityNamespaceId EventSubscription = ToId("58B176E7-3411-457A-89D0-C6D0CCB3C52B");
        public static readonly SecurityNamespaceId Favorites = ToId("FA557B48-B5BF-458A-BB2B-1B680426FE8B");
        public static readonly SecurityNamespaceId GitRepositories = ToId("2E9EB7ED-3C0A-47D4-87C1-0FFDD275FD87");
        public static readonly SecurityNamespaceId Graph = ToId("C2EE56C9-E8FA-4CDD-9D48-2C44F697A58E");
        public static readonly SecurityNamespaceId Identity = ToId("5A27515B-CCD7-42C9-84F1-54C998F03866");
        public static readonly SecurityNamespaceId IdentityPicker = ToId("A60E0D84-C2F8-48E4-9C0C-F32DA48D5FD1");
        public static readonly SecurityNamespaceId Iteration = ToId("BF7BFA03-B2B7-47DB-8113-FA2E002CC5B1");
        public static readonly SecurityNamespaceId Job = ToId("2A887F97-DB68-4B7C-9AE3-5CEBD7ADD999");
        public static readonly SecurityNamespaceId Library = ToId("B7E84409-6553-448A-BBB2-AF228E07CBEB");
        public static readonly SecurityNamespaceId Location = ToId("2725D2BC-7520-4AF4-B0E3-8D876494731F");
        public static readonly SecurityNamespaceId MetaTask = ToId("F6A4DE49-DBE2-4704-86DC-F8EC1A294436");
        public static readonly SecurityNamespaceId OrganizationLevelData = ToId("F0003BCE-5F45-4F93-A25D-90FC33FE3AA9");
        public static readonly SecurityNamespaceId PipelineCachePrivileges = ToId("62A7AD6B-8B8D-426B-BA10-76A7090E94D5");
        public static readonly SecurityNamespaceId Plan = ToId("BED337F8-E5F3-4FB9-80DA-81E17D06E7A8");
        public static readonly SecurityNamespaceId Process = ToId("2DAB47F9-BD70-49ED-9BD5-8EB051E59C02");
        public static readonly SecurityNamespaceId Project = ToId("52D39943-CB85-4D7F-8FA8-C6BAAC873819");
        public static readonly SecurityNamespaceId ProjectAnalysisLanguageMetrics = ToId("FC5B7B85-5D6B-41EB-8534-E128CB10EB67");
        public static readonly SecurityNamespaceId Proxy = ToId("CB4D56D2-E84B-457E-8845-81320A133FBB");
        public static readonly SecurityNamespaceId Registry = ToId("4AE0DB5D-8437-4EE8-A18B-1F6FB38BD34C");
        public static readonly SecurityNamespaceId ReleaseManagement = ToId("C788C23E-1B46-4162-8F5E-D7585343B5DE");
        // not sure why there's two ReleaseManagement values...
        public static readonly SecurityNamespaceId ReleaseManagement2 = ToId("7C7D32F7-0E86-4CD6-892E-B35DBBA870BD");
        public static readonly SecurityNamespaceId Security = ToId("9A82C708-BFBE-4F31-984C-E860C2196781");
        public static readonly SecurityNamespaceId Server = ToId("1F4179B3-6BAC-4D01-B421-71EA09171400");
        public static readonly SecurityNamespaceId ServiceEndpoints = ToId("49B48001-CA20-4ADC-8111-5B60C903A50C");
        public static readonly SecurityNamespaceId ServiceHooks = ToId("CB594EBE-87DD-4FC9-AC2C-6A10A4C92046");
        public static readonly SecurityNamespaceId ServicingOrchestration = ToId("84CC1AA4-15BC-423D-90D9-F97C450FC729");
        public static readonly SecurityNamespaceId SettingEntries = ToId("6EC4592E-048C-434E-8E6C-8671753A8418");
        public static readonly SecurityNamespaceId Social = ToId("81C27CC8-7A9F-48EE-B63F-DF1E1D0412DD");
        public static readonly SecurityNamespaceId StrongBox = ToId("4A9E8381-289A-4DFD-8460-69028EAA93B3");
        public static readonly SecurityNamespaceId Tagging = ToId("BB50F182-8E5E-40B8-BC21-E8752A1E7AE2");
        public static readonly SecurityNamespaceId TeamLabSecurity = ToId("9E4894C3-FF9A-4EAC-8A85-CE11CAFDC6F1");
        public static readonly SecurityNamespaceId TestManagement = ToId("E06E1C24-E93D-4E4A-908A-7D951187B483");
        public static readonly SecurityNamespaceId UtilizationPermissions = ToId("83ABDE3A-4593-424E-B45F-9898AF99034D");
        public static readonly SecurityNamespaceId VersionControlItems = ToId("A39371CF-0841-4C16-BBD3-276E341BC052");
        public static readonly SecurityNamespaceId VersionControlItems2 = ToId("3C15A8B7-AF1A-45C2-AA97-2CB97078332E");
        public static readonly SecurityNamespaceId VersionControlPrivileges = ToId("66312704-DEB5-43F9-B51C-AB4FF5E351C3");
        public static readonly SecurityNamespaceId ViewActivityPaneSecurity = ToId("DC02BF3D-CD48-46C3-8A41-345094ECC94B");
        public static readonly SecurityNamespaceId WebPlatform = ToId("0582EB05-C896-449A-B933-AA3D99E121D6");
        public static readonly SecurityNamespaceId WorkItemQueryFolders = ToId("71356614-AAD7-4757-8F2C-0FB3BFF6F680");
        public static readonly SecurityNamespaceId WorkItemsHub = ToId("C0E7A722-1CAD-4AE6-B340-A8467501E7CE");
        public static readonly SecurityNamespaceId WorkItemTracking = ToId("73E71C45-D483-40D5-BDBA-62FD076F7F87");
        public static readonly SecurityNamespaceId WorkItemTrackingAdministration = ToId("445D2788-C5FB-4132-BBEF-09C4045AD93F");
        public static readonly SecurityNamespaceId WorkItemTrackingConfiguration = ToId("35E35E8E-686D-4B01-AFF6-C369D6E36CE0");
        public static readonly SecurityNamespaceId WorkItemTrackingProvision = ToId("5A6CD233-6615-414D-9393-48DBB252BD23");
        public static readonly SecurityNamespaceId Workspaces = ToId("93BAFC04-9075-403A-9367-B7164EAC6B5C");
    }
}
