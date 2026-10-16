CREATE TABLE [dbo].[TaskService] (
    [Istituto]           SMALLINT       NOT NULL,
    [IdProcesso]         NVARCHAR (20)  NOT NULL,
    [Descrizione]        NVARCHAR (20)  NULL,
    [Path_Source]        NVARCHAR (100) NULL,
    [File_Source]        NVARCHAR (50)  NULL,
    [Orario_Check_File]  DATETIME       NULL,
    [Day_Check_File]     NCHAR (10)     NULL,
    [Polling_Intervall]  SMALLINT       NULL,
    [Cutoff]             SMALLINT       NULL,
    [Mail_Mittente]      NVARCHAR (50)  NULL,
    [Mail_Failure]       NVARCHAR (250) NULL,
    [Path_Archiviazione] NVARCHAR (50)  NULL,
    [Mail_success]       NVARCHAR (250) NULL,
    [LasciaInSource]     BIT            NULL,
    CONSTRAINT [PK_TaskService_1] PRIMARY KEY CLUSTERED ([Istituto] ASC, [IdProcesso] ASC)
);

