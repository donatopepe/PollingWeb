CREATE TABLE [dbo].[ListaTriggerTemp] (
    [Trigger_nome] NVARCHAR (50) NOT NULL,
    [Trigger_next] NVARCHAR (50) NULL,
    [Trigger_prev] NVARCHAR (50) NULL,
    CONSTRAINT [PK_ListaTriggerTemp] PRIMARY KEY CLUSTERED ([Trigger_nome] ASC)
);

