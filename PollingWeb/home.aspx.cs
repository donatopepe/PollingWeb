using System;


public partial class home : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }



    protected void btnLetturafor_Click(object sender, EventArgs e)
    {
        Response.Redirect("letturafornitori.aspx", false);
    }
    protected void btnFatture_Click(object sender, EventArgs e)
    {
        Response.Redirect("Import_Fatture.aspx", false);
    }
    protected void btnVislog_Click(object sender, EventArgs e)
    {
        Response.Redirect("VisualizzaLogTaskService.aspx", false);
    }

    protected void btnTaskservice_Click(object sender, EventArgs e)
    {
        Response.Redirect("TaskService.aspx", false);
    }

    protected void btnOrdinisap_Click(object sender, EventArgs e)
    {
        Response.Redirect("ordini_sap.aspx", false);
    }

    protected void btnDayQuartz_Click(object sender, EventArgs e)
    {
        Response.Redirect("GestioneDayQuartz.aspx", false);
    }

    protected void btnBudget_Click(object sender, EventArgs e)
    {
        Response.Redirect("Import_Budget.aspx", false);
    }

    protected void btnTEST_Click(object sender, EventArgs e)
    {
     
       
        //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
        //IJobDetail job = JobBuilder.Create<JobFornitori>().Build();
        

        //IList<string> gruppi= scheduler.GetTriggerGroupNames();
        //IList<string> jobGroups = scheduler.GetJobGroupNames();
        //// IList<string> triggerGroups = scheduler.GetTriggerGroupNames();
        //dtsTabelle dts = new dtsTabelle();
        //DataRow dr;
        //DatabaseInfo dbInfo = new DatabaseInfo();
        //dbInfo.IniziaSessione();
        //TableAdapterListaTriggerTemp tbitrigger = new TableAdapterListaTriggerTemp(dbInfo, dts);
        //tbitrigger.Load(null);
        ////dts.ListaTriggerTemp.Rows.Clear();
        //foreach (DataRow   drc in dts.ListaTriggerTemp.Rows)        
        //    drc.Delete();
        
        //tbitrigger.SaveDataTable();
        //tbitrigger.ConfermaDataset();

        //foreach (string group in jobGroups)
        //{
        //    var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
        //    var jobKeys = scheduler.GetJobKeys(groupMatcher);
        //    foreach (var jobKey in jobKeys)
        //    {
        //        var detail = scheduler.GetJobDetail(jobKey);
        //        var triggers = scheduler.GetTriggersOfJob(jobKey);
        //        foreach (ITrigger triggern in triggers)
        //        {
        //            Console.WriteLine(group);
        //            Console.WriteLine(jobKey.Name);
        //            Console.WriteLine(detail.Description);
        //            Console.WriteLine(triggern.Key.Name);
        //            Console.WriteLine(triggern.Key.Group);
        //            Console.WriteLine(triggern.GetType().Name);
        //            Console.WriteLine(scheduler.GetTriggerState(triggern.Key));
        //            DateTimeOffset? nextFireTime = triggern.GetNextFireTimeUtc();
        //            if (nextFireTime.HasValue)
        //            {
        //                Console.WriteLine(nextFireTime.Value.LocalDateTime.ToString());
        //            }

        //            DateTimeOffset? previousFireTime = triggern.GetPreviousFireTimeUtc();
        //            if (previousFireTime.HasValue)
        //            {
        //                Console.WriteLine(previousFireTime.Value.LocalDateTime.ToString());
        //            }
        //            dr = dts.ListaTriggerTemp.NewListaTriggerTempRow();
        //            dr["Trigger_nome"] = triggern.Key.Name;
        //            dr["Trigger_next"] = triggern.GetNextFireTimeUtc().ToString();
        //            dr["Trigger_prev"] = triggern.GetPreviousFireTimeUtc().ToString();
        //            dts.ListaTriggerTemp.Rows.Add(dr);
        //            tbitrigger.SaveDataTable();
        //            tbitrigger.ConfermaDataset();
        //        }
        //    }
        //}
        //dbInfo.TerminaSessione();
        //Response.Redirect("listatrigger.aspx", false);
    }



    protected void btnSql01_Click(object sender, EventArgs e)
    {
        Response.Redirect("SQL01.aspx", false);
    }
}